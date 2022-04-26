using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Crawlers;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatWBXDict : FormatDictionary
    {
        protected LanguageTool Tool = null;

        // Format data.
        private static string FormatDescription = "Format for Dario de Judicibus's dictionaries, or .wbx-format dictionaries that use a (term)<PS>(def)<LS>... format.  See: https://dizionario.dejudicibus.it/";

        public FormatWBXDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "FormatWBXDict",
                FormatDescription,
                "application/octet-stream",
                ".wbx",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
        }

        public FormatWBXDict(FormatWBXDict other)
            : base(other)
        {
        }

        public FormatWBXDict()
            : base(
                  "WBXDict",
                  "FormatWBXDict",
                  FormatDescription,
                  "application/octet-stream",
                  ".wbx",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
        }

        public override Format Clone()
        {
            return new FormatWBXDict(this);
        }

        public override void Read(Stream stream)
        {
            try
            {
                PreRead(10);

                State = StateCode.Reading;

                UpdateProgressElapsed("Reading WBX dictionary ...");

                string dictionaryText = String.Empty;

                using (StreamReader streamReader = new StreamReader(stream))
                {
                    dictionaryText = streamReader.ReadToEnd();
                }

                UpdateProgressElapsed("Processing WBX dictionary ...");

                ProcessDic(dictionaryText);

                WriteDictionary();
                WriteDictionaryDisplayOutput();
                SynthesizeMissingAudio();
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

        protected const char PS = '\x2028';
        protected const char LS = '\x2029';

        protected void ProcessDic(string rawDict)
        {
            StringBuilder sb = new StringBuilder();
            string term = null;
            List<string> meanings = new List<string>();
            string meaning;

            Tool = ApplicationData.LanguageTools.Create(TargetLanguageID);

            foreach (char c in rawDict)
            {
                switch (c)
                {
                    case PS:
                        if (term != null)
                        {
                            meaning = sb.ToString();
                            meaning = FixupTerm(meaning);
                            meanings.Add(meaning);
                        }
                        else
                        {
                            term = sb.ToString();
                            term = FixupTerm(term);
                        }
                        sb.Clear();
                        break;
                    case LS:
                        meaning = sb.ToString();
                        meaning = FixupTerm(meaning);
                        sb.Clear();
                        meanings.Add(meaning);
                        if (term != null)
                            ProcessDefinition(term, meanings);
                        else
                            PutError("Empty term. meaning = " + meaning);
                        term = null;
                        meanings.Clear();
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            if ((term != null) && (meanings.Count() != 0))
                ProcessDefinition(term, meanings);
        }

        protected string FixupTerm(string term)
        {
            term = term.Replace(" (to ~)", "");
            return term;
        }

        protected void ProcessDefinition(
            string term,
            List<string> meanings)
        {
            DictionaryEntry dictionaryEntry;
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = null;

            if ((WBXBreakpoint != null) && (term == WBXBreakpoint))
                ApplicationData.Global.PutConsoleMessage("WBXBreakpoint 1: " + term);

            if (Tool != null)
                Tool.InferCategoryFromWord(term, out category, out categoryString);

            AddSimpleTargetEntry(
                term,
                TargetLanguageID,
                null,
                WBXDictionarySourceIDList,
                meanings,
                HostLanguageID,
                category,
                categoryString,
                null,
                0,
                null,
                out dictionaryEntry);
        }

        public static string WBXBreakpoint = null;

        protected override void AddStemCheck(DictionaryEntry dictionaryEntry, Sense sense)
        {
            if (Tool == null)
                return;

            string type;

            switch (sense.Category)
            {
                case LexicalCategory.Verb:
                    type = "Verb";
                    break;
                default:
                    return;
            }

            string targetWord = dictionaryEntry.KeyString;

            if ((WBXBreakpoint != null) && (targetWord == WBXBreakpoint))
                ApplicationData.Global.PutConsoleMessage("WBXBreakpoint 2: " + targetWord);

            string stemCategoryString;
            string targetStem = Tool.GetStem(targetWord, TargetLanguageID, out stemCategoryString);
            DictionaryEntry stemDictionaryEntry;

            if (targetStem == null)
                return;

            string categoryString = sense.CategoryString;

            if (!String.IsNullOrEmpty(categoryString) &&
                    !String.IsNullOrEmpty(stemCategoryString) &&
                    !categoryString.Contains(stemCategoryString))
                categoryString = categoryString + "," + stemCategoryString;
            else
                categoryString = stemCategoryString;

            sense.CategoryString = categoryString;

            List<string> irregularStems;
            string irregularCategoryStringTerm;

            if (Tool.GetIrregularStems(
                type,
                dictionaryEntry,
                targetStem,
                TargetLanguageID,
                out irregularStems,
                out irregularCategoryStringTerm))
            {
                if (!String.IsNullOrEmpty(irregularCategoryStringTerm))
                    categoryString += "," + irregularCategoryStringTerm;

                sense.CategoryString = categoryString;

                foreach (string irregularStem in irregularStems)
                {
                    DictionaryEntry irregularStemDictionaryEntry;
                    AddSimpleTargetStemEntry(
                        irregularStem,
                        targetWord,
                        WBXDictionarySourceIDList,
                        LexicalCategory.IrregularStem,
                        categoryString,
                        TargetLanguageID,
                        sense.GetSynonyms(HostLanguageID),
                        HostLanguageID,
                        out irregularStemDictionaryEntry);
                }
            }

            AddSimpleTargetStemEntry(
                targetStem,
                targetWord,
                WBXDictionarySourceIDList,
                LexicalCategory.Stem,
                categoryString,
                TargetLanguageID,
                sense.GetSynonyms(HostLanguageID),
                HostLanguageID,
                out stemDictionaryEntry);

            if (dictionaryEntry.Modified)
            {
                dictionaryEntry.TouchAndClearModified();
                UpdateDefinition(dictionaryEntry);
            }
        }

        public static string WBXDictionarySourceName = "WBX";

        protected static int _WBXDictionarySourceID = 0;
        public static int WBXDictionarySourceID
        {
            get
            {
                if (_WBXDictionarySourceID == 0)
                    _WBXDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(WBXDictionarySourceName);

                return _WBXDictionarySourceID;
            }
        }

        protected static List<int> _WBXDictionarySourceIDList = null;
        public static List<int> WBXDictionarySourceIDList
        {
            get
            {
                if (_WBXDictionarySourceIDList == null)
                    _WBXDictionarySourceIDList = new List<int>(1) { WBXDictionarySourceID };

                return _WBXDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "WBXDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
