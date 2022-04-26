using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Crawlers;

namespace JTLanguageModelsPortable.Formats
{
    public partial class FormatPassage : Format
    {
        // Arguments.

        public int MaxTranslationCount { get; set; }
        public static string MaxTranslationCountPrompt = "Maximum translations per word";
        public static string MaxTranslationCountHelp = "Set this to the maximum translations per word (default 5 (0=disabled)).";
        public static int DefaultMaxTranslationCount = 5;

        public int ExclusionRatio { get; set; }
        public static string ExclusionRatioPrompt = "Exclusion ratio";
        public static string ExclusionRatioHelp = "Set this to filter out translations with a frequency this times less than the maximum (default 100 (0=disabled)).";
        public static int DefaultExclusionRatio = 12; // Was 100, then 24, now 12.

        public int MaxTranslationWords { get; set; }
        public static string MaxTranslationWordsPrompt = "Max translation words";
        public static string MaxTranslationWordsHelp = "Set this to filter out translations with more than this many words (default 3 (0=disabled)).";
        public static int DefaultMaxTranslationWords = 4;

        public bool TextUnitsSentences { get; set; }
        public static string TextUnitsSentencesPrompt = "TextUnits are sentences";
        public static string TextUnitsSentencesHelp = "Set this to true to map sentences to text units (default true).";
        public static bool DefaultTextUnitsSentences = true;

        public bool ShowTranslation { get; set; }
        public static string ShowTranslationPrompt = "Show native translation";
        public static string ShowTranslationHelp = "If checked, show native translation.";
        public static bool DefaultShowTranslation = true;

        public bool ShowStatistics { get; set; }
        public static string ShowStatisticsPrompt = "Show statistics";
        public static string ShowStatisticsHelp = "If checked, show statistics.";
        public static bool DefaultShowStatistics = false;

        public bool ShowGlossary { get; set; }
        public static string ShowGlossaryPrompt = "Show glossary";
        public static string ShowGlossaryHelp = "If checked, show glossary.";
        public static bool DefaultShowGlossary = false;

        public bool ShowJSON { get; set; }
        public static string ShowJSONPrompt = "Show JSON";
        public static string ShowJSONHelp = "If checked, show JSON.";
        public static bool DefaultShowJSON = false;

        public string WordAudioMode { get; set; }
        public static string WordAudioModePrompt = "Word audio mode";
        public static string WordAudioModeHelp = "Selects how word audio is handled."
            + "\nNone: Don't do word audio at all."
            + "\nPreload: Ensure all audio is available, even if it needs to be synthesized."
            + "\nMissingLazy: Use existing audio. If missing, download it on demand."
            + "\nAllLazy: All audio references processed on demand.";
        public static string DefaultWordAudioMode = "AllLazy";
        public static List<string> WordAudioModeStrings = new List<string>
        {
            "None",
            "Preload",
            "MissingLazy",
            "AllLazy"
        };

        // Inherited from Format.
        //public bool IsSynthesizeMissingAudio { get; set; }
        public static string IsSynthesizeMissingAudioPrompt = "Synthesize missing audio";
        //public static string IsSynthesizeMissingAudioHelp = "If checked, will synthesize missing audio, if not found in dictionary.";
        public static bool DefaultIsSynthesizeMissingAudio = true;

        public bool IsIncludeRejectedAudio { get; set; }
        public static string IsIncludeRejectedAudioPrompt = "Include rejected audio";
        public static string IsIncludeRejectedAudioHelp = "If checked, include rejected audio.";
        public static bool DefaultIsIncludeRejectedAudio = false;

        protected List<string> AudioTags { get; set; }
        protected static string AudioTagsPrompt = "Audio tags";
        protected static string AudioTagsHelp = "Enter a comma-separated lists of optional audio tags to use as a filter.";

        protected string AudioAddFilePath { get; set; }
        protected static string AudioAddFilePathPrompt = "Add audio file path";
        protected string AudioAddFilePathHelp = "Enter the file path for a list of audio files to add.";

        protected string AudioRejectFilePath { get; set; }
        protected static string AudioRejectFilePathPrompt = "Reject audio file path";
        protected string AudioRejectFilePathHelp = "Enter the file path for a list of audio files to reject.";

        public bool IsNoGlossary { get; set; }
        public static string IsNoGlossaryPrompt = "Don't use glossary";
        public static string IsNoGlossaryHelp = "If checked, won't use glossary.";
        public static bool DefaultIsNoGlossary = false;

        // Non-argument inputs.
        public string TargetText { get; set; }
        public string HostText { get; set; }
        public Dictionary<string, DictionaryEntry> Glossary { get; set; }   // Also output, new glossary entries added.
        public List<string> Phrases { get; set; }
        public bool IsScripture { get; set; }

        // Outputs.
        public string Module { get; set; }
        public int TotalUniqueWordCount { get; set; }               // The number of unique words.
        public int FoundInGlossaryCount { get; set; }               // The number of words found in the global glossary.
        public int FoundInDictionaryCount { get; set; }             // The number of words found in the dictionary.
        public int RecognizedAsInflectionCount { get; set; }        // The number of words recognized as inflected.
        public int GoogleTranslateCount { get; set; }               // The number of words requiring Google Translate.
        public int TranslatorCacheCount { get; set; }               // The number of words from the translator cache or other means.
        public int LookupDictionaryCount { get; set; }              // The number of words found in the dictionary.
        public int LookupInflectionCount { get; set; }              // The number of words recognized as inflected.
        public int LookupGoogleTranslateCount { get; set; }         // The number of words requiring Google Translate.
        public int LookupTranslatorCacheCount { get; set; }         // The number of words from the translator cache or other means.
        public float AverageDictionaryLookupTime { get; set; }      // The average dictionary lookup time.
        public float AverageInflectionLookupTime { get; set; }      // The average recognize-as-inflection time.
        public float AverageGoogleTranslateTime { get; set; }       // The average Google Translate time.
        public float AverageTranslatorCacheTime { get; set; }       // The average translator cache lookup time.
        public double SumDictionaryLookupTime { get; set; }         // The sum of the dictionary lookup time.
        public double SumInflectionLookupTime { get; set; }         // The sum of the recognize-as-inflection time.
        public double SumGoogleTranslateTime { get; set; }          // The sum of the Google Translate time.
        public double SumTranslatorCacheTime { get; set; }          // The sum of the translator cache lookup time.

        // Implementation.
        protected ContentStudyList StudyList { get; set; }
        protected List<MultiLanguageItem> StudyItems { get; set; }
        protected Dictionary<string, GlossaryData> GlossaryDataCache;
        protected LanguageTool TargetTool { get; set; }
        protected LanguageTool HostTool { get; set; }
        protected List<string> SourceNames { get; set; }
        protected Dictionary<string, int> SourceToIndexMap { get; set; }
        protected SoftwareTimer GlossaryLookupTimer;

        // List of audio files to add.
        protected List<string> AudioAddFileNames;

        // List of audio files to reject.
        protected List<string> AudioRejectFileNames;

        // Dictionary of audio rejects.
        protected Dictionary<string, List<List<Audio>>> AudioRejects;

        // Directory for reject audio .mp3 files.
        protected string OutputLanguageAudioRejectDirectoryPath;

        // Audio reject JSON file path.
        protected string OutputAudioRejectFilePath;

        private static string FormatDescription = "Format for importing text passages and outputting Embark Json text.";

        public FormatPassage()
            : base(
                  "Passage",
                  "FormatPassage",
                  FormatDescription,
                  String.Empty,
                  String.Empty,
                  "text/plain",
                  ".txt",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
            ClearFormatPassage();
        }

        public FormatPassage(
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(
                  "Passage",
                  "FormatPassage",
                  FormatDescription,
                  String.Empty,
                  String.Empty,
                  "text/plain",
                  ".txt",
                  userRecord,
                  userProfile,
                  repositories,
                  languageUtilities,
                  nodeUtilities)
        {
            ClearFormatPassage();
        }

        public FormatPassage(FormatPassage other)
            : base(other)
        {
            CopyFormatPassage(other);
        }

        public FormatPassage(
            string name,
            string type,
            string description,
            string targetType,
            string importExportType,
            string mimeType,
            string defaultExtension,
            UserRecord userRecord,
            UserProfile userProfile,
            IMainRepository repositories,
            LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatPassage();
        }

        public void ClearFormatPassage()
        {
            // Local parameters.
            MaxTranslationCount = DefaultMaxTranslationCount;
            ExclusionRatio = DefaultExclusionRatio;
            TextUnitsSentences = DefaultTextUnitsSentences;
            MaxTranslationWords = DefaultMaxTranslationWords;
            ShowTranslation = DefaultShowTranslation;
            ShowStatistics = DefaultShowStatistics;
            ShowGlossary = DefaultShowGlossary;
            ShowJSON = DefaultShowJSON;
            WordAudioMode = DefaultWordAudioMode;
            IsSynthesizeMissingAudio = DefaultIsSynthesizeMissingAudio;
            IsIncludeRejectedAudio = false;
            AudioTags = null;
            AudioAddFilePath = String.Empty;
            AudioRejectFilePath = String.Empty;
            IsNoGlossary = DefaultIsNoGlossary;

            // Non-argument inputs.
            TargetText = String.Empty;
            HostText = String.Empty;
            Glossary = null;
            Phrases = null;
            IsScripture = false;

            // Outputs.
            Module = String.Empty;
            TotalUniqueWordCount = 0;
            FoundInGlossaryCount = 0;
            FoundInDictionaryCount = 0;
            RecognizedAsInflectionCount = 0;
            GoogleTranslateCount = 0;
            TranslatorCacheCount = 0;
            AverageDictionaryLookupTime = 0.0f;
            AverageInflectionLookupTime = 0.0f;
            AverageGoogleTranslateTime = 0.0f;
            AverageTranslatorCacheTime = 0.0f;
            LookupDictionaryCount = 0;
            LookupInflectionCount = 0;
            LookupGoogleTranslateCount = 0;
            LookupTranslatorCacheCount = 0;
            SumDictionaryLookupTime = 0.0;
            SumInflectionLookupTime = 0.0;
            SumGoogleTranslateTime = 0.0;
            SumTranslatorCacheTime = 0.0;

            // Implementation.
            StudyList = null;
            StudyItems = null;
            GlossaryDataCache = null;
            TargetTool = null;
            HostTool = null;
            SourceNames = new List<string>();
            SourceToIndexMap = new Dictionary<string, int>();
            GlossaryLookupTimer = null;
            AudioAddFileNames = null;
            AudioRejectFileNames = null;
            AudioRejects = new Dictionary<string, List<List<Audio>>>(StringComparer.OrdinalIgnoreCase);

            // Base parameters.

            DefaultContentType = "Text";
            DefaultContentSubType = "Text";
            Label = "Text";
        }

        public void CopyFormatPassage(FormatPassage other)
        {
            ClearFormatPassage();
        }

        public override Format Clone()
        {
            return new FormatPassage(this);
        }

        protected void InitializeTools()
        {
            TargetTool = NodeUtilities.GetLanguageTool(TargetLanguageID);
            HostTool = NodeUtilities.GetLanguageTool(HostLanguageID);

            if ((TargetTool != null) && (TargetTool.MultiTool == null))
            {
                MultiLanguageTool multiTool = NodeUtilities.GetMultiLanguageTool(
                    UserProfile.TargetLanguageID,
                    UserProfile.HostLanguageIDs);

                TargetTool.MultiTool = multiTool;
            }

            if ((HostTool != null) && (HostTool.MultiTool == null))
            {
                MultiLanguageTool multiTool = TargetTool.MultiTool;

                HostTool.MultiTool = multiTool;
            }
        }

        public bool LoadGlossary(string filePath, out string errorMessage)
        {
            errorMessage = null;

            InitializeTools();

            if (TargetTool == null)
            {
                errorMessage = "Sorry, there currently is no language tool support for the current target language: "
                    + TargetLanguageID.LanguageName(HostLanguageID);
                PutError(errorMessage);
                return false;
            }

            if (HostTool == null)
            {
                errorMessage = "Sorry, there currently is no language tool support for the current host language: "
                    + HostLanguageID.LanguageName(HostLanguageID);
                PutError(errorMessage);
                return false;
            }

            Glossary = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);

            if (!FileSingleton.Exists(filePath))
            {
#if true
                errorMessage = "Note: There is no glossary for the current target language: "
                    + TargetLanguageID.LanguageName(HostLanguageID)
                    + "\nWe will just do word lookups with the dictionary (if any) or Google Translate.";
                PutMessage(errorMessage);
                return true;
#else
                errorMessage = "Sorry, unable to find at glossary in the following place: "
                    + filePath
                    + "\nThis might be due to an expected language not being set up in the current user profile, "
                    + "or that the word alignment pipeline has not been run for the current languages.\n"
                    + "The current target language is: "
                    + TargetLanguageID.LanguageName(HostLanguageID)
                    + "\nThe current host language is: "
                    + HostLanguageID.LanguageName(HostLanguageID);
                PutError(errorMessage);
                return false;
#endif
            }

            try
            {
                string[] lines = FileSingleton.ReadAllLines(
                    filePath,
                    ApplicationData.Encoding);
                int count = lines.Length;
                int index;
                int sourceID = PassageGlossarySourceID;
                DictionaryEntry entry = null;
                List<Sense> senses = null;
                Sense sense = null;
                int reading = 0;
                LexicalCategory category = LexicalCategory.Unknown;
                string categoryString = null;
                int priorityLevel = 0;
                List<LanguageSynonyms> languageSynonymsList = null;
                LanguageSynonyms languageSynonyms = null;
                List<ProbableMeaning> probableSynonyms = null;
                ProbableMeaning probableSynonym = null;

                for (index = 1; index < count; index++)
                {
                    string line = lines[index];
                    string[] parts = line.Split(LanguageLookup.Tab);
                    string word = parts[0];
                    string translation = parts[1];
                    int translationNumber = ObjectUtilities.GetIntegerFromString(parts[2], -1);
                    string lemma = parts[3];
                    string translationLemma = parts[4];
                    string inflectionType = parts[5];
                    int fitFrequency = ObjectUtilities.GetIntegerFromString(parts[6], 0);
                    int wordFreqInWork = ObjectUtilities.GetIntegerFromString(parts[7], 0);
                    Designator designator;
                    Language.Inflection inflection;
                    List<MultiLanguageString> examples = null;

                    probableSynonym = new ProbableMeaning(
                        translation,
                        category,
                        categoryString,
                        1.0f,
                        fitFrequency,
                        sourceID);

                    if (!String.IsNullOrEmpty(inflectionType))
                    {
                        designator = TargetTool.GetDesignator(category.ToString(), inflectionType);

                        if (designator != null)
                        {
                            DictionaryEntry lemmaEntry = null;
                            MultiLanguageString dictionaryForm = null;

                            if (!String.IsNullOrEmpty(lemma))
                            {
                                lemmaEntry = new DictionaryEntry(lemma, TargetLanguageID);
                                ProbableMeaning lemmaMeaning = new ProbableMeaning(
                                    translationLemma,
                                    category,
                                    categoryString,
                                    0.0f,
                                    0,
                                    0);
                                List<ProbableMeaning> lemmaSynonyms = new List<ProbableMeaning>() { lemmaMeaning };
                                LanguageSynonyms lemmaLanguageSynonymns = new LanguageSynonyms(
                                    HostLanguageID,
                                    lemmaSynonyms);
                                List<LanguageSynonyms> lemmaLanguageSynonymnsList = new List<LanguageSynonyms>() { lemmaLanguageSynonymns };
                                Sense lemmaSense = new Sense(
                                    0,
                                    category,
                                    categoryString,
                                    0,
                                    lemmaLanguageSynonymnsList,
                                    null);
                                lemmaEntry.AddSense(lemmaSense);
                                dictionaryForm = new MultiLanguageString(null, UserProfile.TargetHostLanguageIDs);
                                dictionaryForm.SetText(TargetLanguageID, lemma);
                                dictionaryForm.SetText(HostLanguageID, translationLemma);
                            }

                            inflection = new Language.Inflection(
                                lemmaEntry,
                                designator,
                                UserProfile.TargetHostLanguageIDs);

                            inflection.DictionaryForm = dictionaryForm;

                            List<Language.Inflection> synonymInflections = new List<Language.Inflection>() { inflection };

                            probableSynonym.Inflections = synonymInflections;
                        }
                    }

                    if (translationNumber == 1)
                    {
                        if (String.IsNullOrEmpty(translation))
                            continue;

                        probableSynonyms = new List<ProbableMeaning>();
                        if (!String.IsNullOrEmpty(translation))
                            probableSynonyms.Add(probableSynonym);
                        languageSynonyms = new LanguageSynonyms(HostLanguageID, probableSynonyms);
                        languageSynonymsList = new List<LanguageSynonyms>() { languageSynonyms };
                        sense = new Sense(
                            reading,
                            category,
                            categoryString,
                            priorityLevel,
                            languageSynonymsList,
                            examples);
                        senses = new List<Sense>() { sense };
                        entry = new DictionaryEntry(
                            word,
                            TargetLanguageID,
                            null,
                            wordFreqInWork,
                            sourceID,
                            senses,
                            null);
                        Glossary.Add(word, entry);
                    }
                    else if (!String.IsNullOrEmpty(translation) &&
                            probableSynonyms.FirstOrDefault(x => x.MatchMeaning(translation)) == null)
                        probableSynonyms.Add(probableSynonym);

                    if ((sense != null) && probableSynonym.HasInflections())
                        sense.AddInflections(probableSynonym.Inflections);
                }
            }
            catch (Exception exc)
            {
                string msg = exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                PutError(msg);
                return false;
            }

            return true;
        }

        public bool LoadPhrases(string filePath, out string errorMessage)
        {
            errorMessage = null;

            Phrases = new List<string>();

            if (!FileSingleton.Exists(filePath))
            {
                errorMessage = "Note: There is no phrase file for the current target language: "
                    + TargetLanguageID.LanguageName(HostLanguageID)
                    + " and host language "
                    + HostLanguageID.LanguageName(HostLanguageID)
                    + "\nNo phrases will be marked.";
                PutMessage(errorMessage);
                return true;
            }

            try
            {
                Phrases = FileSingleton.ReadAllLines(
                    filePath,
                    ApplicationData.Encoding).ToList();
            }
            catch (Exception exc)
            {
                string msg = exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                PutError(msg);
                return false;
            }

            return true;
        }

        public bool ConvertTextToJsonPackage(out string errorMessage)
        {
            errorMessage = null;

            ClearStatistics();

            if (ShowStatistics)
            {
                CreateTimerCheck();
                GlossaryLookupTimer = new SoftwareTimer();
                Timer.Start();
            }

            InitializeTools();

            if (!InitializeGlossaryData(
                    new List<LanguageID>() { TargetLanguageID },
                    new List<LanguageID>() { HostLanguageID }))
                return false;

            StudyList = new ContentStudyList("Passages");

            if (!ReadStudyList())
            {
                errorMessage = Error;
                return false;
            }

            StudyItems = StudyList.StudyItems;

            if (TextUnitsSentences)
                StudyItems = ExpandStudyItems(
                    StudyItems,
                    TargetLanguageIDs,
                    HostLanguageIDs,
                    UniqueLanguageIDs);

            if (!GenerateModule())
            {
                errorMessage = Error;
                return false;
            }

            if (ShowStatistics)
            {
                Timer.Stop();
                double totalGenerationTime = Timer.GetTimeInSeconds();

                if (Module != null)
                {
                    string newField = "\"totalGenerationTime\": " + totalGenerationTime.ToString();
                    Module = Module.Replace(
                        "\"totalGenerationTime\": 0.0",
                        newField);
                }
            }

            return true;
        }

        public EmbarkPackage ConvertTextToJsonPackageRaw(
            string targetText,
            string hostText,
            bool showTranslation,
            bool showGlossary,
            bool showStatistics,
            bool showJSON,
            string wordAudioMode,
            string errorMessage)
        {
            EmbarkPackage package = null;

            TargetText = targetText;
            HostText = hostText;
            ShowTranslation = showTranslation;
            ShowGlossary = showGlossary;
            ShowStatistics = showStatistics;
            ShowJSON = showJSON;
            WordAudioMode = wordAudioMode;

            ClearStatistics();

            if (ShowStatistics)
            {
                CreateTimerCheck();
                GlossaryLookupTimer = new SoftwareTimer();
                Timer.Start();
            }

            InitializeTools();

            if (!String.IsNullOrEmpty(errorMessage))
            {
                package = new EmbarkPackage();
                package.errorMessage = errorMessage;
            }
            else
            {
                if (!InitializeGlossaryData(
                        new List<LanguageID>() { TargetLanguageID },
                        new List<LanguageID>() { HostLanguageID }))
                {
                    package = new EmbarkPackage();
                    package.errorMessage = Error;
                }
                else
                {
                    StudyList = new ContentStudyList("Passages");

                    if (!ReadStudyList())
                    {
                        package = new EmbarkPackage();
                        package.errorMessage = Error;
                    }
                    else
                    {
                        StudyItems = StudyList.StudyItems;

                        if (TextUnitsSentences)
                            StudyItems = ExpandStudyItems(
                                StudyItems,
                                TargetLanguageIDs,
                                HostLanguageIDs,
                                UniqueLanguageIDs);

                        package = GetPackage();

                        if (package == null)
                        {
                            package = new EmbarkPackage();
                            package.errorMessage = Error;
                        }
                    }
                }
            }

            if (ShowStatistics)
            {
                Timer.Stop();
                package.totalGenerationTime = (float)Timer.GetTimeInSeconds();
            }

            return package;
        }

        protected virtual bool ReadStudyList()
        {
            bool returnValue = true;

            if (!String.IsNullOrEmpty(HostText))
                returnValue = ReadStudyListDouble();
            else
                returnValue = ReadStudyListSingle();

            return returnValue;
        }

        protected virtual bool ReadStudyListSingle()
        {
            bool returnValue = true;

            LanguageTool tool = TargetTool;

            try
            {
                using (StringReader reader = new StringReader(TargetText))
                {
                    string textLine;
                    int index = 0;
                    LanguageID languageID = TargetLanguageID;

                    while ((textLine = reader.ReadLine()) != null)
                    {
                        textLine = FilterLine(textLine);

                        string studyKey = "I" + index.ToString();
                        MultiLanguageItem studyItem = new MultiLanguageItem(studyKey, languageID, textLine);
                        LanguageItem languageItem = studyItem.LanguageItem(0);

                        languageItem.LoadSentenceRunsFromText();

#if true
                        languageItem.LoadWordRunsFromText(null);
#else
                        List<TextGraphNode> wordNodes;
                        List<TextRun> wordRuns;

                        if (tool.ParseAndGetWordRuns(
                            textLine,
                            languageID,
                            out wordNodes,
                            out wordRuns))
                        {
                            languageItem.WordRuns = wordRuns;
                        }
                        else
                        {
                            PutError("Error parsing text line: " + textLine);
                            returnValue = false;
                            break;
                        }
#endif

                        if ((tool != null) && (Phrases != null) && (Phrases.Count() != 0))
                        {
                            string errorMessage;

                            if (!tool.MarkPhrasesLanguageItem(
                                    studyItem,
                                    languageItem,
                                    Phrases,
                                    false,
                                    false,
                                    out errorMessage))
                                PutError(errorMessage);
                        }

                        StudyList.AddStudyItem(studyItem);
                    }
                }
            }
            catch (Exception exc)
            {
                string msg = exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                PutError(msg);
                returnValue = false;
            }
            finally
            {
            }

            return returnValue;
        }

        protected virtual bool ReadStudyListDouble()
        {
            bool returnValue = true;

            LanguageTool targetTool = TargetTool;
            //LanguageTool hostTool = HostTool;

            try
            {
                using (StringReader targetReader = new StringReader(TargetText))
                {
                    using (StringReader hostReader = new StringReader(HostText))
                    {
                        string targetLine;
                        string hostLine;
                        int index = 0;
                        LanguageID targetLanguageID = TargetLanguageID;
                        LanguageID hostLanguageID = HostLanguageID;

                        for (;;)
                        {
                            while ((targetLine = targetReader.ReadLine()) == String.Empty)
                                ;

                            while ((hostLine = hostReader.ReadLine()) == String.Empty)
                                ;

                            if ((targetLine == null) || (hostLine == null))
                            {
                                if ((targetLine != null) || (hostLine != null))
                                {
                                    PutError("Paragraphs don't align.");
                                    return false;
                                }

                                break;
                            }

                            targetLine = FilterLine(targetLine);
                            hostLine = FilterLine(hostLine);

                            string studyKey = "I" + index.ToString();
                            MultiLanguageItem studyItem = new MultiLanguageItem(studyKey, targetLanguageID, targetLine);
                            LanguageItem targetLanguageItem = studyItem.LanguageItem(0);
                            LanguageItem hostLanguageItem = new LanguageItem(studyKey, hostLanguageID, hostLine);

                            studyItem.Add(hostLanguageItem);
                            targetLanguageItem.LoadSentenceRunsFromText();
                            hostLanguageItem.LoadSentenceRunsFromText();

#if true
                            targetLanguageItem.LoadWordRunsFromText(null);
                            hostLanguageItem.LoadWordRunsFromText(null);
#else
                            List<TextGraphNode> wordNodes;
                            List<TextRun> wordRuns;

                            if (targetTool.ParseAndGetWordRuns(
                                targetTextLine,
                                targetLanguageID,
                                out wordNodes,
                                out wordRuns))
                            {
                                targetLanguageItem.WordRuns = wordRuns;

                                if (hostTool.ParseAndGetWordRuns(
                                    hostTextLine,
                                    hostLanguageID,
                                    out wordNodes,
                                    out wordRuns))
                                {
                                    hostLanguageItem.WordRuns = wordRuns;
                                }
                                else
                                {
                                    PutError("Error parsing text line: " + hostTextLine);
                                    returnValue = false;
                                    break;
                                }
                            }
                            else
                            {
                                PutError("Error parsing text line: " + targetTextLine);
                                returnValue = false;
                                break;
                            }
#endif

                            if (IsParagraphsOnly)
                                studyItem.JoinSentenceRuns(UniqueLanguageIDs);
                            else
                                ContentUtilities.ParseSentenceRuns(
                                    studyItem,
                                    TargetLanguageIDs,
                                    HostLanguageIDs,
                                    UniqueLanguageIDs,
                                    SentenceParsingAlgorithm.RatioWalker,
                                    SentenceParsingFallback.DoNothing);

                            if (studyItem.IsSentenceMismatch(UniqueLanguageIDs))
                                studyItem.JoinSentenceRuns(UniqueLanguageIDs);

                            if ((targetTool != null) && (Phrases != null) && (Phrases.Count() != 0))
                            {
                                string errorMessage;

                                if (!targetTool.MarkPhrasesLanguageItem(
                                        studyItem,
                                        targetLanguageItem,
                                        Phrases,
                                        false,
                                        false,
                                        out errorMessage))
                                    PutError(errorMessage);
                            }

                            StudyList.AddStudyItem(studyItem);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                string msg = exc.Message;

                if (exc.InnerException != null)
                    msg += ": " + exc.InnerException.Message;

                PutError(msg);
                returnValue = false;
            }
            finally
            {
            }

            return returnValue;
        }

        List<MultiLanguageItem> ExpandStudyItems(
            List<MultiLanguageItem> studyItems,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs,
            List<LanguageID> languageIDs)
        {
            List<MultiLanguageItem> newStudyItems = new List<MultiLanguageItem>();

            foreach (MultiLanguageItem oldStudyItem in studyItems)
            {
                List<LanguageID> allLanguageIDs = LanguageID.CreateIntersection(oldStudyItem.LanguageIDs, languageIDs);
                int sentenceCount = oldStudyItem.GetMaxSentenceCount(allLanguageIDs);
                int sentenceIndex;

                if (IsParagraphsOnly)
                    oldStudyItem.JoinSentenceRuns(languageIDs);
                else
                    ContentUtilities.ParseSentenceRuns(
                        oldStudyItem,
                        targetLanguageIDs,
                        hostLanguageIDs,
                        languageIDs,
                        //SentenceParsingAlgorithm.MatchPunctuation,
                        //SentenceParsingFallback.CollapseFailedRatios);
                        SentenceParsingAlgorithm.RatioWalker,
                        SentenceParsingFallback.DoNothing);

                if (oldStudyItem.IsSentenceMismatch(languageIDs))
                {
                    MultiLanguageItem newStudyItem = new MultiLanguageItem(oldStudyItem);
                    newStudyItem.JoinSentenceRuns(allLanguageIDs);
                    newStudyItems.Add(newStudyItem);
                }
                else if (sentenceCount == 0)
                {
                    MultiLanguageItem newStudyItem = new MultiLanguageItem(oldStudyItem);
                    newStudyItems.Add(newStudyItem);
                }
                else
                {
                    for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                    {
                        List<LanguageItem> newLanguageItems = new List<LanguageItem>();
                        string newKey = oldStudyItem.KeyString + "." + sentenceIndex.ToString();

                        foreach (LanguageID languageID in allLanguageIDs)
                        {
                            LanguageItem oldLanguageItem = oldStudyItem.LanguageItem(languageID);

                            if (oldLanguageItem == null)
                                continue;

                            TextRun oldSentenceRun = oldLanguageItem.GetSentenceRun(sentenceIndex);
                            LanguageItem newLanguageItem;
                            TextRun newSentenceRun;
                            List<TextRun> newSentenceRuns = new List<TextRun>();
                            List<TextRun> newWordRuns;
                            List<TextRun> newphraseRuns;
                            string newText;

                            if (oldSentenceRun == null)
                            {
                                newText = String.Empty;
                                newSentenceRun = new TextRun(0, 0, null);
                                newWordRuns = new List<TextRun>();
                                newphraseRuns = new List<TextRun>();
                            }
                            else
                            {
                                newText = oldLanguageItem.GetRunText(oldSentenceRun);
                                newSentenceRun = new TextRun(oldSentenceRun);
                                newSentenceRun.Start = 0;
                                newWordRuns = oldLanguageItem.GetSentenceWordRunsRetargeted(oldSentenceRun);
                                newphraseRuns = oldLanguageItem.GetSentencePhraseRunsRetargeted(oldSentenceRun);
                            }

                            newSentenceRuns.Add(newSentenceRun);
                            newLanguageItem = new LanguageItem(newKey, languageID, newText, newSentenceRuns, newWordRuns);
                            newLanguageItem.PhraseRuns = newphraseRuns;
                            newLanguageItems.Add(newLanguageItem);
                        }

                        List<Annotation> newAnnotations = null;

                        if (oldStudyItem.HasAnnotations())
                        {
                            newAnnotations = oldStudyItem.CloneAnnotations();

                            if (sentenceIndex != 0)
                            {
                                Annotation ordinalAnnotation = newAnnotations.FirstOrDefault(x => x.Type == "Ordinal");

                                if (ordinalAnnotation != null)
                                    ordinalAnnotation.Type = "HiddenOrdinal";
                            }
                        }

                        MultiLanguageItem newStudyItem = new MultiLanguageItem(
                            newKey,
                            newLanguageItems,
                            oldStudyItem.SpeakerNameKey,
                            newAnnotations,
                            null,
                            oldStudyItem.StudyList);

                        newStudyItems.Add(newStudyItem);
                    }
                }
            }

            return newStudyItems;
        }

        protected static char[] AllowCharactersBeforeNumber =
        {
            '.',
            ',',
            '-',
            '@',
            '#',
            '$',
            '%',
            '^',
            '&',
            '*',
            '(',
            '[',
            '(',
            '+',
            '=',
            '|',
            '/',
            '\\',
            '~',
            '<',
            '>'
        };

        protected string FilterLine(string textLine)
        {
            StringBuilder sb = new StringBuilder();
            int count = textLine.Length;
            int index;
            char lastChr = '\0';

            for (index = 0; index < count; )
            {
                char chr = textLine[index];

                if (char.IsDigit(chr))
                {
                    if (lastChr != '\0')
                    {
                        if (!char.IsWhiteSpace(lastChr) &&
                            !char.IsDigit(lastChr) &&
                            !AllowCharactersBeforeNumber.Contains(lastChr))
                        {
                            for (index++; index < count; index++)
                            {
                                chr = textLine[index];

                                if (!char.IsDigit(chr))
                                    break;
                            }

                            continue;
                        }
                    }
                }

                sb.Append(chr);
                lastChr = chr;
                index++;
            }

            return sb.ToString();
        }

        public class GlossaryData
        {
            public LanguageID TargetLanguageID;
            public LanguageID HostLanguageID;
            public Dictionary<string, DictionaryEntry> EntryDictionary;
            public List<DictionaryEntry> EntryList;
            public HashSet<string> FilteredFlags;

            public GlossaryData(
                LanguageID targetLanguageID,
                LanguageID hostLanguageID)
            {
                TargetLanguageID = targetLanguageID;
                HostLanguageID = hostLanguageID;
                EntryDictionary = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
                EntryList = new List<DictionaryEntry>();
                FilteredFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public GlossaryData(
                LanguageID targetLanguageID,
                LanguageID hostLanguageID,
                Dictionary<string, DictionaryEntry> entryDictionary,
                List<DictionaryEntry> entryList)
            {
                TargetLanguageID = targetLanguageID;
                HostLanguageID = hostLanguageID;
                EntryDictionary = entryDictionary;
                EntryList = entryList;
                FilteredFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public GlossaryData()
            {
                TargetLanguageID = null;
                HostLanguageID = null;
                EntryDictionary = null;
                EntryList = null;
                FilteredFlags = null;
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

            // Assumes already exists.
            public bool ReplaceEntry(
                DictionaryEntry entry,
                int glossaryIndex)
            {
                EntryList[glossaryIndex] = entry;
                EntryDictionary[entry.KeyString] = entry;
                return true;
            }

            // Assumes already exists.
            public bool SetFilteredEntry(
                DictionaryEntry entry,
                int glossaryIndex)
            {
                EntryList[glossaryIndex] = entry;
                EntryDictionary[entry.KeyString] = entry;
                return FilteredFlags.Add(entry.KeyString);
            }

            public bool IsFilteredEntry(
                DictionaryEntry entry,
                int glossaryIndex)
            {
                bool returnValue = FilteredFlags.Contains(entry.KeyString);
                return returnValue;
            }

            public static string GetKey(LanguageID targetLanguageID, LanguageID hostLanguageID)
            {
                string key = targetLanguageID.LanguageCultureExtensionCode + "_" + hostLanguageID.LanguageCultureExtensionCode;
                return key;
            }
        }

        protected bool InitializeGlossaryData(
            List<LanguageID> targetRootLanguageIDs,
            List<LanguageID> hostRootLanguageIDs)
        {
            GlossaryDataCache = new Dictionary<string, GlossaryData>();

            foreach (LanguageID targetLanguageID in targetRootLanguageIDs)
            {
                foreach (LanguageID hostLanguageID in hostRootLanguageIDs)
                {
                    if (GetGlossaryData(targetLanguageID, hostLanguageID) != null)
                        continue;

                    GlossaryData glossaryData = new GlossaryData(
                        targetLanguageID,
                        hostLanguageID);

                    string key = GlossaryData.GetKey(targetLanguageID, hostLanguageID);
                    GlossaryDataCache.Add(key, glossaryData);
                }
            }

            foreach (LanguageID hostLanguageID in hostRootLanguageIDs)
            {
                foreach (LanguageID targetLanguageID in targetRootLanguageIDs)
                {
                    if (GetGlossaryData(hostLanguageID, targetLanguageID) != null)
                        continue;

                    GlossaryData glossaryData = new GlossaryData(
                        hostLanguageID,
                        targetLanguageID);

                    string key = GlossaryData.GetKey(hostLanguageID, targetLanguageID);
                    GlossaryDataCache.Add(key, glossaryData);
                }
            }

            return true;
        }

        protected GlossaryData GetGlossaryData(LanguageID targetLanguageID, LanguageID hostLanguaegID)
        {
            GlossaryData glossaryData = null;
            string key = GlossaryData.GetKey(targetLanguageID, hostLanguaegID);
            GlossaryDataCache.TryGetValue(key, out glossaryData);
            return glossaryData;
        }

        public class AnnotationItem
        {
            public string InputTag;             // Input style to match.
            public TextType OutputType;         // Output TextType enum.

            public AnnotationItem(string inputTag, TextType outputType)
            {
                InputTag = inputTag;
                OutputType = outputType;
            }
        }

        // Map annotation tag to text type.
        public static AnnotationItem[] AnnotationTagMap = {
            new AnnotationItem("SimpleText", TextType.SimpleText),                  // 0 = Simple text.
            new AnnotationItem("Verse", TextType.Verse),                            // 1 = Verse.
            new AnnotationItem("Heading", TextType.Heading),                        // 2 = Chapter heading.
            new AnnotationItem("SubHeading", TextType.SubHeading),                  // 3 = Chapter subheading.
            new AnnotationItem("ByLine", TextType.ByLine),                          // 4 = Byline.
            new AnnotationItem("Kicker", TextType.Kicker),                          // 5 = Kicker.
            new AnnotationItem("Intro", TextType.Intro),                            // 6 = Introduction.
            new AnnotationItem("StudyIntro", TextType.StudyIntro),                  // 7 = Study introduction.
            new AnnotationItem("Title", TextType.Title),                            // 8 = General title.
            new AnnotationItem("TitleNumber", TextType.TitleNumber),                // 9 = Title with a number.
            new AnnotationItem("SubTitle", TextType.SubTitle),                      // 10 = General subtitle.
            new AnnotationItem("Summary", TextType.Summary),                        // 11 = Summary.
            new AnnotationItem("SuperScript", TextType.SuperScript),                // 12 = Superscript to footnote.
            new AnnotationItem("Footnote", TextType.Footnote),                      // 13 = Footnote.
            new AnnotationItem("Suffix", TextType.Suffix),                          // 14 = Suffix.
            new AnnotationItem("InsetHeadingRed", TextType.InsetHeadingRed),        // 15 = Red inset heading.
            new AnnotationItem("InsetHeadingGreen", TextType.InsetHeadingGreen),    // 16 = Green inset heading.
            new AnnotationItem("InsetHeadingOrange", TextType.InsetHeadingOrange),  // 17 = Orange inset heading.
            new AnnotationItem("InsetHeadingTeal", TextType.InsetHeadingTeal),      // 18 = Teal inset heading.
            new AnnotationItem("InsetText", TextType.InsetText),                    // 19 = Inset text.
            new AnnotationItem("AsideHeading", TextType.AsideHeading),              // 20 = Aside heading.
            new AnnotationItem("AsideText", TextType.AsideText)                     // 21 = Aside text.
            };

        public bool GenerateModule()
        {
            if (StudyItems.Count() == 0)
                return false;

            EmbarkPackage package = GetPackage();
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;

            Module = JsonConvert.SerializeObject(
                package,
                Newtonsoft.Json.Formatting.Indented,
                jsonSettings);

            return true;
        }

        public EmbarkPackage GetPackage()
        {
            if (StudyItems.Count() == 0)
                return null;

            bool inInset = false;
            bool inAside = false;
            string nodeKey = "Passage";
            string targetTitle = "Passage";
            string nativeTitle = "Passage";
            string mediaFileName = String.Empty;
            string iconFileName = String.Empty;
            GlossaryData glossaryData = GetGlossaryData(TargetLanguageID, HostLanguageID);
            List<TextUnit> textUnits = new List<TextUnit>();

            NodeUtilities.CollectDictionaryWordsSpecial(
                StudyItems,
                TargetLanguageID,
                null,
                HostLanguageIDs,
                true,
                IsTranslateMissingItems,
                IsAddNewItemsToDictionary,
                glossaryData.EntryList,
                glossaryData.EntryDictionary,
                GlossaryLookup);

            foreach (MultiLanguageItem studyItem in StudyItems)
            {
                LanguageItem targetLanguageItem = studyItem.LanguageItem(TargetLanguageID);
                LanguageItem hostLanguageItem = studyItem.LanguageItem(HostLanguageID);

                if (targetLanguageItem == null)
                    continue;

                List<RunGroup> runGroups = new List<RunGroup>();
                List<TextRun> sentenceRuns = targetLanguageItem.SentenceRuns;
                List<TextRun> hostSentenceRuns = (hostLanguageItem != null ? hostLanguageItem.SentenceRuns : null);
                TextType textType = TextType.SimpleText;
                string targetPrefix = null;
                string nativePrefix = null;

                if (studyItem.HasAnnotations())
                {
                    foreach (Annotation annotation in studyItem.Annotations)
                    {
                        if (annotation.Type.StartsWith("Hidden"))
                            continue;

                        AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == annotation.Tag);

                        if (annotationItem != null)
                            textType = annotationItem.OutputType;

                        if (textType == TextType.Verse)
                        {
                            if ((annotation.Text != null) && annotation.Text.HasText(TargetLanguageID))
                            {
                                targetPrefix = annotation.Text.Text(TargetLanguageID) + " ";
                                nativePrefix = annotation.Text.Text(HostLanguageID) + " ";
                            }
                            else if (!String.IsNullOrEmpty(annotation.Value))
                            {
                                targetPrefix = annotation.Value + " ";
                                nativePrefix = annotation.Value + " ";
                            }
                        }
                    }
                }

                string targetText = targetLanguageItem.Text;
                string nativeText = (hostLanguageItem != null ? hostLanguageItem.Text : String.Empty);

                Annotation startAnnotation = studyItem.FindAnnotation("Start");
                Annotation styleAnnotation = studyItem.FindAnnotation("Style");

                if (startAnnotation != null)
                {
                    if (startAnnotation.Value.StartsWith("Inset"))
                    {
                        AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == startAnnotation.Tag);

                        if (annotationItem != null)
                            textType = annotationItem.OutputType;

                        inInset = true;
                    }
                    else if (startAnnotation.Value.StartsWith("Aside"))
                    {
                        AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == startAnnotation.Tag);

                        if (annotationItem != null)
                            textType = annotationItem.OutputType;

                        inAside = true;
                    }
                }
                else if (inInset)
                {
                    if (styleAnnotation != null)
                    {
                        AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == styleAnnotation.Tag);

                        if (annotationItem != null)
                            textType = annotationItem.OutputType;
                        else
                            textType = TextType.InsetText;
                    }
                    else
                        textType = TextType.InsetText;
                }
                else if (inAside)
                {
                    if (styleAnnotation != null)
                    {
                        AnnotationItem annotationItem = AnnotationTagMap.FirstOrDefault(x => x.InputTag == styleAnnotation.Tag);

                        if (annotationItem != null)
                            textType = annotationItem.OutputType;
                        else
                            textType = TextType.AsideText;
                    }
                    else
                        textType = TextType.AsideText;
                }

                Annotation stopAnnotation = studyItem.FindAnnotation("Stop");

                if (stopAnnotation != null)
                {
                    if (stopAnnotation.Value.StartsWith("Inset"))
                        inInset = false;
                    else if (stopAnnotation.Value.StartsWith("Aside"))
                        inAside = false;
                }

                if (sentenceRuns != null)
                {
                    float startTime = -1;
                    float stopTime = -1;
                    int sentenceCount = sentenceRuns.Count();
                    int sentenceIndex;
                    TextUnit textUnit = null;

                    for (sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                    {
                        TextRun sentenceRun = sentenceRuns[sentenceIndex];
                        TextRun hostSentenceRun = (hostSentenceRuns != null ? hostSentenceRuns[sentenceIndex] : null);
                        RunGroup runGroup = GetMainRunGroup(
                            studyItem,
                            targetLanguageItem,
                            sentenceRun,
                            sentenceIndex,
                            TargetLanguageID,
                            HostLanguageID,
                            targetPrefix,
                            nativePrefix,
                            glossaryData);

                        runGroups.Add(runGroup);

                        if (TextUnitsSentences)
                        {
                            textUnit = new TextUnit();

                            if (sentenceIndex == 0)
                            {
                                textUnit.targetPrefix = targetPrefix;
                                textUnit.nativePrefix = nativePrefix;
                            }
                            else
                            {
                                textUnit.targetPrefix = null;
                                textUnit.nativePrefix = null;
                            }

                            textUnit.targetText = targetLanguageItem.GetRunText(sentenceRun);
                            textUnit.nativeText = (hostSentenceRun != null ? hostLanguageItem.GetRunText(hostSentenceRun) : String.Empty);
                            textUnit.runGroups = runGroups.ToArray();
                            textUnit.type = (int)textType;
                            textUnit.startTime = runGroup.startTime;
                            textUnit.stopTime = runGroup.stopTime;
                            textUnits.Add(textUnit);
                            runGroups = new List<RunGroup>();
                        }
                    }

                    if (textUnit == null)
                    {
                        textUnit = new TextUnit();

                        textUnit.targetPrefix = targetPrefix;
                        textUnit.nativePrefix = nativePrefix;
                        textUnit.targetText = targetText;
                        textUnit.nativeText = nativeText;
                        textUnit.runGroups = runGroups.ToArray();
                        textUnit.type = (int)textType;

                        if (startTime != -1)
                            textUnit.startTime = startTime;
                        else
                            textUnit.startTime = null;

                        if (stopTime != -1)
                            textUnit.stopTime = stopTime;
                        else
                            textUnit.stopTime = null;

                        textUnits.Add(textUnit);
                    }
                }
            }

            Node node = new Node();
            node.key = nodeKey;
            node.targetTitle = targetTitle;
            node.nativeTitle = nativeTitle;
            node.textUnits = textUnits.ToArray();
            node.mediaFileName = mediaFileName;
            node.iconFileName = iconFileName;
            node.children = null;

            List<Node> nodes = new List<Node>() { node };
            List<string> targetLanguageCodes = new List<string>();
            List<string> hostLanguageCodes = new List<string>();

            List<WordList> wordLists = new List<WordList>();

            CreateGlossaries(wordLists);

            string targetLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(TargetLanguageID.LanguageCultureExtensionCode);

            if (!targetLanguageCodes.Contains(targetLanguageCode))
                targetLanguageCodes.Add(targetLanguageCode);

            string hostLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(HostLanguageID.LanguageCultureExtensionCode);

            if (!hostLanguageCodes.Contains(hostLanguageCode))
                hostLanguageCodes.Add(hostLanguageCode);

            EmbarkPackage package = new EmbarkPackage();
            package.formatVersion = currentFormatVersion;
            package.targetLanguageCode = targetLanguageCodes[0];
            package.nativeLanguageCode = hostLanguageCodes[0];
            package.mediaPath = String.Empty;
            package.imagePath = String.Empty;
            package.nodes = nodes.ToArray();
            package.wordList = wordLists.FirstOrDefault();

            if (ShowStatistics)
            {
                package.totalUniqueWordCount = TotalUniqueWordCount;
                package.foundInGlossaryCount = FoundInGlossaryCount;
                package.foundInDictionaryCount = FoundInDictionaryCount;
                package.recognizedAsInflectionCount = RecognizedAsInflectionCount;
                package.googleTranslateCount = GoogleTranslateCount;
                package.translatorCacheCount = TranslatorCacheCount;

                package.lookupDictionaryCount = LookupDictionaryCount;
                package.lookupInflectionCount = LookupInflectionCount;
                package.lookupGoogleTranslateCount = LookupGoogleTranslateCount;
                package.lookupTranslatorCacheCount = LookupTranslatorCacheCount;

                package.sumDictionaryLookupTime = (float)SumDictionaryLookupTime;
                package.sumInflectionLookupTime = (float)SumInflectionLookupTime;
                package.sumGoogleTranslateTime = (float)SumGoogleTranslateTime;
                package.sumTranslatorCacheTime = (float)SumTranslatorCacheTime;

                if (LookupDictionaryCount != 0)
                    package.averageDictionaryLookupTime = (float)(SumDictionaryLookupTime / LookupDictionaryCount);
                else
                    package.averageDictionaryLookupTime = 0.0f;

                if (LookupInflectionCount != 0)
                    package.averageInflectionLookupTime = (float)(SumInflectionLookupTime / LookupInflectionCount);
                else
                    package.averageInflectionLookupTime = 0.0f;

                if (LookupGoogleTranslateCount != 0)
                    package.averageGoogleTranslateTime = (float)(SumGoogleTranslateTime / LookupGoogleTranslateCount);
                else
                    package.averageGoogleTranslateTime = 0.0f;

                if (LookupTranslatorCacheCount != 0)
                    package.averageTranslatorCacheTime = (float)(SumTranslatorCacheTime / LookupTranslatorCacheCount);
                else
                    package.averageTranslatorCacheTime = 0.0f;

                package.totalServerTime = 0.0f;    // Filled in by backend.
                package.totalGenerationTime = 0.0f;    // Filled in by backend.
            }

            return package;
        }

        protected RunGroup GetMainRunGroup(
            MultiLanguageItem studyItem,
            LanguageItem languageItem,
            TextRun sentenceRun,
            int sentenceIndex,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            string targetPrefix,
            string nativePrefix,
            GlossaryData glossaryData)
        {
            string languageText = String.Empty;
            int sentenceNumber = sentenceIndex + 1;
            List<TextRun> wordRuns = null;
            TextRun wordRun;
            List<Run> runs = new List<Run>();
            Run run;
            LanguageItem hostLanguageItem = studyItem.LanguageItem(hostLanguageID);
            TextRun hostSentenceRun = (hostLanguageItem != null ? hostLanguageItem.GetSentenceRun(sentenceIndex) : null);
            List<DictionaryEntry> hostDictionaryEntries =
                //GetSentenceRunWordDictionaryEntries(
                //    hostLanguageItem, targetLanguageID, hostSentenceRun);
                null;
            string targetText = String.Empty;
            string hostText = String.Empty;

            if (TextUnitsSentences)
            {
                string studyItemKey = studyItem.KeyString;
                int ofs = studyItemKey.LastIndexOf('.');

                if (ofs != -1)
                {
                    string actualSentenceIndexString = studyItemKey.Substring(ofs + 1);
                    int acutalSentenceIndex = ObjectUtilities.GetIntegerFromString(actualSentenceIndexString, 0);
                    sentenceNumber = acutalSentenceIndex + 1;
                }
            }

            if (languageItem != null)
            {
                languageText = languageItem.Text;
                wordRuns = languageItem.PhrasedWordRuns;

                if (sentenceRun != null)
                    targetText = languageItem.GetRunText(sentenceRun);
            }

            if (hostLanguageItem != null)
            {
                if (hostSentenceRun != null)
                    hostText = hostLanguageItem.GetRunText(hostSentenceRun);
                else
                    hostText = hostLanguageItem.Text;
            }

            int groupStart = sentenceRun.Start;
            int groupStop = sentenceRun.Stop;
            int textIndex = groupStart;

            if (wordRuns != null)
            {
                int runCount = wordRuns.Count();
                int runIndex;
                int sentenceWordIndex = 0;

                for (runIndex = 0; runIndex < runCount; runIndex++)
                {
                    wordRun = wordRuns[runIndex];

                    if ((wordRun.Stop <= sentenceRun.Start) || (wordRun.Start >= sentenceRun.Stop))
                        continue;

                    int runStart = wordRun.Start;
                    int runStop = wordRun.Stop;
                    string text = languageText.Substring(wordRun.Start, wordRun.Length);
                    int wordIndex = -1;
                    List<int> fitsContext = null;
                    List<Sense> senses = null;

                    textIndex += text.Length;

                    if (!String.IsNullOrEmpty(text))
                    {
                        DictionaryEntry dictionaryEntry;

                        if (text == "mis")
                            ApplicationData.Global.PutConsoleMessage(text);

                        if (glossaryData.GetEntry(text.ToLower(), out dictionaryEntry, out wordIndex))
                        {
                            FilterDictionaryEntry(glossaryData, ref dictionaryEntry, wordIndex, hostLanguageID);

                            senses = dictionaryEntry.GetSensesWithLanguageID(HostLanguageIDs.First());

                            if ((senses != null) && (senses.Count() != 0))
                                FindBestTranslationIndexes(
                                    senses,
                                    hostDictionaryEntries,
                                    targetLanguageID,
                                    hostLanguageID,
                                    targetText,
                                    hostText,
                                    out fitsContext);
                        }
                    }

                    run = new Run();

                    run.runStart = runStart;
                    run.runStop = runStop;
                    run.glossaryKey = wordIndex;

                    if (fitsContext != null)
                        run.fitsContext = fitsContext.ToArray();
                    else
                        run.fitsContext = null;

                    runs.Add(run);

                    sentenceWordIndex++;
                }
            }

            RunGroup runGroup = new RunGroup();
            runGroup.sentenceNumber = sentenceNumber;
            runGroup.groupStart = groupStart;
            runGroup.groupStop = groupStop;
            runGroup.runs = runs.ToArray();

            return runGroup;
        }

        protected List<DictionaryEntry> GetSentenceRunWordDictionaryEntries(
            LanguageItem languageItem,
            LanguageID hostLanguageID,
            TextRun sentenceRun)
        {
            List<DictionaryEntry> entries = new List<DictionaryEntry>();

            if ((languageItem == null) || (languageItem.WordRunCount() == 0) || (sentenceRun == null))
                return entries;

            LanguageID languageID = languageItem.LanguageID;
            GlossaryData glossaryData = GetGlossaryData(languageID, hostLanguageID);

            foreach (TextRun wordRun in languageItem.PhrasedWordRuns)
            {
                if ((wordRun.Stop <= sentenceRun.Start) || (wordRun.Start >= sentenceRun.Stop))
                    continue;

                string word = languageItem.GetRunText(wordRun);

                if (String.IsNullOrEmpty(word))
                    continue;

                DictionaryEntry dictionaryEntry;

                if (!glossaryData.EntryDictionary.TryGetValue(word.ToLower(), out dictionaryEntry))
                    dictionaryEntry = new DictionaryEntry(word, hostLanguageID);

                if (entries.FirstOrDefault(x => x.KeyString == word) == null)
                    entries.Add(dictionaryEntry);
            }

            return entries;
        }

        protected bool FindBestTranslationIndexes(
            List<Sense> senses,
            List<DictionaryEntry> hostDictionaryEntries,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            string targetSentence,
            string hostSentence,
            out List<int> fitsContext)
        {
            bool returnValue = false;
            int senseCount = senses.Count();
            int indexSense;
            List<DictionaryEntry> matchedEntries = new List<DictionaryEntry>();
            List<int> matchedIndices = new List<int>();
            int meaningIndex = 0;

            fitsContext = null;

            if (!String.IsNullOrEmpty(hostSentence))
            {
                meaningIndex = 0;

                for (indexSense = 0; indexSense < senseCount; indexSense++)
                {
                    Sense sense = senses[indexSense];
                    LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                    if (languageSynonyms == null)
                        continue;

                    if (languageSynonyms.HasProbableSynonyms())
                    {
                        foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                        {
                            if (!matchedIndices.Contains(meaningIndex))
                            {
                                string hostWordOrPhrase = probableSynonym.Meaning;

                                if (TextUtilities.ContainsWholeWordCaseInsensitive(hostSentence, hostWordOrPhrase))
                                    matchedIndices.Add(meaningIndex);
                            }

                            meaningIndex++;
                        }
                    }
                }
            }

            if ((hostDictionaryEntries != null) && (hostDictionaryEntries.Count() != 0))
            {
                foreach (DictionaryEntry hostDictionaryEntry in hostDictionaryEntries)
                {
                    string hostWord = hostDictionaryEntry.KeyString;

                    meaningIndex = 0;

                    for (indexSense = 0; indexSense < senseCount; indexSense++)
                    {
                        Sense sense = senses[indexSense];
                        LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                        if (languageSynonyms == null)
                            continue;

                        if (languageSynonyms.HasProbableSynonyms())
                        {
                            foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                            {
                                if (!matchedIndices.Contains(meaningIndex))
                                {
                                    if (probableSynonym.MatchMeaningIgnoreCase(hostWord))
                                    {
                                        matchedEntries.Add(hostDictionaryEntry);
                                        matchedIndices.Add(meaningIndex);
                                    }
                                }

                                meaningIndex++;
                            }
                        }
                    }
                }
            }

            if (matchedIndices.Count() != 0)
            {
                fitsContext = matchedIndices;
                returnValue = true;
            }
            else
                fitsContext = null;

            return returnValue;
        }

        private static MediaUtilities.AudioAttributeComparer AudioAttributeComparerStatic =
            new MediaUtilities.AudioAttributeComparer();

        protected List<AudioReading> GetAudioReadings(
            string targetWord,
            List<string> audioKeys,
            LanguageID targetLanguageID,
            LanguageID preferedMediaLanguageID,
            Voice voice)
        {
            switch (WordAudioMode)
            {
                case "None":
                case "AllLazy":
                    return null;
                default:
                    break;
            }

            List<AudioReading> audioReadings = null;
            List<AudioMultiReference> audioRecordsToUpdate = new List<AudioMultiReference>();
            List<List<Audio>> rejectReadings = new List<List<Audio>>();

            foreach (string audioKey in audioKeys)
            {
                AudioMultiReference audioReference = ApplicationData.Repositories.DictionaryMultiAudio.Get(
                    audioKey, preferedMediaLanguageID);
                List<AudioInstance> audioInstances = null;

                //if (audioKey == "orado")
                //    ApplicationData.Global.PutConsoleMessage("GetAudioReadings: " + audioKey);

                if ((audioReference == null) || (audioReference.AudioInstanceCount() == 0))
                {
                    // If we are an entry with no instances, just delete it and start over.
                    if ((audioReference != null) && (audioReference.AudioInstanceCount() == 0))
                        ApplicationData.Repositories.DictionaryMultiAudio.DeleteKey(audioKey, preferedMediaLanguageID);

                    if (WordAudioMode == "MissingLazy")
                        continue;
                    else
                    {
                        string wordAudioSource = (IsSynthesizeMissingAudio ? AudioInstance.DontCareSourceName : AudioInstance.ForvoSourceName);

                        if (!GetWordAudio(
                                audioKey,
                                preferedMediaLanguageID,
                                wordAudioSource,
                                voice,
                                out audioReference))
                            continue;
                    }
                }

                audioInstances = audioReference.CloneAudioInstances();

                if ((audioInstances == null) || (audioInstances.Count() == 0))
                    continue;

                audioInstances.Sort(AudioAttributeComparerStatic);

                // Pull out one for each gender and region.
                // Rely on sort order (NotSynthesized, Region, DontUseVotes, UseVotes, SourceVotes, Country) to get the best voted instances.
                HashSet<string> usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<AudioInstance> useAudioInstances = new List<AudioInstance>();
                List<AudioInstance> rejectAudioInstances = new List<AudioInstance>();

                if ((AudioTags != null) && (AudioTags.Count() != 0) && audioReference.HasTaggedAudioInstances(AudioTags))
                {
                    foreach (AudioInstance audioInstance in audioInstances)
                    {
                        if (audioInstance.IsTagged("MTCReject"))
                            continue;

                        bool isUse = true;

                        if (!audioInstance.IsAnyTagged(AudioTags))
                            isUse = false;

                        if (isUse)
                            useAudioInstances.Add(audioInstance);
                        else
                            rejectAudioInstances.Add(audioInstance);
                    }
                }
                else
                {
                    foreach (AudioInstance audioInstance in audioInstances)
                    {
                        if (audioInstance.IsTagged("MTCReject"))
                            continue;

                        bool isUse = true;

                        if (audioInstance.SourceName == AudioInstance.SynthesizedSourceName)
                            isUse = false;

                        if (audioInstance.DontUseVotes != 0)
                            isUse = false;

                        string region = audioInstance.GetAttribute(AudioInstance.Region);

                        if (String.IsNullOrEmpty(region))
                            isUse = false;

                        string gender = audioInstance.GetAttribute(AudioInstance.Gender);

                        if (String.IsNullOrEmpty(gender))
                            isUse = false;

                        string key = region + gender;

                        if (!usedKeys.Add(key))
                            isUse = false;

                        if (isUse)
                            useAudioInstances.Add(audioInstance);
                        else
                            rejectAudioInstances.Add(audioInstance);
                    }
                }

                if ((useAudioInstances.Count() != 0) && (AudioRejectFileNames != null) && (AudioRejectFileNames.Count() != 0))
                {
                    foreach (string fileName in AudioRejectFileNames)
                    {
                        if (String.IsNullOrEmpty(fileName))
                            continue;

                        AudioInstance rejectInstance = useAudioInstances.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.FileName, fileName));

                        if (rejectInstance != null)
                        {
                            useAudioInstances.Remove(rejectInstance);
                            rejectAudioInstances.Add(rejectInstance);
                        }
                    }
                }

                if ((rejectAudioInstances.Count() != 0) && (AudioAddFileNames != null) && (AudioAddFileNames.Count() != 0))
                {
                    foreach (string fileName in AudioAddFileNames)
                    {
                        if (String.IsNullOrEmpty(fileName))
                            continue;

                        AudioInstance addInstance = rejectAudioInstances.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.FileName, fileName));

                        if (addInstance != null)
                        {
                            useAudioInstances.Add(addInstance);
                            rejectAudioInstances.Remove(addInstance);
                        }
                    }
                }

                if (useAudioInstances.Count() == 0)
                {
                    AudioInstance synthesizedInstance = audioInstances.FirstOrDefault(x => x.SourceName == AudioInstance.SynthesizedSourceName);
                    if (synthesizedInstance != null)
                    {
                        useAudioInstances.Add(synthesizedInstance);
                        rejectAudioInstances.Remove(synthesizedInstance);
                    }
                    else
                    {
                        if (IsSynthesizeMissingAudio)
                        {
                            if (GetWordAudioSynthesizer(
                                    audioKey,
                                    preferedMediaLanguageID,
                                    voice,
                                    ref audioReference))
                            {
                                audioInstances = audioReference.GetAudioInstancesBySynthesizer();
                                useAudioInstances.AddRange(audioInstances);

                                foreach (AudioInstance anInstance in audioInstances)
                                    rejectAudioInstances.Remove(anInstance);
                            }
                        }
                        else if (rejectAudioInstances.Count() != 0)
                        {
                            PutMessage("No audio instances for \"" + audioKey +
                                "\" and IsSynthesizeMissingAudio is off. Using first rejected audio instead: " +
                                rejectAudioInstances.First().FileName);
                            useAudioInstances.Add(rejectAudioInstances.First());
                            rejectAudioInstances.Remove(rejectAudioInstances.First());
                        }
                    }
                }

                if ((AudioTags != null) && (AudioTags.Count() != 0))
                {
                    foreach (AudioInstance audioInstance in useAudioInstances)
                        audioInstance.SetTags(AudioTags);

                    foreach (AudioInstance audioInstance in rejectAudioInstances)
                        audioInstance.DeleteTags(AudioTags);

                    if (audioReference.Modified && !audioRecordsToUpdate.Contains(audioReference))
                    {
                        audioReference.TouchAndClearModified();
                        audioRecordsToUpdate.Add(audioReference);
                    }
                }

                AudioReading audioReading = new AudioReading();
                List<Audio> instances = null;
                List<Audio> rejects = null;
                int modeCount = (IsIncludeRejectedAudio ? 2 : 1);

                for (int mode = 0; mode < modeCount; mode++)
                {
                    List<AudioInstance> modeInstances = (mode == 0 ? useAudioInstances : rejectAudioInstances);

                    foreach (AudioInstance audioInstance in modeInstances)
                    {
                        string mediaFileName = audioInstance.FileName;
                        string mimeType = audioInstance.MimeType;
                        string audioFilePath =
                            MediaUtilities.ConcatenateFilePath(
                                ApplicationData.ContentPath,
                                MediaUtilities.GetDictionaryAudioFilePath(preferedMediaLanguageID, mediaFileName));
                        bool audioExists = FileSingleton.Exists(audioFilePath);

                        if (audioExists)
                        {
                            Audio audio = new Audio();

                            audio.file = mediaFileName;
                            audio.gender = audioInstance.GetAttribute(AudioInstance.Gender);
                            audio.region = audioInstance.GetAttribute(AudioInstance.Region);
                            audio.country = audioInstance.GetAttribute(AudioInstance.Country);
                            audio.source = audioInstance.SourceName;

                            if (mode == 0)
                            {
                                if (instances == null)
                                    instances = new List<Audio>();

                                instances.Add(audio);
                            }
                            else
                            {
                                if (rejects == null)
                                    rejects = new List<Audio>();

                                rejects.Add(audio);
                            }
                        }
                    }
                }

                if ((instances != null) && (instances.Count() != 0))
                {
                    audioReading.instances = instances.ToArray();

                    if (audioReadings == null)
                        audioReadings = new List<AudioReading>();

                    audioReadings.Add(audioReading);
                }

                rejectReadings.Add(rejects);
            }

            if (audioRecordsToUpdate.Count() != 0)
            {
                if (!Repositories.DictionaryMultiAudio.UpdateList(audioRecordsToUpdate, preferedMediaLanguageID))
                    PutError("Error updating audio references.");
            }

            List<List<Audio>> testAudioRejects;

            if (!AudioRejects.TryGetValue(targetWord, out testAudioRejects))
                AudioRejects.Add(targetWord, rejectReadings);

            return audioReadings;
        }

        public AudioReading[] GetLazyAudioReadings(
            string audioKey,
            LanguageID targetLanguageID)
        {
            LanguageID preferedMediaLanguageID = LanguageLookup.GetPreferedMediaLanguageID(targetLanguageID);
            Voice voice =
                (ApplicationData.VoiceList != null
                    ? ApplicationData.VoiceList.GetVoiceByLanguageID(preferedMediaLanguageID)
                    : null);
            List<AudioReading> audioReadings = null;
            List<AudioMultiReference> audioRecordsToUpdate = new List<AudioMultiReference>();
            List<List<Audio>> rejectReadings = new List<List<Audio>>();
            AudioMultiReference audioReference = ApplicationData.Repositories.DictionaryMultiAudio.Get(
                audioKey, preferedMediaLanguageID);
            List<AudioInstance> audioInstances = null;

            //if (audioKey == "orado")
            //    ApplicationData.Global.PutConsoleMessage("GetAudioReadings: " + audioKey);

            if ((audioReference == null) || (audioReference.AudioInstanceCount() == 0))
            {
                // If we are an entry with no instances, just delete it and start over.
                if ((audioReference != null) && (audioReference.AudioInstanceCount() == 0))
                    ApplicationData.Repositories.DictionaryMultiAudio.DeleteKey(audioKey, preferedMediaLanguageID);

                string wordAudioSource = (IsSynthesizeMissingAudio ? AudioInstance.DontCareSourceName : AudioInstance.ForvoSourceName);

                if (!GetWordAudio(
                        audioKey,
                        preferedMediaLanguageID,
                        wordAudioSource,
                        voice,
                        out audioReference))
                    return null;
            }

            if (audioReference == null)
                return null;

            audioInstances = audioReference.CloneAudioInstances();

            if ((audioInstances == null) || (audioInstances.Count() == 0))
                return null;

            audioInstances.Sort(AudioAttributeComparerStatic);

            // Pull out one for each gender and region.
            // Rely on sort order (NotSynthesized, Region, DontUseVotes, UseVotes, SourceVotes, Country) to get the best voted instances.
            HashSet<string> usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<AudioInstance> useAudioInstances = new List<AudioInstance>();
            List<AudioInstance> rejectAudioInstances = new List<AudioInstance>();

            if ((AudioTags != null) && (AudioTags.Count() != 0) && audioReference.HasTaggedAudioInstances(AudioTags))
            {
                foreach (AudioInstance audioInstance in audioInstances)
                {
                    if (audioInstance.IsTagged("MTCReject"))
                        continue;

                    bool isUse = true;

                    if (!audioInstance.IsAnyTagged(AudioTags))
                        isUse = false;

                    if (isUse)
                        useAudioInstances.Add(audioInstance);
                    else
                        rejectAudioInstances.Add(audioInstance);
                }
            }
            else
            {
                foreach (AudioInstance audioInstance in audioInstances)
                {
                    if (audioInstance.IsTagged("MTCReject"))
                        continue;

                    bool isUse = true;

                    if (audioInstance.SourceName == AudioInstance.SynthesizedSourceName)
                        isUse = false;

                    if (audioInstance.DontUseVotes != 0)
                        isUse = false;

                    string region = audioInstance.GetAttribute(AudioInstance.Region);

                    if (String.IsNullOrEmpty(region))
                        isUse = false;

                    string gender = audioInstance.GetAttribute(AudioInstance.Gender);

                    if (String.IsNullOrEmpty(gender))
                        isUse = false;

                    string key = region + gender;

                    if (!usedKeys.Add(key))
                        isUse = false;

                    if (isUse)
                        useAudioInstances.Add(audioInstance);
                    else
                        rejectAudioInstances.Add(audioInstance);
                }
            }

            if ((useAudioInstances.Count() != 0) && (AudioRejectFileNames != null) && (AudioRejectFileNames.Count() != 0))
            {
                foreach (string fileName in AudioRejectFileNames)
                {
                    if (String.IsNullOrEmpty(fileName))
                        continue;

                    AudioInstance rejectInstance = useAudioInstances.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.FileName, fileName));

                    if (rejectInstance != null)
                    {
                        useAudioInstances.Remove(rejectInstance);
                        rejectAudioInstances.Add(rejectInstance);
                    }
                }
            }

            if ((rejectAudioInstances.Count() != 0) && (AudioAddFileNames != null) && (AudioAddFileNames.Count() != 0))
            {
                foreach (string fileName in AudioAddFileNames)
                {
                    if (String.IsNullOrEmpty(fileName))
                        continue;

                    AudioInstance addInstance = rejectAudioInstances.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(x.FileName, fileName));

                    if (addInstance != null)
                    {
                        useAudioInstances.Add(addInstance);
                        rejectAudioInstances.Remove(addInstance);
                    }
                }
            }

            if (useAudioInstances.Count() == 0)
            {
                AudioInstance synthesizedInstance = audioInstances.FirstOrDefault(x => x.SourceName == AudioInstance.SynthesizedSourceName);
                if (synthesizedInstance != null)
                {
                    useAudioInstances.Add(synthesizedInstance);
                    rejectAudioInstances.Remove(synthesizedInstance);
                }
                else
                {
                    if (IsSynthesizeMissingAudio)
                    {
                        if (GetWordAudioSynthesizer(
                                audioKey,
                                preferedMediaLanguageID,
                                voice,
                                ref audioReference))
                        {
                            audioInstances = audioReference.GetAudioInstancesBySynthesizer();
                            useAudioInstances.AddRange(audioInstances);

                            foreach (AudioInstance anInstance in audioInstances)
                                rejectAudioInstances.Remove(anInstance);
                        }
                    }
                    else if (rejectAudioInstances.Count() != 0)
                    {
                        PutMessage("No audio instances for \"" + audioKey +
                            "\" and IsSynthesizeMissingAudio is off. Using first rejected audio instead: " +
                            rejectAudioInstances.First().FileName);
                        useAudioInstances.Add(rejectAudioInstances.First());
                        rejectAudioInstances.Remove(rejectAudioInstances.First());
                    }
                }
            }

            if ((AudioTags != null) && (AudioTags.Count() != 0))
            {
                foreach (AudioInstance audioInstance in useAudioInstances)
                    audioInstance.SetTags(AudioTags);

                foreach (AudioInstance audioInstance in rejectAudioInstances)
                    audioInstance.DeleteTags(AudioTags);

                if (audioReference.Modified && !audioRecordsToUpdate.Contains(audioReference))
                {
                    audioReference.TouchAndClearModified();
                    audioRecordsToUpdate.Add(audioReference);
                }
            }

            AudioReading audioReading = new AudioReading();
            List<Audio> instances = null;
            List<Audio> rejects = null;
            int modeCount = (IsIncludeRejectedAudio ? 2 : 1);

            for (int mode = 0; mode < modeCount; mode++)
            {
                List<AudioInstance> modeInstances = (mode == 0 ? useAudioInstances : rejectAudioInstances);

                foreach (AudioInstance audioInstance in modeInstances)
                {
                    string mediaFileName = audioInstance.FileName;
                    string mimeType = audioInstance.MimeType;
                    string audioFilePath =
                        MediaUtilities.ConcatenateFilePath(
                            ApplicationData.ContentPath,
                            MediaUtilities.GetDictionaryAudioFilePath(preferedMediaLanguageID, mediaFileName));
                    bool audioExists = FileSingleton.Exists(audioFilePath);

                    if (audioExists)
                    {
                        Audio audio = new Audio();

                        audio.file = mediaFileName;
                        audio.gender = audioInstance.GetAttribute(AudioInstance.Gender);
                        audio.region = audioInstance.GetAttribute(AudioInstance.Region);
                        audio.country = audioInstance.GetAttribute(AudioInstance.Country);
                        audio.source = audioInstance.SourceName;

                        if (mode == 0)
                        {
                            if (instances == null)
                                instances = new List<Audio>();

                            instances.Add(audio);
                        }
                        else
                        {
                            if (rejects == null)
                                rejects = new List<Audio>();

                            rejects.Add(audio);
                        }
                    }
                }
            }

            if ((instances != null) && (instances.Count() != 0))
            {
                audioReading.instances = instances.ToArray();

                if (audioReadings == null)
                    audioReadings = new List<AudioReading>();

                audioReadings.Add(audioReading);
            }

            rejectReadings.Add(rejects);

            if (audioRecordsToUpdate.Count() != 0)
            {
                if (!Repositories.DictionaryMultiAudio.UpdateList(audioRecordsToUpdate, preferedMediaLanguageID))
                    PutError("Error updating audio references.");
            }

            return audioReadings != null ? audioReadings.ToArray() : new AudioReading[0];
        }

        protected bool GetWordAudio(
            string word,
            LanguageID languageID,
            string wordAudioSource,
            Voice voice,
            out AudioMultiReference audioRecord)
        {
            bool returnValue = false;

            audioRecord = null;

            if (wordAudioSource == AudioReference.SynthesizedSourceName)
                returnValue = GetWordAudioSynthesizer(word, languageID, voice, ref audioRecord);
            else if (wordAudioSource == AudioReference.ForvoSourceName)
                returnValue = GetWordAudioForvo(word, languageID, out audioRecord);
            else if (wordAudioSource == AudioReference.DontCareSourceName)
            {
                returnValue = GetWordAudioForvo(word, languageID, out audioRecord);

                if (!returnValue)
                    returnValue = GetWordAudioSynthesizer(word, languageID, voice, ref audioRecord);
            }

            return returnValue;
        }

        protected bool GetWordAudioSynthesizer(
            string word,
            LanguageID languageID,
            Voice voice,
            ref AudioMultiReference audioRecord)
        {
            bool isNew = false;

            AudioInstance audioInstance = null;

            if (audioRecord == null)
            {
                audioRecord = Repositories.DictionaryMultiAudio.GetAudio(word, languageID);

                if (audioRecord == null)
                {
                    audioRecord = new AudioMultiReference(
                        word,
                        languageID,
                        new List<AudioInstance>());
                    isNew = true;
                }
            }

            audioInstance = audioRecord.GetAudioInstanceBySourceAndAttribute(
                AudioInstance.SynthesizedSourceName,
                AudioInstance.Speaker,
                voice.GetAttribute(AudioInstance.Speaker));

            if (audioInstance != null)
                return true;

            audioInstance = new AudioInstance(
                word,
                Owner,
                MediaUtilities.MimeTypeMp3,
                audioRecord.AllocateAudioFileName(),
                AudioInstance.SynthesizedSourceName,
                voice.CloneAttributes());

            string audioFilePath = audioInstance.GetFilePath(languageID);

            bool entryHasAudio = NodeUtilities.AddSynthesizedVoiceDefault(
                word,
                audioFilePath,
                languageID);

            if (entryHasAudio)
                audioRecord.AddAudioInstance(audioInstance);
            else
            {
                PutError("Audio synthesis failed for: " + word);
                audioRecord = null;
                return false;
            }

            audioRecord.TouchAndClearModified();
            audioInstance.TouchAndClearModified();

            if (isNew)
            {
                try
                {
                    if (Repositories.DictionaryMultiAudio.Add(audioRecord, languageID))
                        PutMessage("Added sythesized audio for: " + word);
                    else
                        PutError("Error adding audio record for: " + word);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Exception while adding audio record", exc);
                }
            }
            else
            {
                try
                {
                    if (Repositories.DictionaryMultiAudio.Update(audioRecord, languageID))
                        PutMessage("Updated sythesized audio for: " + word);
                    else
                        PutError("Error updating audio record for: " + word);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Exception while updating audio record", exc);
                }
            }

            return entryHasAudio;
        }

        protected bool GetWordAudioForvo(
            string word,
            LanguageID preferedMediaLanguage,
            out AudioMultiReference audioRecord)
        {
            string formatType = "Crawler";

            audioRecord = null;

            FormatCrawler importFormatObject = (FormatCrawler)ApplicationData.Formats.Create(formatType, "AudioReference", "Audio",
                UserRecord, UserProfile, Repositories, LanguageUtilities, NodeUtilities);

            if (importFormatObject == null)
            {
                PutError("Failed to create format object.");
                return false;
            }

            importFormatObject.SetUpDumpStringCheck();

            if (importFormatObject.WebCrawler != null)
                importFormatObject.WebCrawler.SetUpDumpStringCheck();

            importFormatObject.WebFormatType = "Forvo Audio";
            importFormatObject.ImportExportType = "Web";

            importFormatObject.InitializeCrawler();

            importFormatObject.Arguments = new List<FormatArgument>();

            string Word = word;
            string WordPrompt = "Single word";
            string WordHelp = "Enter a single word to get audio for.";

            importFormatObject.SetArgument("Word", "string", "rw", Word,
                WordPrompt, WordHelp, null, null);

            importFormatObject.LoadFromArguments();

            Crawler importCrawlerObject = importFormatObject.WebCrawler;

            int progressCount = 0;

            importFormatObject.InitializeProgress("Read", false, progressCount);

            try
            {
                importFormatObject.Read(null);

                if (importFormatObject.ReadObjects != null)
                    audioRecord = importFormatObject.ReadObjects.FirstOrDefault() as AudioMultiReference;
                else
                    return false;
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception during import", exc);
                return false;
            }

            importFormatObject.FinishProgress("Read", false);

            return true;
        }

        protected bool CreateGlossaries(List<WordList> wordLists)
        {
            bool returnValue = true;

            LanguageID targetLanguageID = TargetLanguageID;
            LanguageID hostLanguageID = HostLanguageID;

            UpdateProgressElapsed("Creating glossary for "
                + targetLanguageID.LanguageName(UILanguageID)
                + " and "
                + hostLanguageID.LanguageName(UILanguageID) + "...");

            GlossaryData glossaryData = GetGlossaryData(targetLanguageID, hostLanguageID);

            if ((glossaryData.EntryList == null) || (glossaryData.EntryList.Count() == 0))
            {
                PutMessage("No glossary data for target language " + targetLanguageID.LanguageName(UILanguageID));
                //returnValue = false;
                return false;
            }

            WordList wordList = CreateWordList(targetLanguageID, hostLanguageID, glossaryData);

            if (wordList == null)
                return false;
            else if (wordList.words.Length == 0)
            {
                PutError("No words in word list for languages "
                    + targetLanguageID.LanguageName(UILanguageID)
                    + " and "
                    + hostLanguageID.LanguageName(UILanguageID)
                    + ".");
                return false;
            }

            wordLists.Add(wordList);

            return returnValue;
        }

        protected WordList CreateWordList(
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            GlossaryData glossaryData)
        {
            string targetLanguageCode = LanguageCodes.GetCode_ll_CC_FromCode2(targetLanguageID.LanguageCultureExtensionCode);
            WordList wordList = new WordList();
            List<Word> targetWords = new List<Word>();
            List<LanguageID> targetAlternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(targetLanguageID);
            List<LanguageID> hostAlternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(hostLanguageID);
            List<Inflection> inflections = new List<Inflection>();
            Dictionary<string, int> inflectionsIndexDictionary = new Dictionary<string, int>();
#if FUTURE
            List<PartOfSpeech> partsOfSpeech = new List<PartOfSpeech>();
            Dictionary<string, int> partsOfSpeechIndexDictionary = new Dictionary<string, int>();
#endif
            LanguageID preferedMediaLanguageID = LanguageLookup.GetPreferedMediaLanguageID(targetLanguageID);
            Voice voice =
                (ApplicationData.VoiceList != null
                    ? ApplicationData.VoiceList.GetVoiceByLanguageID(preferedMediaLanguageID)
                    : null);
            string mediaURLPath = String.Empty;
            int wordIndex;

            SourceNames.Clear();
            SourceToIndexMap.Clear();

            for (wordIndex = 0; wordIndex < glossaryData.EntryList.Count(); wordIndex++)
            {
                DictionaryEntry dictionaryEntry = glossaryData.EntryList[wordIndex];

                Word word = CreateWord(
                    dictionaryEntry,
                    targetLanguageID,
                    targetAlternateLanguageIDs,
                    hostLanguageID,
                    hostAlternateLanguageIDs,
                    preferedMediaLanguageID,
                    voice,
#if FUTURE
                    partsOfSpeechIndexDictionary,
                    partsOfSpeech,
#endif
                    inflectionsIndexDictionary,
                    inflections);

                targetWords.Add(word);
            }

            // Shorten the names.
            for (int i = 0; i < SourceNames.Count(); i++)
            {
                string name = SourceNames[i];
                switch (name)
                {
                    case "Passage Glossary":
                        name = "Glossary";
                        break;
                    case "Google Translate":
                        name = "Google";
                        break;
                    case "Translator Dictionary":
                        name = "Google";
                        break;
                    case "Translator Conversion":
                        name = "Conversion";
                        break;
                    case "Translator Database":
                        name = "Database";
                        break;
                    case "Translator Database Cache":
                        name = "DBCache";
                        break;
                    case "Translator Memory Cache":
                        name = "MemCache";
                        break;
                    default:
                        break;
                }
                SourceNames[i] = name;
            }

            wordList.words = targetWords.ToArray();
            wordList.mediaPath = mediaURLPath;
            wordList.inflections = inflections.ToArray();
            wordList.sourceNames = SourceNames.ToArray();
#if FUTURE
            wordList.PartsOfSpeech = partsOfSpeech.ToArray();
#endif

            return wordList;
        }

        public static string CreateWordBreakpoint = null;

        protected Word CreateWord(
            DictionaryEntry dictionaryEntry,
            LanguageID targetLanguageID,
            List<LanguageID> targetAlternateLanguageIDs,
            LanguageID hostLanguageID,
            List<LanguageID> hostAlternateLanguageIDs,
            LanguageID preferedMediaLanguageID,
            Voice voice,
#if FUTURE
            Dictionary<string, int> partsOfSpeechIndexDictionary,
            List<PartOfSpeech> partsOfSpeech,
#endif
            Dictionary<string, int> inflectionsIndexDictionary,
            List<Inflection> inflections)
        {
            string targetWord = dictionaryEntry.KeyString;
            List<AudioReading> audioReadings = null;
            List<Translation> translations;

            if (!String.IsNullOrEmpty(CreateWordBreakpoint) && (targetWord == CreateWordBreakpoint))
                ApplicationData.Global.PutConsoleMessage("CreateWord breakpoint hit: " + targetWord);

            // This needs fixing for multiple readings
            audioReadings = GetAudioReadings(
                targetWord,
                new List<string>() { targetWord },
                targetLanguageID,
                preferedMediaLanguageID,
                voice);

            translations = GetTranslations(
                dictionaryEntry,
                hostLanguageID,
                targetAlternateLanguageIDs,
#if FUTURE
                partsOfSpeechIndexDictionary,
                partsOfSpeech,
#endif
                inflectionsIndexDictionary,
                inflections);

            Word word = new Word();
            word.text = targetWord;

            if (audioReadings != null)
                word.audioReadings = audioReadings.ToArray();
            else
                word.audioReadings = null;

            word.translations = translations.ToArray();

            return word;
        }

        protected List<Translation> GetTranslations(
            DictionaryEntry dictionaryEntry,
            LanguageID hostLanguageID,
            List<LanguageID> alternateLanguageIDs,
#if FUTURE
            Dictionary<string, int> partsOfSpeechIndexDictionary,
            List<PartOfSpeech> partsOfSpeech,
#endif
            Dictionary<string, int> inflectionsIndexDictionary,
            List<Inflection> inflections)
        {
            List<Translation> translations = new List<Translation>();
            int senseIndex = 0;
            int maxSourceIDCount = dictionaryEntry.MaxSourceIDCount(hostLanguageID);
            int maxFrequency = dictionaryEntry.MaxFrequency(hostLanguageID);
            int frequencyLimit = int.MaxValue;
            int translationCount = 0;
            Translation translation = null;

            if ((ExclusionRatio != 0) && (maxFrequency != 0))
                frequencyLimit = maxFrequency / ExclusionRatio;

            foreach (JTLanguageModelsPortable.Dictionary.Sense sense in dictionaryEntry.Senses)
            {
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                if (languageSynonyms == null)
                    continue;

                List<LanguageSynonyms> alternateLanguageSynonymsList = null;

                if (alternateLanguageIDs != null)
                {
                    alternateLanguageSynonymsList = new List<LanguageSynonyms>();

                    foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                    {
                        LanguageSynonyms alternateLanguageSynonyms = sense.GetLanguageSynonyms(alternateLanguageID);

                        if (alternateLanguageSynonyms == null)
                            continue;

                        alternateLanguageSynonymsList.Add(alternateLanguageSynonyms);
                    }
                }

                int synonymCount = languageSynonyms.SynonymCount;
                int synonymIndex;
                string category = sense.CategoryString;
                int reading = sense.Reading;
#if FUTURE
                int partOfSpeechIndex = GetPartOfSpeechIndex(
                    sense,
                    partsOfSpeechIndexDictionary,
                    partsOfSpeech);
#endif
                int lemmaIndex = -1;
                int inflectionIndex = -1;

                GetInflectionAndLemmaIndex(
                    sense,
                    TargetLanguageID,
                    hostLanguageID,
                    inflectionsIndexDictionary,
                    inflections,
                    out inflectionIndex,
                    out lemmaIndex);

                for (synonymIndex = 0; synonymIndex < synonymCount; synonymIndex++)
                {
                    ProbableMeaning probableSynonym = languageSynonyms.GetProbableSynonymIndexed(synonymIndex);
                    string synonymText = probableSynonym.Meaning;
                    List<string> alternateTexts = null;

                    if (maxSourceIDCount > 1)
                    {
                        if (probableSynonym.SourceIDCount() <= 1)
                            continue;
                    }

                    if (probableSynonym.Frequency > 0)
                    {
                        if (probableSynonym.Frequency <= frequencyLimit)
                            continue;
                    }

                    if (MaxTranslationWords != 0)
                    {
                        int wordCount = TextUtilities.GetWordCount(synonymText);

                        if (wordCount > MaxTranslationWords)
                            continue;
                    }

                    if (alternateLanguageSynonymsList != null)
                    {
                        alternateTexts = new List<string>();

                        foreach (LanguageSynonyms alternateLanguageSynonyms in alternateLanguageSynonymsList)
                        {
                            string alternateText = alternateLanguageSynonyms.GetSynonymIndexed(synonymIndex);
                            alternateTexts.Add(alternateText);
                        }
                    }

                    List<int> sourceIndexes = null;

                    if ((probableSynonym.SourceIDs != null) && (probableSynonym.SourceIDs.Count != 0))
                    {
                        sourceIndexes = new List<int>();

                        foreach (int sourceID in probableSynonym.SourceIDs)
                        {
                            string sourceName = ApplicationData.DictionarySources.GetByID(sourceID);

                            if (!String.IsNullOrEmpty(sourceName))
                            {
                                int sourceIndex;

                                if (!SourceToIndexMap.TryGetValue(sourceName, out sourceIndex))
                                {
                                    sourceIndex = SourceNames.Count();
                                    SourceNames.Add(sourceName);
                                    SourceToIndexMap.Add(sourceName, sourceIndex);
                                }

                                sourceIndexes.Add(sourceIndex);
                            }
                        }
                    }

                    translation = new Translation();
                    translation.text = synonymText;

                    if (reading > 0)
                        translation.reading = reading;
                    else
                        translation.reading = null;

                    if (lemmaIndex != -1)
                        translation.lemmaIndex = lemmaIndex;
                    else
                        translation.lemmaIndex = null;

                    if (inflectionIndex != -1)
                        translation.inflectionIndex = inflectionIndex;
                    else
                        translation.inflectionIndex = null;

                    if (probableSynonym.Frequency != 0)
                        translation.fitFrequency = probableSynonym.Frequency;
                    else
                        translation.fitFrequency = null;

                    if (sourceIndexes != null)
                        translation.sourceIndexes = sourceIndexes.ToArray();

#if FUTURE
                    translation.senseID = senseIndex;
                    translation.partOfSpeechIndex = partOfSpeechIndex;
#endif

                    translations.Add(translation);
                }

                senseIndex++;
            }

            int translationIndex;

            for (translationIndex = 0; translationIndex < translations.Count(); translationIndex++)
            {
                Translation t1 = translations[translationIndex];
                string t1s = t1.text;
                string t1ss = " " + t1s;

                for (int i = translationIndex + 1; i < translations.Count();)
                {
                    Translation t2 = translations[i];
                    string t2s = t2.text;
                    string t2ss = " " + t2s;

                    if (t1s.EndsWith(t2ss))
                    {
                        translations.RemoveAt(i);
                        continue;
                    }
                    else if (t2s.EndsWith(t1ss))
                    {
                        translations.RemoveAt(i);
                        translations.RemoveAt(translationIndex);
                        translations.Insert(translationIndex, t2);
                        continue;
                    }

                    i++;
                }
            }

            translationCount = translations.Count();

            if ((MaxTranslationCount != 0) && (translationCount > MaxTranslationCount))
                translations.RemoveRange(MaxTranslationCount, translationCount - MaxTranslationCount);

            return translations;
        }

        protected void FilterDictionaryEntry(
            GlossaryData glossaryData,
            ref DictionaryEntry dictionaryEntry,
            int glossaryIndex,
            LanguageID hostLanguageID)
        {
            if (glossaryData.IsFilteredEntry(dictionaryEntry, glossaryIndex))
                return;

            dictionaryEntry = new DictionaryEntry(dictionaryEntry);

            int senseIndex = 0;
            int senseCount = dictionaryEntry.SenseCount;
            int maxSourceIDCount = dictionaryEntry.MaxSourceIDCount(hostLanguageID);
            int maxFrequency = dictionaryEntry.MaxFrequency(hostLanguageID);
            int frequencyLimit = int.MaxValue;
            int translationCount = 0;
            List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>();
            bool hasTranslationFrequencyTable = (HostTool != null ? HostTool.HasFrequencyTable() : false);
            bool doTranslationFrequency = !IsScripture && hasTranslationFrequencyTable;
            int translationFrequency;

            if ((ExclusionRatio != 0) && (maxFrequency != 0))
                frequencyLimit = maxFrequency / ExclusionRatio;

            for (senseIndex = senseCount - 1; senseIndex >= 0; senseIndex--)
            {
                Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);
                LanguageSynonyms languageSynonyms = sense.GetLanguageSynonyms(hostLanguageID);

                if (languageSynonyms == null)
                    continue;

                int synonymCount = languageSynonyms.SynonymCount;
                int synonymIndex;
                string category = sense.CategoryString;
                int reading = sense.Reading;

                for (synonymIndex = synonymCount - 1; synonymIndex >= 0; synonymIndex--)
                {
                    ProbableMeaning probableSynonym = languageSynonyms.GetProbableSynonymIndexed(synonymIndex);
                    string synonymText = probableSynonym.Meaning;

                    if (doTranslationFrequency)
                    {
                        translationFrequency = HostTool.GetLowestPhraseWordFrequency(synonymText, "Inflection");

                        if ((translationFrequency == 0) && (languageSynonyms.ProbableSynonymCount != 1))
                        {
                            languageSynonyms.DeleteSynonymIndexed(synonymIndex);
                            continue;
                        }
                    }

                    if (maxSourceIDCount > 1)
                    {
                        if (probableSynonym.SourceIDCount() <= 1)
                        {
                            languageSynonyms.DeleteSynonymIndexed(synonymIndex);
                            continue;
                        }
                    }

                    if (probableSynonym.Frequency > 0)
                    {
                        if (probableSynonym.Frequency <= frequencyLimit)
                        {
                            languageSynonyms.DeleteSynonymIndexed(synonymIndex);
                            continue;
                        }
                    }

                    if (MaxTranslationWords != 0)
                    {
                        int wordCount = TextUtilities.GetWordCount(synonymText);

                        if (wordCount > MaxTranslationWords)
                        {
                            languageSynonyms.DeleteSynonymIndexed(synonymIndex);
                            continue;
                        }
                    }

                    List<int> sourceIndexes = null;

                    if ((probableSynonym.SourceIDs != null) && (probableSynonym.SourceIDs.Count != 0))
                    {
                        sourceIndexes = new List<int>();

                        foreach (int sourceID in probableSynonym.SourceIDs)
                        {
                            string sourceName = ApplicationData.DictionarySources.GetByID(sourceID);

                            if (!String.IsNullOrEmpty(sourceName))
                            {
                                int sourceIndex;

                                if (!SourceToIndexMap.TryGetValue(sourceName, out sourceIndex))
                                {
                                    sourceIndex = SourceNames.Count();
                                    SourceNames.Add(sourceName);
                                    SourceToIndexMap.Add(sourceName, sourceIndex);
                                }

                                sourceIndexes.Add(sourceIndex);
                            }
                        }
                    }

                    probableSynonyms.Add(probableSynonym);
                }

                if (languageSynonyms.ProbableSynonymCount == 0)
                    dictionaryEntry.DeleteSenseIndexed(senseIndex);
            }

            int translationIndex;

            for (translationIndex = 0; translationIndex < probableSynonyms.Count(); translationIndex++)
            {
                ProbableMeaning t1 = probableSynonyms[translationIndex];
                string t1s = t1.Meaning;
                string t1ss = " " + t1s;

                for (int i = translationIndex + 1; i < probableSynonyms.Count();)
                {
                    ProbableMeaning t2 = probableSynonyms[i];
                    string t2s = t2.Meaning;
                    string t2ss = " " + t2s;

                    if (t1s.EndsWith(t2ss))
                    {
                        probableSynonyms.RemoveAt(i);
                        dictionaryEntry.DeleteProbableSynonym(hostLanguageID, t2);
                        continue;
                    }
                    else if (t2s.EndsWith(t1ss))
                    {
                        probableSynonyms.RemoveAt(i);
                        dictionaryEntry.DeleteProbableSynonym(hostLanguageID, t2);
                        dictionaryEntry.FindAndReplaceProbableSynonym(t1, hostLanguageID, t2);
                        probableSynonyms.Insert(translationIndex, t2);
                        continue;
                    }

                    i++;
                }
            }

            translationCount = probableSynonyms.Count();

            if ((MaxTranslationCount != 0) && (translationCount > MaxTranslationCount))
            {
                int endIndex = translationCount - MaxTranslationCount;

                // At this point, translations are ordered lowest frequency first.
                for (translationIndex = 0; translationIndex < endIndex; translationIndex++)
                    dictionaryEntry.DeleteProbableSynonym(hostLanguageID, probableSynonyms[translationIndex]);
            }

            glossaryData.SetFilteredEntry(dictionaryEntry, glossaryIndex);
        }

        protected bool GetInflectionAndLemmaIndex(
            Sense sense,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            Dictionary<string, int> inflectionsIndexDictionary,
            List<Inflection> inflections,
            out int inflectionIndex,
            out int lemmaIndex)
        {
            bool returnValue = false;

            inflectionIndex = -1;
            lemmaIndex = -1;

            if (sense.HasInflections())
            {
                foreach (JTLanguageModelsPortable.Language.Inflection anInflection in sense.Inflections)
                {
                    Designator designation = anInflection.Designation;
                    string fullName = designation.Label;
                    string abbreviation = "";
                    string targetLemma = anInflection.GetDictionaryForm(targetLanguageID);

                    if (!inflectionsIndexDictionary.TryGetValue(fullName, out inflectionIndex))
                    {
                        Inflection inflection = new Inflection();
                        inflection.fullName = fullName;
                        List<NameValuePair> designators = new List<NameValuePair>();

                        if (designation.Classifications != null)
                        {
                            foreach (Classifier classifier in designation.Classifications)
                            {
                                NameValuePair nvp = new NameValuePair();
                                nvp.name = classifier.Name;
                                nvp.value = classifier.Text;
                                designators.Add(nvp);
                                abbreviation += GetInflectionCategoryAbbreviation(classifier.Name, classifier.Text);
                            }
                        }

                        inflection.abbreviation = abbreviation;
                        inflection.designators = designators.ToArray();
                        inflectionIndex = inflections.Count();
                        inflections.Add(inflection);
                        inflectionsIndexDictionary.Add(fullName, inflectionIndex);
                    }

                    if (!String.IsNullOrEmpty(targetLemma))
                        GetDictionaryFormEntry(targetLemma, targetLanguageID, hostLanguageID, out lemmaIndex);

                    returnValue = true;
                }
            }

            return returnValue;
        }

        public static string GlossaryLookupBreakpoint = null;

        List<DictionaryEntry> GlossaryLookup(
            string pattern,
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();
            DictionaryEntry dictionaryEntry;
            double dictionaryLookupTime = 0.0;
            double inflectionLookupTime = 0.0;
            double translateLookupTime = 0.0;

            if (!String.IsNullOrEmpty(GlossaryLookupBreakpoint) && (pattern == GlossaryLookupBreakpoint))
                ApplicationData.Global.PutConsoleMessage("GlossaryLookup breakpoint hit: " + pattern);

            TotalUniqueWordCount += 1;

            if (Glossary != null)
            {
                if (Glossary.TryGetValue(pattern, out dictionaryEntry))
                {
                    dictionaryEntries.Add(dictionaryEntry);
                    FoundInGlossaryCount += 1;
                    return dictionaryEntries;
                }
            }

            LanguageID targetLanguageID = targetLanguageIDs.First();
            LanguageID hostLanguageID = hostLanguageIDs.First();
            LanguageTranslatorSource translatorSource;

            if (ShowStatistics)
            {
                LookupDictionaryCount += 1;
                GlossaryLookupTimer.Start();
            }

            string normalizedPattern = TextUtilities.GetGenericNormalizedPunctuationString(pattern);

            dictionaryEntry = TargetTool.GetDictionaryLanguageEntry(normalizedPattern, targetLanguageID);

            if (ShowStatistics)
            {
                GlossaryLookupTimer.Stop();
                dictionaryLookupTime = GlossaryLookupTimer.GetTimeInSeconds();
                SumDictionaryLookupTime += dictionaryLookupTime;
            }

            if (dictionaryEntry != null)
            {
                dictionaryEntries.Add(dictionaryEntry);
                FoundInDictionaryCount += 1;
                goto exit;
            }

            if (ShowStatistics)
            {
                LookupInflectionCount += 1;
                GlossaryLookupTimer.Start();
            }

            dictionaryEntry = TargetTool.GetInflectionDictionaryLanguageEntry(pattern, targetLanguageID);

            if (ShowStatistics)
            {
                GlossaryLookupTimer.Stop();
                inflectionLookupTime = GlossaryLookupTimer.GetTimeInSeconds();
                SumInflectionLookupTime += inflectionLookupTime;
            }

            if (dictionaryEntry != null)
            {
                dictionaryEntries.Add(dictionaryEntry);
                RecognizedAsInflectionCount += 1;
                goto exit;
            }

            if (ShowStatistics)
                GlossaryLookupTimer.Start();

            dictionaryEntry = GetTranslationServiceEntry(
                pattern,
                targetLanguageID,
                hostLanguageID,
                out translatorSource);

            if (ShowStatistics)
            {
                GlossaryLookupTimer.Stop();
                translateLookupTime = GlossaryLookupTimer.GetTimeInSeconds();

                switch (translatorSource)
                {
                    default:
                    case LanguageTranslatorSource.NotFound:
                    case LanguageTranslatorSource.Service:
                        LookupGoogleTranslateCount += 1;
                        SumGoogleTranslateTime += translateLookupTime;
                        break;
                    case LanguageTranslatorSource.Dictionary:
                    case LanguageTranslatorSource.Conversion:
                    case LanguageTranslatorSource.Database:
                    case LanguageTranslatorSource.DatabaseCache:
                    case LanguageTranslatorSource.MemoryCache:
                        LookupTranslatorCacheCount += 1;
                        SumTranslatorCacheTime += translateLookupTime;
                        break;
                }
            }

            if (dictionaryEntry != null)
            {
                dictionaryEntries.Add(dictionaryEntry);

                switch (translatorSource)
                {
                    default:
                    case LanguageTranslatorSource.NotFound:
                        break;
                    case LanguageTranslatorSource.Service:
                        GoogleTranslateCount += 1;
                        break;
                    case LanguageTranslatorSource.Dictionary:
                    case LanguageTranslatorSource.Conversion:
                    case LanguageTranslatorSource.Database:
                    case LanguageTranslatorSource.DatabaseCache:
                    case LanguageTranslatorSource.MemoryCache:
                        TranslatorCacheCount += 1;
                        break;
                }
            }

        exit:

            return dictionaryEntries;
        }

        protected DictionaryEntry GetTranslationServiceEntry(
            string pattern,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            out LanguageTranslatorSource translatorSource)
        {
            translatorSource = LanguageTranslatorSource.NotFound;

            if (LanguageUtilities == null)
                return null;

            ILanguageTranslator translator = LanguageUtilities.Translator;

            if (translator == null)
                return null;

            List<string> resultList;
            string errorMessage;

            string normalizedPattern = TextUtilities.GetGenericNormalizedPunctuationString(pattern);

            if (!translator.TranslateStringWithAlternates(
                    "ContentTranslate",
                    null,
                    null,
                    normalizedPattern,
                    targetLanguageID,
                    hostLanguageID,
                    out resultList,
                    out translatorSource,
                    out errorMessage))
                return null;

            if ((resultList == null) || (resultList.Count() == 0))
                return null;

            DictionaryEntry dictionaryEntry = null;
            List<Sense> senses = null;
            Sense sense;
            int reading = 0;
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = null;
            int priorityLevel = 0;
            List<LanguageSynonyms> languageSynonymsList = null;
            LanguageSynonyms languageSynonyms = null;
            List<ProbableMeaning> probableSynonyms = null;
            ProbableMeaning probableSynonym = null;
            List<MultiLanguageString> examples = null;
            int sourceID = 0;
            int index = 0;

            switch (translatorSource)
            {
                default:
                case LanguageTranslatorSource.NotFound:
                    break;
                case LanguageTranslatorSource.Service:
                    sourceID = ApplicationData.GoogleTranslateSourceID;
                    break;
                case LanguageTranslatorSource.Dictionary:
                    sourceID = ApplicationData.TranslatorDictionarySourceID;
                    break;
                case LanguageTranslatorSource.Conversion:
                    sourceID = ApplicationData.TranslatorConversionSourceID;
                    break;
                case LanguageTranslatorSource.Database:
                    sourceID = ApplicationData.TranslatorDatabaseSourceID;
                    break;
                case LanguageTranslatorSource.DatabaseCache:
                    sourceID = ApplicationData.TranslatorDatabaseCacheSourceID;
                    break;
                case LanguageTranslatorSource.MemoryCache:
                    sourceID = ApplicationData.TranslatorMemoryCacheSourceID;
                    break;
            }

            foreach (string translation in resultList)
            {
                float resultProbability = 0.0f;
                int resultFrequency = 0;

                probableSynonym = new ProbableMeaning(
                    translation,
                    LexicalCategory.Unknown,
                    String.Empty,
                    resultProbability,
                    resultFrequency,
                    sourceID);

                if (index == 0)
                {
                    probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                    languageSynonyms = new LanguageSynonyms(HostLanguageID, probableSynonyms);
                    languageSynonymsList = new List<LanguageSynonyms>() { languageSynonyms };
                    sense = new Sense(
                        reading,
                        category,
                        categoryString,
                        priorityLevel,
                        languageSynonymsList,
                        examples);
                    senses = new List<Sense>() { sense };
                    dictionaryEntry = new DictionaryEntry(
                        pattern,
                        targetLanguageID,
                        null,
                        resultFrequency,
                        sourceID,
                        senses,
                        null);
                }
                else if (probableSynonyms.FirstOrDefault(x => x.MatchMeaning(translation)) == null)
                    probableSynonyms.Add(probableSynonym);

                index++;
            }

            return dictionaryEntry;
        }

        protected void ClearStatistics()
        {
            TotalUniqueWordCount = 0;
            FoundInGlossaryCount = 0;
            FoundInDictionaryCount = 0;
            RecognizedAsInflectionCount = 0;
            GoogleTranslateCount = 0;
            TranslatorCacheCount = 0;
            AverageDictionaryLookupTime = 0.0f;
            AverageInflectionLookupTime = 0.0f;
            AverageGoogleTranslateTime = 0.0f;
            AverageTranslatorCacheTime = 0.0f;
            LookupDictionaryCount = 0;
            LookupInflectionCount = 0;
            LookupGoogleTranslateCount = 0;
            LookupTranslatorCacheCount = 0;
            SumDictionaryLookupTime = 0.0;
            SumInflectionLookupTime = 0.0;
            SumGoogleTranslateTime = 0.0;
            SumTranslatorCacheTime = 0.0;
        }

        protected DictionaryEntry GetDictionaryFormEntry(
            string key,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            out int glossaryIndex)
        {
            GlossaryData glossaryData = GetGlossaryData(targetLanguageID, hostLanguageID);
            DictionaryEntry dictionaryEntry = null;

            glossaryIndex = -1;

            if (!glossaryData.GetEntry(key, out dictionaryEntry, out glossaryIndex))
            {
                DictionaryEntry dictionaryFormEntry = TargetTool.GetDictionaryLanguageEntry(key, TargetLanguageID);

                if (dictionaryFormEntry != null)
                {
                    DictionaryEntry clonedEntry = new DictionaryEntry(dictionaryFormEntry);
                    glossaryData.AddEntry(clonedEntry, out glossaryIndex);
                }
            }

            return dictionaryEntry;
        }

        protected static string[] InflectionCategoryAbbreviationInitializers =
        {
            /* Aspect */    "Imperfect", "Imp",  "Imperfect1", "Imp1",  "Imperfect2", "Imp2",  "Progressive", "Prog",  "Perfect", "Pfct",
            /* Gender */    "Masculine", "Masc",  "Feminine", "Fem",
            /* Mood */      "Indicative", "Ind",  "Conditional", "Cond",  "Subjunctive", "Subj",  "Imperative", "Imp",
            /* Number */    "Singular", "Sngl",  "Plural", "Plur",
            /* Person */    "First", "Frst",  "Second", "Scnd",  "Third", "Thrd",
            /* Polarity */  "Positive", "Pos",  "Negative", "Neg",
            /* Politeness */"Informal", "Infm",  "Informal1", "Infm1",  "Formal", "Form",  "Informal2", "Infm2",
            /* Special */   "Dictionary", "Dic",  "Stem", "Stm",  "Infinitive", "Infn",  "Gerund", "Ger",  "Participle", "Part",
            /* Tense */     "Present", "Pres",  "Past", "Pst",  "Future", "Fut",
            /* Contraction */ "Contraction", "Cont"
        };

        protected static Dictionary<string, string> InflectionCategoryAbbreviationMap = null;

        protected string GetInflectionCategoryAbbreviation(string classifierName, string classifierValue)
        {
            string abbrev;

            if (InflectionCategoryAbbreviationMap == null)
            {
                InflectionCategoryAbbreviationMap = new Dictionary<string, string>();

                int count = InflectionCategoryAbbreviationInitializers.Length;
                int index;

                for (index = 0; index < count; index += 2)
                    InflectionCategoryAbbreviationMap.Add(InflectionCategoryAbbreviationInitializers[index], InflectionCategoryAbbreviationInitializers[index + 1]);
            }

            if (!InflectionCategoryAbbreviationMap.TryGetValue(classifierValue, out abbrev))
            {
                int count = InflectionCategoryAbbreviationInitializers.Length;
                int index;

                for (index = count - 2; index >= 0; index -= 2)
                {
                    if (classifierValue.StartsWith(InflectionCategoryAbbreviationInitializers[index]))
                    {
                        abbrev = InflectionCategoryAbbreviationInitializers[index + 1];
                        break;
                    }
                }
            }

            if (String.IsNullOrEmpty(abbrev))
                abbrev = classifierValue.Substring(0, 3);

            return abbrev;
        }

        public override void LoadFromArguments()
        {
            /*
            base.LoadFromArguments();

            MaxTranslationCount = GetIntegerArgumentDefaulted("MaxTranslationCount", "flag", "r", MaxTranslationCount, MaxTranslationCountPrompt,
                MaxTranslationCountHelp);

            ExclusionRatio = GetIntegerArgumentDefaulted("ExclusionRatio", "flag", "r", ExclusionRatio, ExclusionRatioPrompt,
                ExclusionRatioHelp);

            MaxTranslationWords = GetIntegerArgumentDefaulted("MaxTranslationWords", "flag", "r", MaxTranslationWords, MaxTranslationWordsPrompt,
                MaxTranslationWordsHelp);

            TextUnitsSentences = GetFlagArgumentDefaulted("TextUnitsSentences", "flag", "r", TextUnitsSentences, TextUnitsSentencesPrompt,
                TextUnitsSentencesHelp, null, null);
            */

            ShowTranslation = GetFlagArgumentDefaulted("ShowTranslation", "flag", "r", ShowTranslation, ShowTranslationPrompt,
                ShowTranslationHelp, null, null);

            ShowStatistics = GetFlagArgumentDefaulted("ShowStatistics", "flag", "r", ShowStatistics, ShowStatisticsPrompt,
                ShowStatisticsHelp, null, null);

            ShowGlossary = GetFlagArgumentDefaulted("ShowGlossary", "flag", "r", ShowGlossary, ShowGlossaryPrompt,
                ShowGlossaryHelp, null, null);

            ShowJSON = GetFlagArgumentDefaulted("ShowJSON", "flag", "r", ShowJSON, ShowJSONPrompt,
                ShowJSONHelp, null, null);

            WordAudioMode = GetStringListArgumentDefaulted("WordAudioMode", "stringlist", "r",
                WordAudioMode, WordAudioModeStrings, WordAudioModePrompt,
                WordAudioModeHelp);

            //IsSynthesizeMissingAudio = GetFlagArgumentDefaulted("IsSynthesizeMissingAudio", "flag", "r",
            //    IsSynthesizeMissingAudio, IsSynthesizeMissingAudioPrompt,
            //    IsSynthesizeMissingAudioHelp, null, null);

            IsNoGlossary = GetFlagArgumentDefaulted("IsNoGlossary", "flag", "r", IsNoGlossary, IsNoGlossaryPrompt,
                IsNoGlossaryHelp, null, null);
        }

        public override void SaveToArguments()
        {
            /*
            base.SaveToArguments();

            SetIntegerArgument("MaxTranslationCount", "flag", "r", MaxTranslationCount, MaxTranslationCountPrompt,
                MaxTranslationCountHelp);

            SetIntegerArgument("ExclusionRatio", "flag", "r", ExclusionRatio, ExclusionRatioPrompt,
                ExclusionRatioHelp);

            SetIntegerArgument("MaxTranslationWords", "flag", "r", MaxTranslationWords, MaxTranslationWordsPrompt,
                MaxTranslationWordsHelp);

            SetFlagArgument("TextUnitsSentences", "flag", "r", TextUnitsSentences, TextUnitsSentencesPrompt,
                TextUnitsSentencesHelp, null, null);
            */

            SetFlagArgument("ShowTranslation", "flag", "r", ShowTranslation, ShowTranslationPrompt,
                ShowTranslationHelp, null, null);

            SetFlagArgument("ShowStatistics", "flag", "r", ShowStatistics, ShowStatisticsPrompt,
                ShowStatisticsHelp, null, null);

            SetFlagArgument("ShowGlossary", "flag", "r", ShowGlossary, ShowGlossaryPrompt,
                ShowGlossaryHelp, null, null);

            SetFlagArgument("ShowJSON", "flag", "r", ShowJSON, ShowJSONPrompt,
                ShowJSONHelp, null, null);

            SetStringListArgument("WordAudioMode", "stringlist", "r", WordAudioMode,
                WordAudioModeStrings, WordAudioModePrompt,
                WordAudioModeHelp);

            SetFlagArgument("IsNoGlossary", "flag", "r", IsNoGlossary, IsNoGlossaryPrompt,
                IsNoGlossaryHelp, null, null);
        }

        public static string PassageGlossarySourceName = "Passage Glossary";

        private static int _PassageGlossarySourceID = 0;
        public static int PassageGlossarySourceID
        {
            get
            {
                if (_PassageGlossarySourceID == 0)
                    _PassageGlossarySourceID = ApplicationData.DictionarySourcesLazy.Add(PassageGlossarySourceName);
                return _PassageGlossarySourceID;
            }
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            if (importExport == "Export")
                return false;

            switch (contentName)
            {
                case null:
                case "":
                    if (capability == "Support")
                        return true;
                    else if (capability == "WebText")
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string contentName, string capability)
        {
            return IsSupportedStatic(importExport, contentName, capability);
        }

        public static new string TypeStringStatic { get { return "Passage"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
