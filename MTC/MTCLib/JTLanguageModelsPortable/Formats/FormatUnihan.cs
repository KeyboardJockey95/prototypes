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
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatUnihan : FormatDictionary
    {
        protected Dictionary<string, MultiLanguageString> CharacterDictionary;
        protected List<MultiLanguageString> CharacterList;

        // Format data.
        private static string FormatDescription = "Format used for representing a Chinese dictionary.  See: http://cc-cedict.org/wiki";

        public FormatUnihan(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "UnihanDict",
                FormatDescription,
                "application/xml",
                ".xml",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
        }

        public FormatUnihan(FormatUnihan other)
            : base(other)
        {
        }

        public FormatUnihan()
            : base(
                "UnihanDict",
                "UnihanDict",
                FormatDescription,
                "application/xml",
                ".xml",
                null,
                null,
                null,
                null,
                null)
        {

        }

        public override Format Clone()
        {
            return new FormatUnihan(this);
        }

        public override void Read(Stream stream)
        {
            try
            {
                CharacterDictionary = new Dictionary<string, MultiLanguageString>();
                CharacterList = new List<MultiLanguageString>();

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
                CharacterDictionary = null;
                CharacterList = null;
            }
        }

        protected override void DispatchLine(string line)
        {
            line = line.Trim();

            if (line.StartsWith("<char"))
                ReadEntry(line);
        }

        protected override void ReadEntry(string line)
        {
            XElement element = XElement.Parse(line);
            XAttribute cpAttribute = element.Attribute("cp");
            XAttribute kSimplifiedVariantAttribute = element.Attribute("kSimplifiedVariant");
            XAttribute kTraditionalVariantAttribute = element.Attribute("kTraditionalVariant");
            XAttribute kHanyuPinluAttribute = element.Attribute("kHanyuPinlu");
            XAttribute kMandarinAttribute = element.Attribute("kMandarin");
            XAttribute kDefinitionAttribute = element.Attribute("kDefinition");
            XAttribute kCompatibilityVariantAttribute = element.Attribute("kCompatibilityVariant");
            string cp;
            string key;
            string kSimplifiedVariant = String.Empty;
            string kTraditionalVariant = String.Empty;
            string kHanyuPinlu = String.Empty;
            string kDefinition = String.Empty;
            List<string> simplifieds = new List<string>();
            List<string> traditionals = new List<string>();
            List<string> pinyins = new List<string>();
            List<Sense> compatibilitySenses = null;
            string compatibilityKey = String.Empty;
            DictionaryEntry targetEntry;
            int reading = 0;

            if (cpAttribute == null)
                return;

            cp = cpAttribute.Value;
            key = GetCharacterStringFromAttributeValue(cp);

            PutStatusMessage("Processing entry " + cp + ": " + key);

            if ((kSimplifiedVariantAttribute == null) && (kTraditionalVariantAttribute == null))
            {
                simplifieds.Add(key);
                traditionals.Add(key);
            }
            else if (kSimplifiedVariantAttribute != null)
            {
                kSimplifiedVariant = kSimplifiedVariantAttribute.Value;
                ParseCharacters(kSimplifiedVariant, simplifieds);
                traditionals.Add(key);
            }
            else if (kTraditionalVariantAttribute != null)
            {
                kTraditionalVariant = kTraditionalVariantAttribute.Value;
                simplifieds.Add(key);
                ParseCharacters(kTraditionalVariant, traditionals);
            }

            if (kHanyuPinluAttribute != null)
            {
                kHanyuPinlu = kHanyuPinluAttribute.Value;
                ParseHanyuPinlu(kHanyuPinlu, pinyins);
            }
            else if (kMandarinAttribute != null)
                pinyins.Add(kMandarinAttribute.Value.Trim());
            else if (kCompatibilityVariantAttribute != null)
            {
                compatibilityKey = GetCharacterStringFromAttributeValue(kCompatibilityVariantAttribute.Value);
                DictionaryEntry compatibilityEntrySimplified = GetDefinition(compatibilityKey, LanguageLookup.ChineseSimplified);
                DictionaryEntry compatibilityEntryTraditional = GetDefinition(compatibilityKey, LanguageLookup.ChineseTraditional);
                if (compatibilityEntrySimplified != null)
                {
                    simplifieds.Add(compatibilityKey);

                    if (compatibilityEntrySimplified.Alternates != null)
                    {
                        foreach (LanguageString alternate in compatibilityEntrySimplified.Alternates)
                        {
                            if (alternate.LanguageID == LanguageLookup.ChineseTraditional)
                            {
                                if (!traditionals.Contains(alternate.Text))
                                    traditionals.Add(alternate.Text);
                            }
                            else if (alternate.LanguageID == LanguageLookup.ChinesePinyin)
                            {
                                if (!pinyins.Contains(alternate.Text))
                                    pinyins.Add(alternate.Text);
                            }
                        }
                    }

                    compatibilitySenses = compatibilityEntrySimplified.Senses;
                }
                if (compatibilityEntryTraditional != null)
                {
                    traditionals.Add(compatibilityKey);

                    if (compatibilityEntryTraditional.Alternates != null)
                    {
                        foreach (LanguageString alternate in compatibilityEntryTraditional.Alternates)
                        {
                            if (alternate.LanguageID == LanguageLookup.ChineseSimplified)
                            {
                                if (!simplifieds.Contains(alternate.Text))
                                    simplifieds.Add(alternate.Text);
                            }
                            else if (alternate.LanguageID == LanguageLookup.ChinesePinyin)
                            {
                                if (!pinyins.Contains(alternate.Text))
                                    pinyins.Add(alternate.Text);
                            }
                        }
                    }

                    if (compatibilitySenses == null)
                        compatibilitySenses = compatibilityEntryTraditional.Senses;
                }
            }
            else
                pinyins.Add("x");

            if (kDefinitionAttribute != null)
                kDefinition = kDefinitionAttribute.Value;
            else if (compatibilitySenses == null)
                return;
            
            foreach (string simplified in simplifieds)
            {
                DictionaryEntry entrySimplified = GetDefinition(simplified, LanguageLookup.ChineseSimplified);

                foreach (string traditional in traditionals)
                {
                    DictionaryEntry entryTraditional = GetDefinition(traditional, LanguageLookup.ChineseTraditional);

                    foreach (string pinyin in pinyins)
                    {
                        string pinyinCanonical = pinyin;
                        List<string> targetMeanings = new List<string>(3);
                        List<string> hostMeanings = new List<string>(3);
                        List<Sense> hostSenses = new List<Sense>();
                        DictionaryEntry entryPinyin = GetDefinition(pinyinCanonical, LanguageLookup.ChinesePinyin);

                        if (!String.IsNullOrEmpty(kDefinition))
                            ParseSenses(kDefinition, reading, hostMeanings, hostSenses);
                        else if (compatibilitySenses != null)
                            hostSenses = compatibilitySenses;

                        targetMeanings.Add(simplified);
                        targetMeanings.Add(traditional);
                        targetMeanings.Add(pinyinCanonical);

                        // If we already have a dictionary entry, we ignore the Unihan one.

                        if (!String.IsNullOrEmpty(simplified) && (entrySimplified == null))
                            AddTargetEntry(
                                simplified,
                                LanguageLookup.ChineseSimplified,
                                targetMeanings,
                                UnihanDictionarySourceIDList,
                                hostSenses,
                                out targetEntry);

                        if (!String.IsNullOrEmpty(traditional) && (entryTraditional == null))
                            AddTargetEntry(traditional,
                                LanguageLookup.ChineseTraditional,
                                targetMeanings,
                                UnihanDictionarySourceIDList,
                                hostSenses,
                                out targetEntry);

                        if (!String.IsNullOrEmpty(pinyinCanonical) && (entryPinyin != null))
                            AddTargetEntry(
                                pinyinCanonical.ToLower(),
                                LanguageLookup.ChinesePinyin,
                                targetMeanings,
                                UnihanDictionarySourceIDList,
                                hostSenses,
                                out targetEntry);

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
                                                UnihanDictionarySourceIDList,
                                                targetMeanings,
                                                LexicalCategory.Unknown,
                                                String.Empty,
                                                englishSense.PriorityLevel);
                                    }
                                }
                            }
                        }

                        reading++;
                    }
                }
            }
        }

        protected string GetCharacterStringFromAttributeValue(string value)
        {
            value = value.Trim();

            if (value.StartsWith("U+"))
                value = value.Substring(2).Trim();

            string str = ApplicationData.Global.GetStringFromUTF32Number(ObjectUtilities.GetIntegerFromHexString(value, 0));

            return str;
        }

        protected void ParseCharacters(string value, List<string> list)
        {
            value = value.Trim();
            string[] parts = value.Split(LanguageLookup.Space, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                int ofs = part.IndexOf('(');
                string num;

                if (ofs != -1)
                    num = part.Substring(0, ofs).Trim();
                else
                    num = part.Trim();

                if (num.StartsWith("U+"))
                    num = num.Substring(2);

                string str = ((char)ObjectUtilities.GetIntegerFromHexString(num, 0)).ToString();

                list.Add(str);
            }
        }

        protected void ParseHanyuPinlu(string value, List<string> list)
        {
            value = value.Trim();
            string[] parts = value.Split(LanguageLookup.Space, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                int ofs = part.IndexOf('(');
                string pinyin;

                if (ofs != -1)
                    pinyin = part.Substring(0, ofs).Trim();
                else
                    pinyin = part.Trim();

                list.Add(pinyin);
            }
        }

        protected void ParseSenses(string value, int reading, List<string> meanings, List<Sense> senses)
        {
            value = value.Trim();
            string[] majors = value.Split(LanguageLookup.Semicolon, StringSplitOptions.RemoveEmptyEntries);

            foreach (string major in majors)
            {
                List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>();
                string[] minors = major.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string minor in minors)
                {
                    string meaning = minor.Trim();
                    ProbableMeaning probableMeaning = new ProbableMeaning(
                        meaning,
                        LexicalCategory.Unknown,
                        null,
                        float.NaN,
                        0,
                        UnihanDictionarySourceID);
                    probableSynonyms.Add(probableMeaning);
                    meanings.Add(meaning);
                }

                LanguageSynonyms languageSynonyms  = new LanguageSynonyms(
                    LanguageLookup.English,
                    probableSynonyms);
                Sense sense = new JTLanguageModelsPortable.Dictionary.Sense(
                    reading,
                    LexicalCategory.Unknown,
                    String.Empty,
                    0,
                    new List<LanguageSynonyms>() { languageSynonyms },
                    null);

                senses.Add(sense);
            }
        }

        protected MultiLanguageString GetCharacterEntry(string chr)
        {
            MultiLanguageString mls;

            if (CharacterDictionary.TryGetValue(chr, out mls))
                return mls;

            return null;
        }

        protected MultiLanguageString AddCharacterEntryCheck(string chr, LanguageID languageID, string text)
        {
            MultiLanguageString mls;

            if (!CharacterDictionary.TryGetValue(chr, out mls))
                mls = AddCharacterEntry(chr, languageID, text);
            else
            {
                LanguageString ls = mls.LanguageString(languageID);

                if (ls != null)
                    ls.Text = text;
                else
                {
                    ls = new Object.LanguageString(chr, languageID, text);
                    mls.Add(ls);
                }
            }

            return mls;
        }

        protected MultiLanguageString AddCharacterEntry(string chr, LanguageID languageID, string text)
        {
            MultiLanguageString mls = new MultiLanguageString(chr, languageID, text);
            CharacterDictionary.Add(chr, mls);
            CharacterList.Add(mls);
            return mls;
        }

        public static string UnihanDictionarySourceName = "Unihan";

        protected static int _UnihanDictionarySourceID = 0;
        public static int UnihanDictionarySourceID
        {
            get
            {
                if (_UnihanDictionarySourceID == 0)
                    _UnihanDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(UnihanDictionarySourceName);

                return _UnihanDictionarySourceID;
            }
        }

        protected static List<int> _UnihanDictionarySourceIDList = null;
        public static List<int> UnihanDictionarySourceIDList
        {
            get
            {
                if (_UnihanDictionarySourceIDList == null)
                    _UnihanDictionarySourceIDList = new List<int>(1) { UnihanDictionarySourceID };

                return _UnihanDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "Unihan"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
