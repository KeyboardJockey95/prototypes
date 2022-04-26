using System;
using System.Collections.Generic;
using System.IO;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatJIAlignDict : FormatDictionary
    {
        // Format data.
        private static string FormatDescription = "The jialign word alignment tool phase output.  See: http://www.phontron.com/pialign";

        public FormatJIAlignDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "FormatJIAlignDict",
                FormatDescription,
                "text/plain",
                ".pt",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
        }

        public FormatJIAlignDict(FormatJIAlignDict other)
            : base(other)
        {
        }

        public FormatJIAlignDict()
            : base(
                "JIAlign",
                "FormatJIAlignDict",
                FormatDescription,
                "text/plain",
                ".pt",
                null,
                null,
                null,
                null,
                null)
        {

        }

        public override Format Clone()
        {
            return new FormatJIAlignDict(this);
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

                    WriteDictionary();
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

        private static char[] BarSeparator = { '|' };
        private static char[] NumberSeparator = { ' ' };

        protected override void ReadEntry(string line)
        {
            string[] parts = line.Split(BarSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
                throw new Exception("Unexpected line format: " + line);

            string targetPhrase = parts[0].Trim();
            string hostPhrase = parts[1].Trim();
            string numbersListString = parts[2].Trim();
            string[] numberParts = numbersListString.Split(NumberSeparator);
            List<double> numbers = new List<double>();

            foreach (string numberString in numberParts)
                numbers.Add(ObjectUtilities.GetDoubleFromString(numberString, 0.0));

            if (FilterEntry(targetPhrase, numbers) || FilterEntry(hostPhrase, numbers))
                return;

            // Add target entry.
            AddPhrase(
                targetPhrase,
                hostPhrase,
                TargetLanguageID,
                HostLanguageID);

            if (IsAddReciprocals)
            {
                // Add host reciprocal entry.
                AddPhrase(
                    hostPhrase,
                    targetPhrase,
                    HostLanguageID,
                    TargetLanguageID);
            }
        }

        protected bool FilterEntry(string phrase, List<double> numbers)
        {
            // If conditional probabilities are less than 0.02, ignore.
            if ((numbers[0] < 0.02) || (numbers[0] < 0.02))
                return true;

            if (TextUtilities.ContainsOneOrMoreCharacters(phrase, LanguageLookup.PunctuationCharacters))
                return true;

            return false;
        }

        protected void AddPhrase(
            string targetPhrase,
            string hostPhrase,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID)
        {
            List<string> targetMeanings = new List<string>() { targetPhrase };
            ProbableMeaning probableSynonym = new ProbableMeaning(
                targetPhrase,
                LexicalCategory.Unknown,
                null,
                float.NaN,
                0,
                JIAlignDictionarySourceIDList);

            List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>(1) { probableSynonym };
            LanguageSynonyms languageSynonyms = new LanguageSynonyms(
                hostLanguageID,
                probableSynonyms);
            List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>(1) { languageSynonyms };
            Sense sense = new Sense(
                0,
                LexicalCategory.Unknown,
                null,
                0,
                languageSynonymsList,
                null);
            List<Sense> hostSenses = new List<Sense>() { sense };
            DictionaryEntry dictionaryEntry;

            AddTargetEntry(
                targetPhrase,
                TargetLanguageID,
                targetMeanings,
                JIAlignDictionarySourceIDList,
                hostSenses,
                out dictionaryEntry);
        }

        public static string JIAlignDictionarySourceName = "JIAlign";

        protected static int _JIAlignDictionarySourceID = 0;
        public static int JIAlignDictionarySourceID
        {
            get
            {
                if (_JIAlignDictionarySourceID == 0)
                    _JIAlignDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(JIAlignDictionarySourceName);

                return _JIAlignDictionarySourceID;
            }
        }

        protected static List<int> _JIAlignDictionarySourceIDList = null;
        public static List<int> JIAlignDictionarySourceIDList
        {
            get
            {
                if (_JIAlignDictionarySourceIDList == null)
                    _JIAlignDictionarySourceIDList = new List<int>(1) { JIAlignDictionarySourceID };

                return _JIAlignDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "JIAlign"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
