using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public enum QuickLookupSourceType
    {
        QuickFormat,     // Native quick format.
        CEDict,          // Chinese CEDict format.
        EDict            // Japanese EDict old format.
    }

    public class FormatQuickLookup : Format
    {
        public QuickLookupSourceType SourceType { get; set; }
        public Dictionary<string, string> QuickDictionary { get; set; }
        public LanguageID InputLanguageID { get; set; }
        public LanguageID OutputLanguageID { get; set; }
        DictionaryRepository Repository { get; set; }
        List<LanguageID> LanguageIDs { get; set; }
        public List<string> Errors = new List<string>();
        public static string DictionaryDirectoryUrl = "~/Content/Dictionary";
        public static string _DictionaryDirectory;
        public static string DictionaryDirectory
        {
            get
            {
                if (String.IsNullOrEmpty(_DictionaryDirectory))
                {
                    if (ApplicationData.MapToFilePath != null)
                        _DictionaryDirectory = ApplicationData.MapToFilePath(DictionaryDirectoryUrl);
                    else
                        _DictionaryDirectory = ApplicationData.PlatformPathSeparator + "jtlangexp"
                            + ApplicationData.PlatformPathSeparator + "JTLanguage"
                            + ApplicationData.PlatformPathSeparator + "Content"
                            + ApplicationData.PlatformPathSeparator + "Dictionary";
                }
                return _DictionaryDirectory;
            }
            set
            {
                _DictionaryDirectory = value;
            }
        }
        public static Dictionary<string, FormatQuickLookup> CachedDictionaries { get; set; }
        protected int LanguagePairCount;
        protected int LanguagePairIndex;
        protected int EntryCount;
        protected int EntryIndex;
        private static string FormatDescription = "Format used for representing different dlanguage ictionary formats,"
            + " namely: CEDict, EDict, and a generic format (QuickLookup - (input)|(output) line format)";

        public FormatQuickLookup()
            : base("QuickLookup", "FormatQuickLookup", FormatDescription, "Dictionary", String.Empty, String.Empty, ".qlu", null, null, null, null, null)
        {
        }

        public FormatQuickLookup(QuickLookupSourceType sourceType, string sourceFilePath)
            : base("QuickLookup", "FormatQuickLookup", FormatDescription, "Dictionary", String.Empty, String.Empty, ".qlu", null, null, null, null, null)
        {
            SourceType = sourceType;
            QuickDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Stream stream = FileSingleton.OpenRead(sourceFilePath);
            Read(stream);
        }

        public FormatQuickLookup(LanguageID inputLanguageID, LanguageID outputLanguageID)
            : base("QuickLookup", "FormatQuickLookup", FormatDescription, "Dictionary", String.Empty, String.Empty, ".qlu", null, null, null, null, null)
        {
            SourceType = QuickLookupSourceType.QuickFormat;
            QuickDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            InputLanguageID = inputLanguageID;
            OutputLanguageID = outputLanguageID;
            string filePath = DictionaryDirectory + ApplicationData.PlatformPathSeparator + inputLanguageID.SymbolName + "_to_" + outputLanguageID.SymbolName + DefaultFileExtension;
            Stream stream = FileSingleton.OpenRead(filePath);
            Read(stream);
        }

        public FormatQuickLookup(Dictionary<string, string> quickDictionary, LanguageID inputLanguageID, LanguageID outputLanguageID)
            : base("QuickLookup", "FormatQuickLookup", FormatDescription, "Dictionary", String.Empty, String.Empty, ".qlu", null, null, null, null, null)
        {
            SourceType = QuickLookupSourceType.QuickFormat;
            QuickDictionary = quickDictionary;
            InputLanguageID = inputLanguageID;
            OutputLanguageID = outputLanguageID;
        }

        public FormatQuickLookup(FormatQuickLookup other)
            : base(other)
        {
            SourceType = QuickLookupSourceType.QuickFormat;
            QuickDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            InputLanguageID = null;
            OutputLanguageID = null;
        }

        public override Format Clone()
        {
            return new FormatQuickLookup(this);
        }

        public override void Read(Stream stream)
        {
            switch (SourceType)
            {
                case QuickLookupSourceType.QuickFormat:
                    ReadQuickFormat(stream);
                    break;
                case QuickLookupSourceType.CEDict:
                    ReadCEDict(stream);
                    break;
                case QuickLookupSourceType.EDict:
                    ReadEDict(stream);
                    break;
                default:
                    break;
            }
        }

        public static char[] EntrySeparators = new char[] { '|' };

        public void ReadQuickFormat(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;

                //if (Timer != null)
                //    Timer.Start();

                // Load dictionary with canonical entries.
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    string[] fields = line.Split(EntrySeparators);
                    string input = fields[0];
                    string output = fields[1];
                    string testString;

                    if (!QuickDictionary.TryGetValue(input, out testString))
                        QuickDictionary.Add(input, output);
                }

                if (Timer != null)
                {
                    //Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }
            }
        }

        public void ReadCEDict(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;

                //if (Timer != null)
                //    Timer.Start();

                // Read header.
                string Header = reader.ReadLine();

                // Create two dictionary versions, input->output, output->input.
                Dictionary<string, string> ctPinyinDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> pinyinCTDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> csPinyinDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> pinyinCSDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> ctCSDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> csCTDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // Load dictionary with canonical entries.
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    int ctStartIndex = 0;
                    int ctEndIndex = line.IndexOf(" ");
                    int csStartIndex = ctEndIndex + 1;
                    int csEndIndex = line.IndexOf(" [");
                    int pinyinStartIndex = csEndIndex + 2;
                    int pinyinEndIndex = line.IndexOf("]");
                    string ct = line.Substring(ctStartIndex, ctEndIndex - ctStartIndex);
                    string cs = line.Substring(csStartIndex, csEndIndex - csStartIndex);
                    string pinyin = line.Substring(pinyinStartIndex, pinyinEndIndex - pinyinStartIndex);
                    string testString;

                    if (!ctPinyinDictionary.TryGetValue(ct, out testString))
                        ctPinyinDictionary.Add(ct, pinyin);

                    if (!pinyinCTDictionary.TryGetValue(pinyin, out testString))
                        pinyinCTDictionary.Add(pinyin, ct);

                    if (!csPinyinDictionary.TryGetValue(cs, out testString))
                        csPinyinDictionary.Add(cs, pinyin);

                    if (!pinyinCSDictionary.TryGetValue(pinyin, out testString))
                        pinyinCSDictionary.Add(pinyin, cs);

                    if (!ctCSDictionary.TryGetValue(ct, out testString))
                        ctCSDictionary.Add(ct, cs);

                    if (!csCTDictionary.TryGetValue(cs, out testString))
                        csCTDictionary.Add(cs, ct);
                }

                WriteQuickFormat(ctPinyinDictionary, LanguageLookup.ChineseTraditional, LanguageLookup.ChinesePinyin);
                WriteQuickFormat(pinyinCTDictionary, LanguageLookup.ChinesePinyin, LanguageLookup.ChineseTraditional);
                WriteQuickFormat(csPinyinDictionary, LanguageLookup.ChineseSimplified, LanguageLookup.ChinesePinyin);
                WriteQuickFormat(pinyinCSDictionary, LanguageLookup.ChinesePinyin, LanguageLookup.ChineseSimplified);
                WriteQuickFormat(ctCSDictionary, LanguageLookup.ChineseTraditional, LanguageLookup.ChineseSimplified);
                WriteQuickFormat(csCTDictionary, LanguageLookup.ChineseSimplified, LanguageLookup.ChineseTraditional);

                if (Timer != null)
                {
                    //Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }
            }
        }

        public void ReadEDict(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;

                //if (Timer != null)
                //    Timer.Start();

                // Read header.
                string Header = reader.ReadLine();

                // Create two dictionary versions, input->output, output->input.
                Dictionary<string, string> jaKanaDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> kanaJaDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> jaRomajiDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> romajiJaDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> kanaRomajiDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> romajiKanaDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                ConvertRomaji convertRomaji = new ConvertRomaji(LanguageLookup.JapaneseKana, '\0', null, false);

                int lineNumber = 1;

                // Load dictionary with canonical entries.
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    int inputStartIndex = 0;
                    int inputEndIndex = line.IndexOf(" [");
                    if (inputEndIndex == -1)
                        inputEndIndex = line.IndexOf(" /");
                    int outputStartIndex = inputEndIndex + 2;
                    int outputEndIndex = line.IndexOf("]");
                    string input = line.Substring(inputStartIndex, inputEndIndex - inputStartIndex);
                    string output = (outputEndIndex != -1 ? line.Substring(outputStartIndex, outputEndIndex - outputStartIndex) : input);
                    string romaji;
                    string testString;

                    if (!convertRomaji.To(out romaji, output))
                    {
                        Errors.Add(output + "|" + romaji);
                    }

                    if (!jaKanaDictionary.TryGetValue(input, out testString))
                        jaKanaDictionary.Add(input, output);

                    if (!kanaJaDictionary.TryGetValue(output, out testString))
                        kanaJaDictionary.Add(output, input);

                    if (!jaRomajiDictionary.TryGetValue(input, out testString))
                        jaRomajiDictionary.Add(input, romaji);

                    if (!romajiJaDictionary.TryGetValue(romaji, out testString))
                        romajiJaDictionary.Add(romaji, input);

                    if (!kanaRomajiDictionary.TryGetValue(output, out testString))
                        kanaRomajiDictionary.Add(output, romaji);

                    if (!romajiKanaDictionary.TryGetValue(romaji, out testString))
                        romajiKanaDictionary.Add(romaji, output);

                    lineNumber++;
                }

                WriteQuickFormat(jaKanaDictionary, LanguageLookup.Japanese, LanguageLookup.JapaneseKana);
                WriteQuickFormat(kanaJaDictionary, LanguageLookup.JapaneseKana, LanguageLookup.Japanese);
                WriteQuickFormat(jaRomajiDictionary, LanguageLookup.Japanese, LanguageLookup.JapaneseRomaji);
                WriteQuickFormat(romajiJaDictionary, LanguageLookup.JapaneseRomaji, LanguageLookup.Japanese);
                WriteQuickFormat(kanaRomajiDictionary, LanguageLookup.JapaneseKana, LanguageLookup.JapaneseRomaji);
                WriteQuickFormat(romajiKanaDictionary, LanguageLookup.JapaneseRomaji, LanguageLookup.JapaneseKana);

                if (Timer != null)
                {
                    //Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }
            }
        }

        public void ReadDictionary(DictionaryRepository dictionaryRepository, List<LanguageID> languageIDs)
        {
            int c = languageIDs.Count();
            int i, j;

            LanguagePairCount = (1 << (languageIDs.Count() - 1));
            LanguagePairIndex = 0;
            EntryCount = 0;
            EntryIndex = 0;
            //Timer = new SoftwareTimer();
            //Timer.Start();

            for (i = 0; i < c; i++)
            {
                InputLanguageID = languageIDs[i];
                EntryCount = 0;
                EntryIndex = 0;
                List<DictionaryEntry> entries = dictionaryRepository.GetAll(InputLanguageID);

                for (j = i + 1; j < c; j++)
                {
                    OutputLanguageID = languageIDs[j];
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (DictionaryEntry entry in entries)
                    {
                        string inputText = entry.GetTranslation(InputLanguageID);
                        string outputText = entry.GetTranslation(OutputLanguageID);
                        dictionary.Add(inputText, outputText);
                        EntryIndex++;
                    }

                    WriteQuickFormat(dictionary, InputLanguageID, OutputLanguageID);
                    dictionary.Clear();
                    LanguagePairIndex++;
                }
            }

            for (i = c - 1; i > 0; i--)
            {
                InputLanguageID = languageIDs[i];
                List<DictionaryEntry> entries = dictionaryRepository.GetAll(InputLanguageID);

                EntryCount = entries.Count() * (c - i - 1);
                EntryIndex = 0;

                for (j = i - 1; j >= 0; j--)
                {
                    OutputLanguageID = languageIDs[j];
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (DictionaryEntry entry in entries)
                    {
                        string inputText = entry.GetTranslation(InputLanguageID);
                        string outputText = entry.GetTranslation(OutputLanguageID);
                        dictionary.Add(inputText, outputText);
                        EntryIndex++;
                    }

                    WriteQuickFormat(dictionary, InputLanguageID, OutputLanguageID);
                    LanguagePairIndex++;
                }
            }

            //Timer.Stop();
        }

        public static void WriteQuickFormat(Dictionary<string, string> dictionary, LanguageID inputLanguageID, LanguageID outputLanguageID)
        {
            WriteQuickFormatNoUpdate(dictionary, inputLanguageID, outputLanguageID);
            UpdateQuickDictionary(new FormatQuickLookup(dictionary, inputLanguageID, outputLanguageID));
        }

        public static void WriteQuickFormatNoUpdate(Dictionary<string, string> dictionary, LanguageID inputLanguageID, LanguageID outputLanguageID)
        {
            string filePath = DictionaryDirectory + ApplicationData.PlatformPathSeparator + inputLanguageID.SymbolName + "_to_" + outputLanguageID.SymbolName + ".qlu";

            using (StreamWriter writer = FileSingleton.CreateText(filePath))
            {
                foreach (KeyValuePair<string, string> kvp in dictionary)
                {
                    writer.WriteLine(kvp.Key + "|" + kvp.Value);
                }
            }
        }

        public static FormatQuickLookup GetQuickDictionary(LanguageID inputLanguageID, LanguageID outputLanguageID)
        {
            string filePath = DictionaryDirectory + ApplicationData.PlatformPathSeparator + inputLanguageID.SymbolName + "_to_" + outputLanguageID.SymbolName + ".qlu";
            FormatQuickLookup format;

            if (CachedDictionaries == null)
                CachedDictionaries = new Dictionary<string, FormatQuickLookup>(StringComparer.OrdinalIgnoreCase);

            if (CachedDictionaries.TryGetValue(filePath, out format))
                return format;

            if (!FileSingleton.Exists(filePath))
                return null;

            format = new FormatQuickLookup(inputLanguageID, outputLanguageID);

            CachedDictionaries.Add(filePath, format);

            return format;
        }

        public static void UpdateQuickDictionary(FormatQuickLookup quickLookup)
        {
            string filePath = DictionaryDirectory + ApplicationData.PlatformPathSeparator + quickLookup.InputLanguageID.SymbolName + "_to_" + quickLookup.OutputLanguageID.SymbolName + ".qlu";

            if (CachedDictionaries == null)
                CachedDictionaries = new Dictionary<string, FormatQuickLookup>(StringComparer.OrdinalIgnoreCase);

            FormatQuickLookup format;

            if (CachedDictionaries.TryGetValue(filePath, out format))
                CachedDictionaries.Remove(filePath);

            CachedDictionaries.Add(filePath, quickLookup);
        }

        public static void UpdateDictionaryEntry(DictionaryEntry entry)
        {
            LanguageID languageID = entry.LanguageID;
            List<LanguageID> languageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);

            foreach (LanguageID lid in languageIDs)
            {
                if (lid == languageID)
                    continue;

                UpdateDictionaryEntry(entry, languageID, lid);
            }
        }

        public static void UpdateDictionaryEntry(DictionaryEntry entry, LanguageID inputLanguageID, LanguageID outputLanguageID)
        {
            FormatQuickLookup quickLookup = GetQuickDictionary(inputLanguageID, outputLanguageID);

            if (quickLookup != null)
            {
                string inputText = entry.GetTranslation(inputLanguageID);
                string outputText = entry.GetTranslation(outputLanguageID);
                string oldText;

                if (quickLookup.QuickDictionary.TryGetValue(inputText, out oldText))
                    quickLookup.QuickDictionary.Remove(inputText);

                quickLookup.QuickDictionary.Add(inputText, outputText);

                WriteQuickFormatNoUpdate(quickLookup.QuickDictionary, inputLanguageID, outputLanguageID);
            }
        }

        public override float GetProgress()
        {
            if (LanguagePairCount == 0)
                return 0.0f;

            float progress = ((float)LanguagePairIndex / LanguagePairCount) + (((float)EntryIndex / EntryCount) / LanguagePairCount);
            return progress;
        }

        public override string GetProgressMessage()
        {
            string message;

            if (InputLanguageID == null)
                message = "Waiting to start...";
            else if (EntryCount == 0)
                message = "Reading dictionary: " + InputLanguageID.LanguageName(LanguageLookup.English) + ".";
            else
                message = "Processing entry " + EntryIndex.ToString() + " of " + EntryCount.ToString() + " entries for language combination " + InputLanguageID.LanguageName(LanguageLookup.English) + "/" + OutputLanguageID.LanguageName(LanguageLookup.English) + ".";

            if (Timer != null)
                message += "  Elapsed time is: " + ElapsedTime.ToString();

            return message;
        }
    }
}
