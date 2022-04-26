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
    public class FormatTHTDict : FormatDictionary
    {
        // These two fields are just for develoment to get the list of categories used.
        protected bool SaveCategories = false;
        protected List<string> UsedCategories = null;
        protected LanguageTool Tool = null;

        // Format data.
        private static string FormatDescription = "Format for simple XML dictionary, such as the en-es-en-dic Spanish dictionary.  See: https://github.com/mananoreboton/en-es-en-Dic";

        public FormatTHTDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "FormatTHTDict",
                FormatDescription,
                String.Empty,
                ".xml",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
        }

        public FormatTHTDict(FormatTHTDict other)
            : base(other)
        {
        }

        public FormatTHTDict()
            : base(
                  "THTDict",
                  "FormatTHTDict",
                  FormatDescription,
                  String.Empty,
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
            return new FormatTHTDict(this);
        }

        public override void Read(Stream stream)
        {
            if (SaveCategories)
                UsedCategories = new List<string>();

            try
            {
                PreRead(10);

                State = StateCode.Reading;

                UpdateProgressElapsed("Reading XML dictionary ...");

                XDocument dictDocument = XDocument.Load(stream);
                XElement dicElement = dictDocument.Element("dic");

                if (dicElement == null)
                {
                    PutError("No \"dic\" element in external dictionary.");
                    return;
                }

                UpdateProgressElapsed("Processing XML dictionary ...");

                ProcessDic(dicElement);

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

                    string filePath = Crawler.ComposeCrawlerDataFilePath("THTDict", "Categories_" + TargetLanguageID.LanguageCode + ".txt");
                    FileSingleton.WriteAllText(filePath, sb.ToString());
                }
            }
        }

        protected void ProcessDic(XElement element)
        {
            XAttribute fromAttribute = element.Attribute("from");
            XAttribute toAttribute = element.Attribute("to");

            if (fromAttribute != null)
            {
                string fromLanguageCode = fromAttribute.Value.Trim();
                LanguageID fromLanguageID = LanguageLookup.GetLanguageIDNoAdd(fromLanguageCode);

                if (fromLanguageID == TargetLanguageID)
                    Tool = ApplicationData.LanguageTools.Create(fromLanguageID);
                else
                    Tool = null;

                TargetLanguageIDs = new List<LanguageID>(1) { fromLanguageID };
            }
            else
            {
                PutError("No \"from\" attribute in \"dic\" element.");
                return;
            }

            if (toAttribute != null)
            {
                string toLanguageCode = toAttribute.Value.Trim();
                LanguageID toLanguageID = LanguageLookup.GetLanguageIDNoAdd(toLanguageCode);
                HostLanguageIDs = new List<LanguageID>(1) { toLanguageID };
            }
            else
            {
                PutError("No \"to\" attribute in \"dic\" element.");
                return;
            }

            foreach (XElement childElement in element.Elements())
            {
                switch (childElement.Name.LocalName)
                {
                    case "l":
                        ProcessL(childElement);
                        break;
                    default:
                        PutError("Unexpected child element in \"dic\" element: ", childElement.Name.LocalName);
                        break;
                }
            }
        }

        protected void ProcessL(XElement element)
        {
            foreach (XElement childElement in element.Elements())
            {
                switch (childElement.Name.LocalName)
                {
                    case "w":
                        ProcessW(childElement);
                        break;
                    default:
                        PutError("Unexpected child element in \"l\" element: ", childElement.Name.LocalName);
                        break;
                }
            }
        }

        protected void ProcessW(XElement element)
        {
            XElement cElement = element.Element("c");
            XElement dElement = element.Element("d");
            XElement tElement = element.Element("t");
            string cValue = String.Empty;
            string dValue = String.Empty;
            string tValue = String.Empty;
            string ipaReading = String.Empty;
            List<string> hostSynonyms = new List<string>();
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = String.Empty;
            List<LexicalAttribute> attributes = null;
            string topic = String.Empty;
            int priority = 0;
            List<MultiLanguageString> examples = null;
            DictionaryEntry dictionaryEntry;

            if (cElement != null)
                cValue = cElement.Value.Trim();

            if (dElement != null)
                dValue = dElement.Value.Trim();

            if (tElement != null)
                tValue = tElement.Value.Trim();

            if (String.IsNullOrEmpty(cValue))
                return;

            if (String.IsNullOrEmpty(dValue))
                return;

            if (String.IsNullOrEmpty(tValue))
                return;

            //if (cValue == "oír")
            //{
            //    DictionaryEntry oidEntry = GetDefinition("oid", TargetLanguageID);
            //    DumpString("oír");
            //}

            ParseDefinitions(
                dValue,
                hostSynonyms);

            ParseTypes(
                cValue,
                dValue,
                tValue,
                ref ipaReading,
                ref category,
                ref categoryString,
                ref attributes,
                ref topic,
                ref priority);

            if (!SaveCategories)
                AddSimpleTargetEntry(
                    cValue,
                    TargetLanguageID,
                    ipaReading,
                    THTDictionarySourceIDList,
                    hostSynonyms,
                    HostLanguageID,
                    category,
                    categoryString,
                    attributes,
                    priority,
                    examples,
                    out dictionaryEntry);
        }

        protected static string[] RemoveThese =
        {
            "\"",
            "“",
            "”",
            "{",
            "}",
            "l/en, "
        };

        protected void ParseDefinitions(
            string dValue,
            List<string> hostSynonyms)
        {
            dValue = TextUtilities.RemoveDelimitedStrings(dValue, "{", "}");
            dValue = TextUtilities.RemoveStrings(dValue, RemoveThese);

            if (!dValue.Contains(",") && !dValue.Contains(";") && !dValue.Contains(".") && !dValue.Contains("/"))
                hostSynonyms.Add(dValue);
            else
            {
                int index;
                int length = dValue.Length;
                StringBuilder sb = new StringBuilder();
                int parenLevel = 0;
                string hostSynonym;

                for (index = 0; index < length; index++)
                {
                    char chr = dValue[index];

                    switch (chr)
                    {
                        case '(':
                            parenLevel++;
                            sb.Append(chr);
                            break;
                        case ')':
                            parenLevel--;
                            sb.Append(chr);
                            for (int i = index + 1; i < length; i++, index++)
                            {
                                if (!char.IsWhiteSpace(dValue[i]))
                                    break;
                            }
                            break;
                        case ';':
                        case ',':
                        case '/':
                            if (parenLevel <= 0)
                            {
                                hostSynonym = sb.ToString().Trim();
                                if (hostSynonym.Length != 0)
                                    hostSynonyms.Add(hostSynonym);
                                sb.Clear();
                                for (int i = index + 1; i < length; i++, index++)
                                {
                                    if (!char.IsWhiteSpace(dValue[i]))
                                        break;
                                }
                            }
                            break;
                        case '.':
                            if (parenLevel <= 0)
                            {
                                sb.Append(chr);
                                hostSynonyms.Add(sb.ToString().Trim());
                                sb.Clear();
                                for (int i = index + 1; i < length; i++, index++)
                                {
                                    if (!char.IsWhiteSpace(dValue[i]))
                                        break;
                                }
                            }
                            break;
                        default:
                            sb.Append(chr);
                            break;
                    }
                }

                if (sb.Length != 0)
                {
                    hostSynonym = sb.ToString().Trim();

                    if (hostSynonym.Length != 0)
                        hostSynonyms.Add(hostSynonym);
                }
            }
        }

        protected void ParseTypes(
            string cValue,
            string dValue,
            string tValue,
            ref string ipaReading,
            ref LexicalCategory category,
            ref string categoryString,
            ref List<LexicalAttribute> attributes,
            ref string topic,
            ref int priority)
        {
            string rawCategory = TextUtilities.ParseDelimitedField("{", "}", tValue, 0).Trim();
            string rawAttribute = TextUtilities.ParseDelimitedField("{", "}", dValue, 0).Trim();

            if (SaveCategories)
            {
                if (!UsedCategories.Contains(rawCategory))
                    UsedCategories.Add(rawCategory);
            }

            category = GetCategoryFromRawCategory(rawCategory, cValue);
            ipaReading = TextUtilities.ParseDelimitedField("/", "/", tValue, 0).Trim();
            topic = TextUtilities.ParseDelimitedField("(", ")", tValue, 0).Trim();

            if (!String.IsNullOrEmpty(rawCategory))
                categoryString = GetCategoryStringFromRawCategory(rawCategory, cValue);

            if (!String.IsNullOrEmpty(topic) && (TargetLanguageID != LanguageLookup.English))
            {
                if (!String.IsNullOrEmpty(categoryString))
                    categoryString += ", ";

                categoryString += topic;
            }

            if (String.IsNullOrEmpty(rawAttribute))
            {
                if (attributes == null)
                    attributes = new List<LexicalAttribute>();

                switch (rawAttribute)
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
                    case "":
                        break;
                    default:
                        throw new Exception("ParseTypes: Unexpected gender attribute: " + rawAttribute);
                }

            }
        }

        protected LexicalCategory GetCategoryFromRawCategory(string rawCategory, string cValue)
        {
            LexicalCategory category;

            switch (TargetLanguageID.LanguageCode)
            {
                case "en":
                default:
                    category = GetCategoryFromRawCategoryEn(rawCategory, cValue);
                    break;
                case "es":
                    category = GetCategoryFromRawCategoryEs(rawCategory, cValue);
                    break;
            }

            return category;
        }

        protected string GetCategoryStringFromRawCategory(string rawCategory, string cValue)
        {
            string categoryString;

            switch (TargetLanguageID.LanguageCode)
            {
                case "en":
                default:
                    categoryString = GetCategoryStringFromRawCategoryEn(rawCategory, cValue);
                    break;
                case "es":
                    categoryString = GetCategoryStringFromRawCategoryEs(rawCategory, cValue);
                    break;
            }

            return categoryString;
        }

        protected LexicalCategory GetCategoryFromRawCategoryEn(string rawCategory, string cValue)
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
                    if (TextUtilities.IsUpperString(cValue))
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

        protected string GetCategoryStringFromRawCategoryEn(string rawCategory, string cValue)
        {
            string categoryString;

            switch (rawCategory)
            {
                case "n":
                    categoryString = "n";
                    break;
                case "prop":
                    categoryString = "prop";
                    break;
                case "pron":
                    categoryString = "pron";
                    break;
                case "determiner":
                    categoryString = "determiner";
                    break;
                case "adj":
                    categoryString = "adj";
                    break;
                case "v":
                    categoryString = "v";
                    break;
                case "adv":
                    categoryString = "adv";
                    break;
                case "prep":
                    categoryString = "prep";
                    break;
                case "conj":
                    categoryString = "conj";
                    break;
                case "interj":
                    categoryString = "interj";
                    break;
                case "particle":
                    categoryString = "particle";
                    break;
                case "art":
                    categoryString = "art";
                    break;
                case "meas":        // Not used.
                    categoryString = "meas";
                    break;
                case "num":
                    categoryString = "num";
                    break;
                case "cardinal num":
                    categoryString = "cardinal num";
                    break;
                case "abbr":
                    categoryString = "abbr";
                    break;
                case "acronym":
                    categoryString = "acronym";
                    break;
                case "symbol":
                    categoryString = "symbol";
                    break;
                case "initialism":
                    categoryString = "initialism";
                    break;
                case "phrase":
                    categoryString = "phrase";
                    break;
                case "prep phrase":
                    categoryString = "prep phrase";
                    break;
                case "proverb":
                    categoryString = "proverb";
                    break;
                case "contraction":
                    categoryString = "contraction";
                    break;
                case "idiom":
                    categoryString = "idiom";
                    break;
                case "stem":        // Not used.
                    categoryString = "stem";
                    break;
                case "inflection":  // Not used.
                    categoryString = "inflection";
                    break;
                case "prefix":
                    categoryString = "prefix";
                    break;
                case "suffix":
                    categoryString = "suffix";
                    break;
                case "not found":   // Not used.
                    categoryString = "";
                    break;
                default:
                    PutErrorArgument("Unknown raw category", rawCategory);
                    categoryString = "";
                    break;
            }

            return categoryString;
        }

        protected LexicalCategory GetCategoryFromRawCategoryEs(string rawCategory, string cValue)
        {
            LexicalCategory category;

            switch (rawCategory)
            {
                case "f":
                case "fp":
                case "m":
                case "mf":
                case "mfp":
                case "mp":
                case "n":
                case "p":
                case "vm":
                case "letter":
                    category = LexicalCategory.Noun;
                    break;
                case "prop":
                case "propf":
                case "propm":
                    category = LexicalCategory.ProperNoun;
                    break;
                case "pron":
                    category = LexicalCategory.Pronoun;
                    break;
                case "determiner":
                    category = LexicalCategory.Determiner;
                    break;
                case "adj":
                case "adjf":
                case "adjm":
                case "adjmf":
                    category = LexicalCategory.Adjective;
                    break;
                case "v":
                case "vi":
                case "vir":
                case "vit":
                case "vitr":
                case "vp":
                case "vr":
                case "vrr":
                case "vt":
                case "vtr":
                    category = LexicalCategory.Verb;
                    break;
                case "adv":
                case "advm":
                case "advf":
                case "advmf":
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
                    category = LexicalCategory.Article;
                    break;
                case "meas":        // Not used.
                    category = LexicalCategory.MeasureWord;
                    break;
                case "num":
                case "numm":
                case "numf":
                case "cardinal num":
                    category = LexicalCategory.Number;
                    break;
                case "abbr":
                case "abbrm":
                case "abbrf":
                    category = LexicalCategory.Abbreviation;
                    break;
                case "acronym":
                    category = LexicalCategory.Acronym;
                    break;
                case "symbol":
                case "affix":
                    category = LexicalCategory.Symbol;
                    break;
                case "initialism":
                case "initialismm":
                case "initialismf":
                    if (TextUtilities.IsUpperString(cValue))
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

        protected string GetCategoryStringFromRawCategoryEs(string rawCategory, string cValue)
        {
            string categoryString;

            switch (rawCategory)
            {
                case "f":
                    categoryString = "n,f,s" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "fp":
                    categoryString = "n,f,p" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "m":
                    categoryString = "n,m,s" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "mf":
                    categoryString = "n,mf,s" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "mfp":
                    categoryString = "n,mf,p" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "mp":
                    categoryString = "n,m,p" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "n":
                    categoryString = "n,s" + GetNounCategoryStringSuffixES(cValue);
                    break;
                case "p":
                    categoryString = "n,p";
                    break;
                case "vm":
                    categoryString = "";
                    break;
                case "letter":
                    categoryString = "n,s";
                    break;
                case "prop":
                    categoryString = "n";
                    break;
                case "propf":
                    categoryString = "n,f";
                    break;
                case "propm":
                    categoryString = "n,m";
                    break;
                case "pron":
                    categoryString = "pron";
                    break;
                case "determiner":
                    categoryString = "determiner";
                    break;
                case "adj":
                    categoryString = "adj" + GetAdjectiveCategoryStringSuffixES(cValue);
                    break;
                case "adjf":
                    categoryString = "adj,f" + GetAdjectiveCategoryStringSuffixES(cValue);
                    break;
                case "adjm":
                    categoryString = "adj,m" + GetAdjectiveCategoryStringSuffixES(cValue);
                    break;
                case "adjmf":
                    categoryString = "adj,mf" + GetAdjectiveCategoryStringSuffixES(cValue);
                    break;
                case "v":
                    categoryString = "v" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vi":
                    categoryString = "v,i" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vir":
                    categoryString = "v,i,r" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vit":
                    categoryString = "v,i,t" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vitr":
                    categoryString = "v,i,t,r" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vp":
                    categoryString = "v,p" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vr":
                    categoryString = "v,r" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vrr":
                    categoryString = "v,rr" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vt":
                    categoryString = "v,t" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "vtr":
                    categoryString = "v,t,r" + GetVerbCategoryStringSuffixES(cValue);
                    break;
                case "adv":
                    categoryString = "adv";
                    break;
                case "advm":
                    categoryString = "adv,m";
                    break;
                case "advf":
                    categoryString = "adv,f";
                    break;
                case "advmf":
                    categoryString = "adv,mf";
                    break;
                case "prep":
                    categoryString = "prep";
                    break;
                case "conj":
                    categoryString = "conj";
                    break;
                case "interj":
                    categoryString = "interj";
                    break;
                case "particle":
                    categoryString = "particle";
                    break;
                case "art":
                    categoryString = "art";
                    break;
                case "meas":        // Not used.
                    categoryString = "meas";
                    break;
                case "num":
                    categoryString = "num";
                    break;
                case "numm":
                    categoryString = "num,m";
                    break;
                case "numf":
                    categoryString = "num,f";
                    break;
                case "cardinal num":
                    categoryString = "cardinal ";
                    break;
                case "abbr":
                    categoryString = "abbr";
                    break;
                case "abbrm":
                    categoryString = "abbr,m";
                    break;
                case "abbrf":
                    categoryString = "abbr,f";
                    break;
                case "acronym":
                    categoryString = "acronym";
                    break;
                case "symbol":
                    categoryString = "symbol";
                    break;
                case "affix":
                    categoryString = "affix";
                    break;
                case "initialism":
                    categoryString = "initialism";
                    break;
                case "initialismm":
                    categoryString = "initialism,m";
                    break;
                case "initialismf":
                    categoryString = "initialism,f";
                    break;
                case "phrase":
                    categoryString = "phrase";
                    break;
                case "prep phrase":
                    categoryString = "prep ";
                    break;
                case "proverb":
                    categoryString = "proverb";
                    break;
                case "contraction":
                    categoryString = "contraction";
                    break;
                case "idiom":       // Not used.
                    categoryString = "idiom";
                    break;
                case "stem":        // Not used.
                    categoryString = "stem";
                    break;
                case "inflection":  // Not used.
                    categoryString = "inflection";
                    break;
                case "prefix":
                    categoryString = "prefix";
                    break;
                case "suffix":
                    categoryString = "suffix";
                    break;
                case "not found":   // Not used.
                    categoryString = "not ";
                    break;
                default:
                    PutErrorArgument("Unknown raw category", rawCategory);
                    categoryString = "";
                    break;
            }

            return categoryString;
        }

        protected string GetVerbCategoryStringSuffixES(string cValue)
        {
            if (cValue.Length < 2)
                return "";

            string suffix = cValue.Substring(cValue.Length - 2, 2);

            if ((suffix == "se") && (cValue.Length >= 4))
                suffix = cValue.Substring(cValue.Length - 4, 2);

            suffix = "," + suffix;

            return suffix;
        }

        protected string GetNounCategoryStringSuffixES(string cValue)
        {
            if (String.IsNullOrEmpty(cValue))
                return "";

            char lastChr = cValue[cValue.Length - 1];
            string suffix;

            switch (lastChr)
            {
                case 'a':
                    suffix = "a";
                    break;
                case 'o':
                    suffix = "o";
                    break;
                default:
                    if (cValue.EndsWith("ción"))
                        suffix = "ción";
                    else if (cValue.EndsWith("sión"))
                        suffix = "sión";
                    else if (cValue.EndsWith("dad"))
                        suffix = "dad";
                    else if (cValue.EndsWith("tad"))
                        suffix = "tad";
                    else if (cValue.EndsWith("umbre"))
                        suffix = "umbre";
                   else
                        suffix = "";
                    break;
            }

            suffix = "," + suffix;

            return suffix;
        }

        protected string GetAdjectiveCategoryStringSuffixES(string cValue)
        {
            if (String.IsNullOrEmpty(cValue))
                return "";

            char lastChr = cValue[cValue.Length - 1];
            string suffix;

            switch (lastChr)
            {
                case 'a':
                    suffix = "a";
                    break;
                case 'o':
                    suffix = "o";
                    break;
                default:
                    suffix = "";
                    break;
            }

            suffix = "," + suffix;

            return suffix;
        }

        public static string THTBreakpoint = null;

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

            if ((THTBreakpoint != null) && (targetWord == THTBreakpoint))
                ApplicationData.Global.PutConsoleMessage("THTBreakpoint: " + targetWord);

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
                        THTDictionarySourceIDList,
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
                THTDictionarySourceIDList,
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

        public static string THTDictionarySourceName = "THT";

        protected static int _THTDictionarySourceID = 0;
        public static int THTDictionarySourceID
        {
            get
            {
                if (_THTDictionarySourceID == 0)
                    _THTDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(THTDictionarySourceName);

                return _THTDictionarySourceID;
            }
        }

        protected static List<int> _THTDictionarySourceIDList = null;
        public static List<int> THTDictionarySourceIDList
        {
            get
            {
                if (_THTDictionarySourceIDList == null)
                    _THTDictionarySourceIDList = new List<int>(1) { THTDictionarySourceID };

                return _THTDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "THTDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
