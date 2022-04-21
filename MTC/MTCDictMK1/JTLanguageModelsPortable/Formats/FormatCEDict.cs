using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatCEDict : FormatDictionary
    {
        // CEDict header information.

        // #! version=1
        public int Version { get; set; }
        // #! subversion=0
        public int SubVersion { get; set; }
        // #! format=ts
        public string Format { get; set; }
        // #! charset=UTF-8
        public string CharSet { get; set; }
        // #! entries=98958
        public int Entries { get; set; }
        // #! publisher=MDBG
        public string Publisher { get; set; }
        // #! license=http://creativecommons.org/licenses/by-sa/3.0/
        public string License { get; set; }
        // #! date=2010-09-30T07:39:36Z
        public string Date { get; set; }
        // #! time=1285832376
        public string Time { get; set; }

        // Format data.
        private static string FormatDescription = "Format used for representing a Chinese dictionary.  See: http://cc-cedict.org/wiki";

        public FormatCEDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "FormatCEDict",
                FormatDescription,
                String.Empty,
                ".u8",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
            IsAddReciprocals = true;
        }

        public FormatCEDict(FormatCEDict other)
            : base(other)
        {
        }

        public FormatCEDict()
            : base(
                  "CEDict",
                  "FormatCEDict",
                  FormatDescription,
                  String.Empty,
                  ".u8",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
            IsAddReciprocals = true;
        }

        public override Format Clone()
        {
            return new FormatCEDict(this);
        }

        public override void Read(Stream stream)
        {
            try
            {
                PreRead(8);

                FileSize = (int)stream.Length;

                UpdateProgressElapsed("Reading stream ...");

                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    State = StateCode.Reading;

                    // Load dictionary with canonical entries.
                    while ((line = reader.ReadLine()) != null)
                        ReadLine(line);

                    // Convert dictionary to display form.
                    ConvertDictionaryToDisplay();

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
            if (line.StartsWith("#"))
            {
                if (line.StartsWith("#!"))
                    ReadDirective(line);
                // else ignore comment.
            }
            else if (!String.IsNullOrEmpty(line))
                ReadEntry(line);
        }

        protected override void ReadDirective(string line)
        {
            char[] delims = new char[] { ' ', '\t', '=' };
            string[] strings = line.Split(delims);
            int index;
            int count = strings.Count();
            string key = null;
            string value = null;

            for (index = 1; index < count; index++)
            {
                key = strings[index].Trim();

                if (!String.IsNullOrEmpty(key))
                {
                    index++;
                    break;
                }
            }

            for (; index < count; index++)
            {
                value = strings[index].Trim();

                if (!String.IsNullOrEmpty(value))
                    break;
            }

            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value))
                return;

            switch (key)
            {
                    // #! version=1
                case "version":
                    Version = Convert.ToInt32(value);
                    break;
                    // #! subversion=0
                case "subversion":
                    SubVersion = Convert.ToInt32(value);
                    break;
                    // #! format=ts
                case "format":
                    Format = value;
                    break;
                    // #! charset=UTF-8
                case "charset":
                    CharSet = value;
                    break;
                    // #! entries=98958
                case "entries":
                    Entries = Convert.ToInt32(value);
                    break;
                    // #! publisher=MDBG
                case "publisher":
                    Publisher = value;
                    break;
                    // #! license=http://creativecommons.org/licenses/by-sa/3.0/
                case "license":
                    License = value;
                    break;
                    // #! date=2010-09-30T07:39:36Z
                case "date":
                    Date = value;
                    break;
                    // #! time=1285832376
                case "time":
                    Time = value;
                    break;
                default:
                    throw new ObjectException(Error = "FormatCEDict: Unknown directive: " + key);
            }
        }

        protected override void ReadEntry(string line)
        {
            string traditional;
            string simplified;
            string pinyinCanonical;
            List<string> targetMeanings = new List<string>(3);
            List<string> hostMeanings = new List<string>(3);
            List<Sense> hostSenses = new List<Sense>();
            string collectedCategoryString = String.Empty;
            LexicalCategory primaryCategory = LexicalCategory.Unknown;
            string[] chineseStrings = line.Split(new char[] { ' ' });
            DictionaryEntry dictionaryEntry;

            if ((EntryIndex % 1000) == 0)
                PutStatusMessage("Processing entry " + EntryIndex + ": " + line);

            if (chineseStrings.Count() >= 2)
            {
                traditional = chineseStrings[0].Trim();
                simplified = chineseStrings[1].Trim();
            }
            else
                throw new ObjectException(Error = "FormatCEDict: Fewer than expected Chinese fields: " + line);

            string[] pinyinStrings = line.Split(new char[] { '[', ']' });
            string[] englishStrings = line.Split(new char[] { '/' });

            if ((simplified == null) || (simplified == ""))
                simplified = traditional;

            if (pinyinStrings.Count() >= 2)
                pinyinCanonical = pinyinStrings[1].Trim();
            else
                pinyinCanonical = String.Empty;

            foreach (LanguageID languageID in TargetLanguageIDs)
            {
                if (languageID == LanguageLookup.ChineseTraditional)
                    targetMeanings.Add(traditional);
                else if (languageID == LanguageLookup.ChineseSimplified)
                    targetMeanings.Add(simplified);
                else if (languageID == LanguageLookup.ChinesePinyin)
                    targetMeanings.Add(pinyinCanonical);
            }

            int count = englishStrings.Count();

            for (int index = 1; index < count; index++)
            {
                string englishString = englishStrings[index].Trim();

                if ((englishString != null) && (englishString != ""))
                {
                    if (englishString.Contains(" (idiom)"))
                    {
                        englishString = englishString.Replace(" (idiom)", "").Trim();
                        primaryCategory = LexicalCategory.Idiom;
                    }
                    else if (englishString.Contains("(idiom)"))
                    {
                        englishString = englishString.Replace("(idiom)", "").Trim();
                        primaryCategory = LexicalCategory.Idiom;
                    }

                    // I'm not sure if the order of synonyms is significant, but in order to
                    // make le's "completed action marker"  synonym first, we reverse the order here.
                    if (!String.IsNullOrEmpty(englishString))
                        hostMeanings.Insert(0, englishString);
                }
            }

            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>();

            if (hostMeanings.Count() != 0)
            {
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>();
                foreach (string hostMeaning in hostMeanings)
                {
                    ProbableMeaning probableSynonym = new ProbableMeaning(
                        hostMeaning,
                        primaryCategory,
                        collectedCategoryString,
                        float.NaN,
                        0,
                        CEDictDictionarySourceIDList);
                    probableSynonyms.Add(probableSynonym);
                }
                LanguageSynonyms languageSynonyms = new LanguageSynonyms(
                    LanguageLookup.English,
                    probableSynonyms);
                languageSynonymsList.Add(languageSynonyms);
            }

            Sense hostSense = new Sense(
                0,
                primaryCategory,
                collectedCategoryString,
                0,
                languageSynonymsList,
                null);

            hostSenses.Add(hostSense);

            if (!String.IsNullOrEmpty(simplified))
                AddTargetEntry(
                    simplified,
                    LanguageLookup.ChineseSimplified,
                    targetMeanings,
                    CEDictDictionarySourceIDList,
                    hostSenses,
                    out dictionaryEntry);

            if (!String.IsNullOrEmpty(traditional))
                AddTargetEntry(traditional,
                    LanguageLookup.ChineseTraditional,
                    targetMeanings,
                    CEDictDictionarySourceIDList,
                    hostSenses,
                    out dictionaryEntry);

            if (!String.IsNullOrEmpty(pinyinCanonical))
                AddTargetEntry(
                    pinyinCanonical.ToLower(),
                    LanguageLookup.ChinesePinyin,
                    targetMeanings,
                    CEDictDictionarySourceIDList,
                    hostSenses,
                    out dictionaryEntry);

            if (IsAddReciprocals)
            {
                foreach (Sense englishSense in hostSenses)
                {
                    if (englishSense.LanguageSynonyms != null)
                    {
                        foreach (LanguageSynonyms languageSynonyms in englishSense.LanguageSynonyms)
                        {
                            if (languageSynonyms.LanguageID != LanguageLookup.English)
                                continue;

                            if (languageSynonyms.HasProbableSynonyms())
                            {
                                foreach (ProbableMeaning probableSynonym in languageSynonyms.ProbableSynonyms)
                                    AddHostEntry(
                                        probableSynonym.Meaning,
                                        LanguageLookup.English,
                                        CEDictDictionarySourceIDList,
                                        targetMeanings,
                                        primaryCategory,
                                        collectedCategoryString,
                                        englishSense.PriorityLevel);
                            }
                        }
                    }
                }
            }

            EntryIndex++;
        }

        // Returns true if any changes.
        protected override bool ConvertDictionaryEntryToDisplay(DictionaryEntry entry)
        {
            return ConvertPinyinNumeric.Display(entry, false, TemporaryDictionaryRepository,
                FormatQuickLookup.GetQuickDictionary(LanguageLookup.ChinesePinyin, LanguageLookup.ChineseSimplified));
        }

        public static string CEDictDictionarySourceName = "CEDict";

        protected static int _CEDictDictionarySourceID = 0;
        public static int CEDictDictionarySourceID
        {
            get
            {
                if (_CEDictDictionarySourceID == 0)
                    _CEDictDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(CEDictDictionarySourceName);

                return _CEDictDictionarySourceID;
            }
        }

        protected static List<int> _CEDictDictionarySourceIDList = null;
        public static List<int> CEDictDictionarySourceIDList
        {
            get
            {
                if (_CEDictDictionarySourceIDList == null)
                    _CEDictDictionarySourceIDList = new List<int>(1) { CEDictDictionarySourceID };

                return _CEDictDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "CEDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
