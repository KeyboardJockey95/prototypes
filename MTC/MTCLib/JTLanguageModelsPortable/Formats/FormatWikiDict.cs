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
    public class FormatWikiDict : FormatDictionary
    {
        // These two fields are just for develoment to get the list of categories used.
        protected bool SaveCategories = false;
        protected List<string> UsedCategories = null;
        protected LanguageTool Tool = null;

        // Format data.
        private static string FormatDescription = "Format for dictionary text copied from Wikipedia, such as this: https://en.wiktionary.org/wiki/User:Matthias_Buchmeier/en-fr-a (to z)";

        public FormatWikiDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "WikiDict",
                FormatDescription,
                String.Empty,
                ".txt",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
        }

        public FormatWikiDict(FormatWikiDict other)
            : base(other)
        {
        }

        public FormatWikiDict()
            : base(
                  "WikiDict",
                  "WikiDict",
                  FormatDescription,
                  String.Empty,
                  ".txt",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
        }

        public override Format Clone()
        {
            return new FormatWikiDict(this);
        }

        public override void Read(Stream stream)
        {
            if (SaveCategories)
                UsedCategories = new List<string>();

            try
            {
                PreRead(10);

                State = StateCode.Reading;

                UpdateProgressElapsed("Processing dictionary text ...");

                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                        ProcessTextEntry(line);
                }

                if (!SaveCategories)
                {
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
                if (!SaveCategories)
                    PostRead();
                else
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string cat in UsedCategories)
                        sb.AppendLine(cat);

                    string filePath = Crawler.ComposeCrawlerDataFilePath("WikiDicFr", "Categories_" + TargetLanguageID.LanguageCode + ".txt");
                    FileSingleton.WriteAllText(filePath, sb.ToString());
                }
            }
        }

        public static string WikiBreakpoint = null;

        protected void ProcessTextEntry(string line)
        {
            if (String.IsNullOrEmpty(line))
                return;

            int separatorOffset = line.IndexOf("::");

            if (separatorOffset == -1)
                return;

            string targetTextEntry = line.Substring(separatorOffset + 2).Trim();

            if (String.IsNullOrEmpty(targetTextEntry))
                return;

            string hostTextEntry = line.Substring(0, separatorOffset).Trim();

            if (String.IsNullOrEmpty(hostTextEntry))
                return;

            List<string> targetInstances;
            string target;
            string translation;
            string meaning;
            LexicalCategory category;
            string categoryString;
            List<LexicalAttribute> attributes;
            string ipaReading;
            int priority = 0;
            List<string> hostSynonyms = new List<string>();
            List<MultiLanguageString> examples = null;
            DictionaryEntry dictionaryEntry;

            ParseHostDefinition(hostTextEntry, out translation, out meaning, out category, out categoryString, out ipaReading);

            if (!String.IsNullOrEmpty(translation))
                hostSynonyms.Add(translation);

            if (!String.IsNullOrEmpty(meaning))
                hostSynonyms.Add(meaning);

            if (hostSynonyms.Count() == 0)
                return;

            if ((WikiBreakpoint != null) && (translation == WikiBreakpoint))
                ApplicationData.Global.PutConsoleMessage("WikiBreakpoint: " + translation);

            ParseTargetInstances(targetTextEntry, out targetInstances);

            foreach (string targetInstance in targetInstances)
            {
                ParseTargetDefinition(targetInstance, out target, out attributes, ref categoryString);

                if (String.IsNullOrEmpty(target))
                    continue;

                if (!SaveCategories)
                    AddSimpleTargetEntry(
                        target,
                        TargetLanguageID,
                        ipaReading,
                        WikiDictionarySourceIDList,
                        hostSynonyms,
                        HostLanguageID,
                        category,
                        categoryString,
                        attributes,
                        priority,
                        examples,
                        out dictionaryEntry);
            }
        }

        protected void ParseTargetInstances(
            string rawDefinition,
            out List<string> targetInstances)
        {
            int index;
            int length = rawDefinition.Length;
            StringBuilder sbTargetInstances = new StringBuilder();
            int bracketLevel = 0;

            targetInstances = new List<string>();

            for (index = 0; index < length; index++)
            {
                char chr = rawDefinition[index];

                switch (chr)
                {
                    case '[':
                        bracketLevel++;
                        sbTargetInstances.Append(chr);
                        break;
                    case ']':
                        bracketLevel--;
                        sbTargetInstances.Append(chr);
                        break;
                    case ',':
                        if (bracketLevel == 0)
                        {
                            if (sbTargetInstances.Length != 0)
                            {
                                string targetInstance = sbTargetInstances.ToString().Trim();
                                targetInstances.Add(targetInstance);
                                sbTargetInstances.Clear();
                            }
                        }
                        else
                            sbTargetInstances.Append(chr);
                        break;
                    default:
                        sbTargetInstances.Append(chr);
                        break;
                }
            }

            if (sbTargetInstances.Length != 0)
            {
                string targetInstance = sbTargetInstances.ToString().Trim();
                targetInstances.Add(targetInstance);
            }
        }

        protected void ParseTargetDefinition(
            string rawDefinition,
            out string target,
            out List<LexicalAttribute> attributes,
            ref string categoryString)
        {
            int index;
            int length = rawDefinition.Length;
            StringBuilder sbTarget = new StringBuilder();
            StringBuilder sbAttribute = new StringBuilder();
            int braceLevel = 0;
            int bracketLevel = 0;

            attributes = null;

            for (index = 0; index < length; index++)
            {
                char chr = rawDefinition[index];

                switch (chr)
                {
                    case '{':
                        braceLevel++;
                        break;
                    case '}':
                        braceLevel--;
                        if (sbAttribute.Length != 0)
                        {
                            if (attributes == null)
                                attributes = new List<LexicalAttribute>();

                            string gender = sbAttribute.ToString().Trim();

                            sbAttribute.Clear();

                            switch (gender)
                            {
                                case "m":
                                    attributes.Add(LexicalAttribute.Masculine);
                                    break;
                                case "f":
                                    attributes.Add(LexicalAttribute.Feminine);
                                    break;
                                case "mf":
                                    attributes.Add(LexicalAttribute.Masculine);
                                    attributes.Add(LexicalAttribute.Feminine);
                                    break;
                                case "n":
                                    attributes.Add(LexicalAttribute.Neuter);
                                    break;
                                case "s":
                                    attributes.Add(LexicalAttribute.Singular);
                                    break;
                                case "p":
                                    attributes.Add(LexicalAttribute.Plural);
                                    break;
                                case "d":
                                    // Don't know what this means.
                                    break;
                                case "m-s":
                                    attributes.Add(LexicalAttribute.Masculine);
                                    attributes.Add(LexicalAttribute.Singular);
                                    break;
                                case "f-s":
                                    attributes.Add(LexicalAttribute.Feminine);
                                    attributes.Add(LexicalAttribute.Singular);
                                    break;
                                case "mf-s":
                                    attributes.Add(LexicalAttribute.Masculine);
                                    attributes.Add(LexicalAttribute.Feminine);
                                    attributes.Add(LexicalAttribute.Singular);
                                    break;
                                case "n-s":
                                    attributes.Add(LexicalAttribute.Neuter);
                                    attributes.Add(LexicalAttribute.Singular);
                                    break;
                                case "m-p":
                                    attributes.Add(LexicalAttribute.Masculine);
                                    attributes.Add(LexicalAttribute.Plural);
                                    break;
                                case "f-p":
                                    attributes.Add(LexicalAttribute.Feminine);
                                    attributes.Add(LexicalAttribute.Plural);
                                    break;
                                case "mf-p":
                                    attributes.Add(LexicalAttribute.Masculine);
                                    attributes.Add(LexicalAttribute.Feminine);
                                    attributes.Add(LexicalAttribute.Plural);
                                    break;
                                case "n-p":
                                    attributes.Add(LexicalAttribute.Neuter);
                                    attributes.Add(LexicalAttribute.Plural);
                                    break;
                                case "?":
                                    break;
                                default:
                                    ApplicationData.Global.PutConsoleMessage("ParseTargetDefinition: Unexpected gender attribute: " + gender);
                                    break;
                            }
                        }
                        break;
                    case '[':
                        bracketLevel++;
                        break;
                    case ']':
                        bracketLevel--;
                        if (sbAttribute.Length != 0)
                        {
                            if (attributes == null)
                                attributes = new List<LexicalAttribute>();

                            string attribute = sbAttribute.ToString().Trim();

                            if (!String.IsNullOrEmpty(categoryString))
                                categoryString += ", ";

                            categoryString += attribute;

                            sbAttribute.Clear();
                        }
                        break;
                    default:
                        if (braceLevel > 0)
                            sbAttribute.Append(chr);
                        else if (bracketLevel > 0)
                            sbAttribute.Append(chr);
                        else
                            sbTarget.Append(chr);
                        break;
                }
            }

            if (sbTarget.Length != 0)
                target = sbTarget.ToString().Trim();
            else
                target = null;
        }

        protected void ParseHostDefinition(
            string rawDefinition,
            out string translation,
            out string meaning,
            out LexicalCategory category,
            out string categoryString,
            out string ipaReading)
        {
            int seeOffset = rawDefinition.IndexOf("SEE:");

            if (seeOffset != -1)
                rawDefinition = rawDefinition.Substring(0, seeOffset).Trim();

            int index;
            int length = rawDefinition.Length;
            StringBuilder sbTranslation = new StringBuilder();
            StringBuilder sbMeaning = new StringBuilder();
            StringBuilder sbCategory = new StringBuilder();
            StringBuilder sbReading = new StringBuilder();
            int braceLevel = 0;
            bool inQuotes = false;
            bool inSlash = false;
            int parenLevel = 0;

            for (index = 0; index < length; index++)
            {
                char chr = rawDefinition[index];
                bool saveChar = true;

                switch (chr)
                {
                    case '{':
                        if (!inQuotes)
                        {
                            braceLevel++;
                            saveChar = false;
                        }
                        break;
                    case '}':
                        if (!inQuotes)
                        {
                            braceLevel--;
                            saveChar = false;
                        }
                        break;
                    case '/':
                        if (!inQuotes)
                        {
                            inSlash = !inSlash;
                            saveChar = false;
                        }
                        break;
                    case '"':
                        inQuotes = !inQuotes;
                        break;
                    case '“':
                        inQuotes = true;
                        break;
                    case '”':
                        inQuotes = false;
                        break;
                    case '(':
                        if (!inQuotes)
                        {
                            parenLevel++;
                            if (parenLevel > 1)
                                sbMeaning.Append(chr);
                            saveChar = false;
                        }
                        break;
                    case ')':
                        if (!inQuotes)
                        {
                            parenLevel--;
                            if (parenLevel > 0)
                                sbMeaning.Append(chr);
                            for (int i = index + 1; i < length; i++, index++)
                            {
                                if (!char.IsWhiteSpace(rawDefinition[i]))
                                    break;
                            }
                            saveChar = false;
                        }
                        break;
                    default:
                        break;
                }

                if (saveChar)
                {
                    if (braceLevel > 0)
                        sbCategory.Append(chr);
                    else if (inSlash)
                        sbReading.Append(chr);
                    else if (parenLevel > 0)
                        sbMeaning.Append(chr);
                    else
                        sbTranslation.Append(chr);
                }
            }

            if (sbTranslation.Length != 0)
                translation = sbTranslation.ToString().Trim();
            else
                translation = null;

            if (sbMeaning.Length != 0)
                meaning = sbMeaning.ToString().Trim();
            else
                meaning = null;

            if (sbCategory.Length != 0)
            {
                category = GetCategoryFromRawCategory(sbCategory.ToString().Trim(), translation);
                categoryString = null;
            }
            else
            {
                category = LexicalCategory.Unknown;
                categoryString = null;
            }

            if (sbReading.Length != 0)
                ipaReading = sbReading.ToString().Trim();
            else
                ipaReading = null;
        }

        protected LexicalCategory GetCategoryFromRawCategory(string rawCategory, string translation)
        {
            LexicalCategory category;

            switch (rawCategory)
            {
                case "n":
                    category = LexicalCategory.Noun;
                    break;
                case "prop":
                    category = LexicalCategory.ProperNoun;
                    break;
                case "pron":
                    category = LexicalCategory.Pronoun;
                    break;
                case "determiner":
                    category = LexicalCategory.Determiner;
                    break;
                case "adj":
                    category = LexicalCategory.Adjective;
                    break;
                case "v":
                    category = LexicalCategory.Verb;
                    break;
                case "adv":
                    category = LexicalCategory.Adverb;
                    break;
                case "prep":
                    category = LexicalCategory.Preposition;
                    break;
                case "conj":
                    category = LexicalCategory.Conjunction;
                    break;
                case "interj":
                    category = LexicalCategory.Interjection;
                    break;
                case "particle":
                    category = LexicalCategory.Particle;
                    break;
                case "art":
                case "article":
                    category = LexicalCategory.Article;
                    break;
                case "meas":        // Not used.
                    category = LexicalCategory.MeasureWord;
                    break;
                case "num":
                case "cardinal num":
                    category = LexicalCategory.Number;
                    break;
                case "abbr":
                    category = LexicalCategory.Abbreviation;
                    break;
                case "acronym":
                    category = LexicalCategory.Acronym;
                    break;
                case "symbol":
                    category = LexicalCategory.Symbol;
                    break;
                case "initialism":
                    if (TextUtilities.IsUpperString(translation))
                        category = LexicalCategory.Acronym;
                    else
                        category = LexicalCategory.Phrase;
                    break;
                case "phrase":
                    category = LexicalCategory.Phrase;
                    break;
                case "prep phrase":
                    category = LexicalCategory.PrepositionalPhrase;
                    break;
                case "proverb":
                    category = LexicalCategory.Proverb;
                    break;
                case "contraction":
                    category = LexicalCategory.Contraction;
                    break;
                case "idiom":       // Not used.
                    category = LexicalCategory.Idiom;
                    break;
                case "stem":        // Not used.
                    category = LexicalCategory.Stem;
                    break;
                case "inflection":  // Not used.
                    category = LexicalCategory.Inflection;
                    break;
                case "prefix":
                    category = LexicalCategory.Prefix;
                    break;
                case "suffix":
                    category = LexicalCategory.Suffix;
                    break;
                case "not found":   // Not used.
                    category = LexicalCategory.NotFound;
                    break;
                default:
                    PutErrorArgument("Unknown raw category", rawCategory);
                    category = LexicalCategory.Unknown;
                    break;
            }

            return category;
        }

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

            if ((WikiBreakpoint != null) && (targetWord == WikiBreakpoint))
                ApplicationData.Global.PutConsoleMessage("WikiBreakpoint: " + targetWord);

            string stemCategoryString;
            string targetStem = Tool.GetStem(targetWord, TargetLanguageID, out stemCategoryString);
            DictionaryEntry stemDictionaryEntry;

            if (targetStem == null)
                return;

            string categoryString = sense.CategoryString;

            if (!String.IsNullOrEmpty(categoryString) && !String.IsNullOrEmpty(stemCategoryString))
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
                        WikiDictionarySourceIDList,
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
                WikiDictionarySourceIDList,
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

        public static string WikiDictionarySourceName = "Wiki";

        protected static int _WikiDictionarySourceID = 0;
        public static int WikiDictionarySourceID
        {
            get
            {
                if (_WikiDictionarySourceID == 0)
                    _WikiDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(WikiDictionarySourceName);

                return _WikiDictionarySourceID;
            }
        }

        protected static List<int> _WikiDictionarySourceIDList = null;
        public static List<int> WikiDictionarySourceIDList
        {
            get
            {
                if (_WikiDictionarySourceIDList == null)
                    _WikiDictionarySourceIDList = new List<int>(1) { WikiDictionarySourceID };

                return _WikiDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "WikiDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
