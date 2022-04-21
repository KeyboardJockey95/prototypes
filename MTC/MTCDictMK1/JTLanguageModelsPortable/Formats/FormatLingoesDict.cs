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
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatLingoesDict : FormatDictionary
    {
        protected bool SaveCategories = false;
        protected List<string> UsedCategories = null;
        protected bool IgnoreConjugations = true;
        protected LanguageTool Tool = null;

        // Format data.
        private static string FormatDescription = "Format for .ldx format.  Inspired by lingoes-converter.";

        public FormatLingoesDict(
                string name,
                UserRecord userRecord,
                UserProfile userProfile,
                IMainRepository repositories,
                LanguageUtilities languageUtilities)
            : base(
                name,
                "FormatLingoesDict",
                FormatDescription,
                "application/octet-stream",
                ".ldx",
                userRecord,
                userProfile,
                repositories,
                languageUtilities,
                null)
        {
        }

        public FormatLingoesDict(FormatLingoesDict other)
            : base(other)
        {
        }

        public FormatLingoesDict()
            : base(
                  "LingoesDict",
                  "FormatLingoesDict",
                  FormatDescription,
                  "application/octet-stream",
                  ".ldx",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
        }

        public override Format Clone()
        {
            return new FormatLingoesDict(this);
        }

        public override void Read(Stream stream)
        {
            if (SaveCategories)
                UsedCategories = new List<string>();

            Tool = ApplicationData.LanguageTools.Create(TargetLanguageID);

            try
            {
                PreRead(10);

                State = StateCode.Reading;

                UpdateProgressElapsed("Reading Lingoes dictionary ...");

                MemoryBuffer dataBuffer = null;

                using (BinaryReader streamReader = new BinaryReader(stream))
                {
                    byte[] data = streamReader.ReadBytes((int)stream.Length);
                    dataBuffer = new MemoryBuffer(data);
                }

                UpdateProgressElapsed("Processing Lingoes dictionary ...");

                dataBuffer.Open(MediaInterfaces.PortableFileMode.Open);
                ProcessLingoesDictionary(dataBuffer);
                dataBuffer.Close();

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

                SetError(msg);
            }
            finally
            {
                if (!SaveCategories)
                    PostRead();
                else
                {
                    StringBuilder sb = new StringBuilder();

                    UsedCategories.Sort();

                    foreach (string cat in UsedCategories)
                        sb.AppendLine(cat);

                    string filePath = Crawler.ComposeCrawlerDataFilePath("LingoesDict", "Categories_" + TargetLanguageID.LanguageCode + ".txt");
                    FileSingleton.DirectoryExistsCheck(filePath);
                    FileSingleton.WriteAllText(filePath, sb.ToString());
                    WriteDictionaryDisplayOutput();
                }
            }
        }

        public static string LingoesBreakpoint = null;

        protected void ProcessWordDefinition(
            string word,
            List<string> definitions)
        {
            if ((LingoesBreakpoint != null) && (word == LingoesBreakpoint))
                ApplicationData.Global.PutConsoleMessage("LingoesBreakpoint 1: " + word);

            // Treat each definition (already filterd for duplicates) like a sense.
            foreach (string definition in definitions)
            {
                try
                {
                    if (definition.StartsWith("<"))
                        ProcessWordDefinitionXml(word, definition);
                    else
                        ProcessWordDefinitionRaw(word, definition);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error parsing Raw", exc);
                    return;
                }
            }
        }

        protected void ProcessWordDefinitionXml(
            string word,
            string definition)
        {
            FormatDictionaryRecord record = new FormatDictionaryRecord(
                word,
                TargetLanguageID,
                HostLanguageID);

            record.SourceIDs = LingoesDictionarySourceIDList;

            XElement element = XElement.Parse(definition, LoadOptions.None);
            ProcessC(record, element);
        }

        protected void ProcessWordDefinitionRaw(
            string targetWord,
            string definition)
        {
            int dc = definition.Length;
            int di;
            char chr;
            List<char> charStack = new List<char>();
            StringBuilder termSB = new StringBuilder();
            StringBuilder phraseSB = new StringBuilder();
            string term;
            string phrase;
            string lemma = null;
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = String.Empty;
            string nextLemma = null;
            LexicalCategory nextCategory = LexicalCategory.Unknown;
            string nextCategoryString = String.Empty;
            List<string> hostSynonyms = new List<string>();
            FormatDictionaryRecord record = null;
            List<FormatDictionaryRecord> records = new List<FormatDictionaryRecord>();

            for (di = 0; di < dc; di++)
            {
                chr = definition[di];

                switch (chr)
                {
                    case '(':
                    case '[':
                    case '{':
                    case '“':
                    case '‘':
                        charStack.Add(chr);
                        termSB.Append(chr);
                        break;
                    case ')':
                        if ((charStack.Count() != 0) && (charStack.Last() == '('))
                            charStack.RemoveAt(charStack.Count() - 1);
                        termSB.Append(chr);
                        break;
                    case ']':
                        if ((charStack.Count() != 0) && (charStack.Last() == '['))
                            charStack.RemoveAt(charStack.Count() - 1);
                        termSB.Append(chr);
                        break;
                    case '}':
                        if ((charStack.Count() != 0) && (charStack.Last() == '{'))
                            charStack.RemoveAt(charStack.Count() - 1);
                        termSB.Append(chr);
                        break;
                    case '”':
                        if ((charStack.Count() != 0) && (charStack.Last() == '“'))
                            charStack.RemoveAt(charStack.Count() - 1);
                        termSB.Append(chr);
                        break;
                    case '’':
                        if ((charStack.Count() != 0) && (charStack.Last() == '‘'))
                            charStack.RemoveAt(charStack.Count() - 1);
                        phraseSB.Append(chr);
                        termSB.Append(chr);
                        break;
                    case '"':
                        if ((charStack.Count() != 0) && (charStack.Last() == '"'))
                            charStack.RemoveAt(charStack.Count() - 1);
                        phraseSB.Append(chr);
                        termSB.Append(chr);
                        break;
                    case '\'':
                        if ((charStack.Count() != 0) && (charStack.Last() == '\''))
                            charStack.RemoveAt(charStack.Count() - 1);
                        phraseSB.Append(chr);
                        termSB.Append(chr);
                        break;
                    case ',':
                    case ';':
                        if (charStack.Count() != 0)
                        {
                            phraseSB.Append(chr);
                            termSB.Append(chr);
                        }
                        else
                        {
                            term = termSB.ToString().Trim();

                            if (!String.IsNullOrEmpty(term))
                            {
                                if (phraseSB.Length != 0)
                                {
                                    if (phraseSB[phraseSB.Length - 1] != ' ')
                                        phraseSB.Append(' ');
                                }

                                phraseSB.Append(term);
                            }

                            phrase = phraseSB.ToString().Trim();
                            if (!String.IsNullOrEmpty(phrase))
                                hostSynonyms.Add(phrase);
                            termSB.Clear();
                            phraseSB.Clear();
                        }
                        break;
                    case '.':
                        if (charStack.Count() != 0)
                        {
                            phraseSB.Append(chr);
                            termSB.Append(chr);
                        }
                        else if (termSB.Length == 0)
                            termSB.Append(chr);
                        else
                        {
                            // If we're leading with a term, but no category yet, assume it's a lemma and ignore the whole definition.
                            if ((category == LexicalCategory.Unknown) && (phraseSB.Length != 0))
                                return;

                            term = termSB.ToString();

                            if (ProcessPossibleLemmaPlusType(term, targetWord, out nextLemma, out nextCategory, out nextCategoryString))
                            {
                                // If we have a lemma, ignore the entire entry.
                                if (!String.IsNullOrEmpty(nextLemma))
                                    return;

                                if (!String.IsNullOrEmpty(nextLemma) && (nextCategory != LexicalCategory.Verb))
                                {
                                    if (phraseSB.Length != 0)
                                    {
                                        if (phraseSB[phraseSB.Length - 1] != ' ')
                                            phraseSB.Append(' ');
                                    }

                                    phraseSB.Append(nextLemma);
                                    nextLemma = null;
                                }

                                if (phraseSB.Length != 0)
                                {
                                    phrase = phraseSB.ToString().Trim();
                                    if (!String.IsNullOrEmpty(phrase))
                                        hostSynonyms.Add(phrase);
                                }

                                if (hostSynonyms.Count() != 0)
                                {
                                    record = new FormatDictionaryRecord(
                                        targetWord,
                                        TargetLanguageID,
                                        HostLanguageID);

                                    record.Lemma = lemma;
                                    record.Category = category;
                                    record.CategoryString = categoryString;
                                    record.SourceIDs = LingoesDictionarySourceIDList;

                                    foreach (string hostSynonym in hostSynonyms)
                                        ProcessHostSynonyms(record, hostSynonym);

                                    records.Add(record);

                                    hostSynonyms.Clear();
                                }

                                lemma = nextLemma;
                                category = nextCategory;
                                categoryString = nextCategoryString;
                                termSB.Clear();
                                phraseSB.Clear();
                            }
                            else
                                termSB.Append(chr);
                        }
                        break;
                    case ' ':
                        if (charStack.Count() != 0)
                            termSB.Append(chr);
                        else
                        {
                            if (phraseSB.Length != 0)
                            {
                                if (phraseSB[phraseSB.Length - 1] != ' ')
                                    phraseSB.Append(' ');
                            }

                            phraseSB.Append(termSB);
                            termSB.Clear();
                        }
                        break;
                    default:
                        termSB.Append(chr);
                        break;
                }
            }

            term = termSB.ToString().Trim();

            if (!String.IsNullOrEmpty(term))
            {
                if (phraseSB.Length != 0)
                {
                    if (phraseSB[phraseSB.Length - 1] != ' ')
                        phraseSB.Append(' ');
                }

                phraseSB.Append(term);
            }

            phrase = phraseSB.ToString().Trim();

            if (!String.IsNullOrEmpty(phrase))
                hostSynonyms.Add(phrase);

            if (hostSynonyms.Count() != 0)
            {
                record = new FormatDictionaryRecord(
                    targetWord,
                    TargetLanguageID,
                    HostLanguageID);

                record.Lemma = lemma;
                record.Category = category;
                record.CategoryString = categoryString;
                record.SourceIDs = LingoesDictionarySourceIDList;

                foreach (string hostSynonym in hostSynonyms)
                    ProcessHostSynonyms(record, hostSynonym);

                records.Add(record);
            }

            foreach (FormatDictionaryRecord aRecord in records)
            {
                if (FixupRecord(record, record.TargetText))
                    AddSimpleTargetEntryRecord(record);
            }
        }

        public bool ProcessPossibleLemmaPlusType(
            string term,
            string targetWord,
            out string lemma,
            out LexicalCategory category,
            out string categoryString)
        {
            lemma = null;
            category = LexicalCategory.Unknown;
            categoryString = null;

            if (RawTypeStrings.Contains(term))
                return ProcessCommonTypeRaw(term, targetWord, ref category, ref categoryString, false);
            else
            {
                foreach (string rawType in RawTypeStrings)
                {
                    if (term.EndsWith(rawType))
                    {
                        string front = term.Substring(0, term.Length - rawType.Length);

                        if (ProcessCommonTypeRaw(rawType, targetWord, ref category, ref categoryString, false))
                        {
                            int parenL = front.IndexOf('(');

                            if (parenL != -1)
                            {
                                int parenR = front.IndexOf(')', parenL + 1);

                                if (parenR != -1)
                                {
                                    int ofs = parenL + 1;
                                    int length = parenR - ofs;
                                    string subTypes = front.Substring(ofs, length);

                                    if (ProcessCommonTypeRaw(subTypes, targetWord, ref category, ref categoryString, false))
                                    {
                                        if (parenL > 0)
                                            lemma = front.Substring(0, parenL);

                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                lemma = front;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool ProcessCommonTypeRaw(
            string type,
            string targetWord,
            ref LexicalCategory category,
            ref string categoryString,
            bool prepend)
        {
            switch (type)
            {
                case "adj":
                    category = LexicalCategory.Adjective;
                    break;
                case "adv":
                    category = LexicalCategory.Adverb;
                    break;
                case "conj":
                    category = LexicalCategory.Conjunction;
                    break;
                case "interj":
                case "welcomeinterj":
                    category = LexicalCategory.Interjection;
                    break;
                case "n":
                    category = LexicalCategory.Noun;
                    break;
                case "pref":
                    category = LexicalCategory.Prefix;
                    break;
                case "prep":
                    category = LexicalCategory.Preposition;
                    break;
                case "pron":
                    category = LexicalCategory.Pronoun;
                    break;
                case "v":
                    category = LexicalCategory.Verb;
                    break;
                case "m o f":
                case "m/f":
                case "mf":
                    type = "mf";
                    break;
                case "m":
                case "ms":
                    type = "m,s";
                    break;
                case "f":
                case "fs":
                    type = "f,s";
                    break;
                case "mpl":
                    type = "m,pl";
                    break;
                case "fpl":
                    type = "f,pl";
                    break;
                case "mfs":
                    type = "mf,s";
                    break;
                case "mfpl":
                    type = "mf,pl";
                    break;
                case ";":
                    return false;
                default:
                    if (char.IsUpper(type[0]))
                    {
                        if ((category == LexicalCategory.Unknown) || (category == LexicalCategory.Noun))
                            category = LexicalCategory.ProperNoun;

                        if (targetWord.Contains("."))
                            category = LexicalCategory.Abbreviation;
                    }
                    return true;
            }

            if (prepend)
            {
                if (String.IsNullOrEmpty(categoryString))
                    categoryString = type;
                else
                    categoryString = type + "," + categoryString;
            }
            else
            {
                if (String.IsNullOrEmpty(categoryString))
                    categoryString = type;
                else
                    categoryString += "," + type;
            }

            if (SaveCategories)
            {
                if (!UsedCategories.Contains(type))
                    UsedCategories.Add(type);
            }

            return true;
        }

        protected bool FixupRecord(
            FormatDictionaryRecord record,
            string word)
        {
            if (word.Contains("."))
            {
                record.Category = LexicalCategory.Abbreviation;
                return true;
            }

            if (TextUtilities.IsUpperString(word))
            {
                record.Category = LexicalCategory.Acronym;
                return true;
            }

            if ((record.Category == LexicalCategory.Verb) &&
                (Tool != null) &&
                (Tool.VerbClassEndings != null))
            {
                bool found = false;

                foreach (string ending in Tool.VerbClassEndings)
                {
                    if (word.EndsWith(ending))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    record.Category = LexicalCategory.Inflection;
                    return !IgnoreConjugations;
                }
            }

            if (record.Category == LexicalCategory.Noun)
            {
                if (word.Contains(" "))
                    record.Category = LexicalCategory.Phrase;
            }

            if (record.Category == LexicalCategory.Adjective)
            {
                if (word.Contains(" "))
                    record.Category = LexicalCategory.Phrase;
            }

            if (record.Category == LexicalCategory.Unknown)
            {
                if (word.Contains(" "))
                    record.Category = LexicalCategory.Phrase;
            }

            if (Tool.CanInflectCategory(record.Category))
            {
                string categoryString;
                string classCode;
                string subClassCode;

                if (Tool.GetWordClassCategoryStringAndCodes(
                    word,
                    record.Category,
                    out categoryString,
                    out classCode,
                    out subClassCode))
                {
                    record.CategoryString = Tool.MergeCategoryStrings(
                        record.CategoryString,
                        categoryString);
                }
            }

            if (record.Category == LexicalCategory.Unknown)
                PutError("Category not found for: " + word);

            return true;
        }

        protected void ProcessC(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "C")
            {
                PutError("Got an top-level XML record that is not \"C\".");
                return;
            }

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements == null) || (childElements.Count() == 0))
                return;

            foreach (XElement childElement in childElements)
            {
                switch (childElement.Name.LocalName)
                {
                    case "F":
                        ProcessF(record, childElement);
                        break;
                    default:
                        throw new Exception("ProcessC: Unexpected child element type \"" + childElement.ToString() + "\" for word \"" + record.TargetText + "\".");
                }
            }
        }

        protected void ProcessF(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "F")
            {
                PutError("Expected an \"F\" element.");
                return;
            }

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements == null) || (childElements.Count() == 0))
                return;

            foreach (XElement childElement in childElements)
            {
                switch (childElement.Name.LocalName)
                {
                    case "H":
                        ProcessH(record, childElement);
                        break;
                    case "I":
                        ProcessI(record, childElement);
                        break;
                    default:
                        throw new Exception("ProcessF: Unexpected child element type \"" + childElement.ToString() + "\" for word \"" + record.TargetText + "\".");
                }
            }
        }

        protected void ProcessH(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "H")
            {
                PutError("Expected an \"H\" element.");
                return;
            }

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements == null) || (childElements.Count() == 0))
                return;

            foreach (XElement childElement in childElements)
            {
                switch (childElement.Name.LocalName)
                {
                    case "L":
                        ProcessL(record, childElement);
                        break;
                    case "M":
                        ProcessM(record, childElement);
                        break;
                    default:
                        throw new Exception("ProcessH: Unexpected child element type \"" + childElement.ToString() + "\" for word \"" + record.TargetText + "\".");
                }
            }
        }

        protected void ProcessL(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "L")
            {
                PutError("Expected an \"L\" element.");
                return;
            }

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements == null) || (childElements.Count() == 0))
            {
                string targetTermPlusType = GetElementText(element);
                ProcessTargetTermPlusTargetType(record, targetTermPlusType);
            }
            else
                throw new Exception("ProcessL: Child elements unexpected for \"L\" element for word \"" + record.TargetText + "\".");
        }

        protected void ProcessM(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "M")
            {
                PutError("Expected an \"M\" element.");
                return;
            }

            string IpaReading = GetElementText(element);
            record.IpaReading = IpaReading;

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements != null) && (childElements.Count() != 0))
            {
                foreach (XElement childElement in childElements)
                {
                    switch (childElement.Name.LocalName)
                    {
                        case "h":
                        case "n":
                            // Ignore.
                            break;
                        default:
                            throw new Exception("ProcessF: Unexpected child element type \"" + childElement.ToString() + "\" for word \"" + record.TargetText + "\".");
                    }
                }
            }
        }

        protected void ProcessI(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "I")
            {
                PutError("Expected an \"I\" element.");
                return;
            }

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements == null) || (childElements.Count() == 0))
                return;

            foreach (XElement childElement in childElements)
            {
                switch (childElement.Name.LocalName)
                {
                    case "N":
                        ProcessN(record, childElement);
                        break;
                    default:
                        throw new Exception("ProcessF: Unexpected child element type \"" + childElement.ToString() + "\" for word \"" + record.TargetText + "\".");
                }
            }
        }

        protected void ProcessN(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "N")
            {
                PutError("Expected an \"N\" element.");
                return;
            }

            string synonymsString = String.Empty;
            FormatDictionaryRecord localRecord = new FormatDictionaryRecord(record);
            IEnumerable<XNode> childNodes = element.Nodes();

            if ((childNodes != null) && (childNodes.Count() != 0))
            {
                foreach (XNode childNode in childNodes)
                {
                    switch (childNode.NodeType)
                    {
                        case XmlNodeType.Text:
                            {
                                string text = ((XText)childNode).Value.Trim();
                                if (!String.IsNullOrEmpty(text))
                                {
                                    if (!String.IsNullOrEmpty(synonymsString))
                                        synonymsString += " " + text;
                                    else
                                        synonymsString += text;
                                }
                            }
                            break;
                        case XmlNodeType.Element:
                            {
                                XElement childElement = (XElement)childNode;

                                switch (childElement.Name.LocalName)
                                {
                                    case "U":
                                        ProcessU(localRecord, childElement);
                                        break;
                                    case "h":
                                        {
                                            string text = childElement.Value.Trim();
                                            if (!String.IsNullOrEmpty(text))
                                            {
                                                if (!String.IsNullOrEmpty(synonymsString))
                                                    synonymsString += " " + text;
                                                else
                                                    synonymsString += text;
                                            }
                                        }
                                        break;
                                    default:
                                        throw new Exception("ProcessF: Unexpected child element type \"" + childElement.ToString() + "\" for word \"" + localRecord.TargetText + "\".");
                                }
                            }
                            break;
                    }
                }
            }

            ProcessHostSynonyms(localRecord, synonymsString);

            if (FixupRecord(localRecord, localRecord.TargetText))
                AddSimpleTargetEntryRecord(localRecord);
        }

        protected void ProcessU(
            FormatDictionaryRecord record,
            XElement element)
        {
            if (element.Name.LocalName != "U")
            {
                PutError("Expected an \"U\" element.");
                return;
            }

            IEnumerable<XElement> childElements = element.Elements();

            if ((childElements == null) || (childElements.Count() == 0))
            {
                string hostType = GetElementText(element);
                ProcessHostType(record, hostType);
            }
            else
                throw new Exception("ProcessL: Child elements unexpected for \"U\" element for word \"" + record.TargetText + "\".");
        }

        protected void ProcessHostType(
            FormatDictionaryRecord record,
            string hostType)
        {
            string type = hostType.Trim();

            if (type.EndsWith("."))
                type = type.Substring(0, type.Length - 1);

            if (type.Contains("."))
                PutError("ProcessHostType: Type contains inner period: " + hostType);

            if (!String.IsNullOrEmpty(type))
                ProcessCommonType(record, type, true);
        }

        protected void ProcessTargetTermPlusTargetType(
            FormatDictionaryRecord record,
            string targetTermPlusType)
        {
            string text = targetTermPlusType;
            string type;

            while ((type = ExtractType(ref text)) != null)
                ProcessCommonType(record, type, false);

            if (!String.IsNullOrEmpty(text))
            {
                if (text != record.TargetText)
                    record.AddAlternateText(record.TargetLanguageID, text);
            }
        }

        public static string[] TypeStrings =
        {
            "adj",
            "adv",
            "conj",
            "interj",
            "welcomeinterj",
            "n",
            "pref",
            "prep",
            "pron",
            "v"
        };

        public static string[] RawTypeStrings =
        {
            "adj",
            "adv",
            "conj",
            "interj",
            "welcomeinterj",
            "n",
            "pref",
            "prep",
            "pron",
            "v",
            "m",
            "m o f",
            "m/f",
            "mf",
            "ms",
            "f",
            "fs",
            "mpl",
            "fpl",
            "mfs",
            "mfpl",
            ";"
        };

        protected void ProcessTargetTermPlusHostType(
            FormatDictionaryRecord record,
            string targetTermPlusType)
        {
            string text = targetTermPlusType;

            if (!String.IsNullOrEmpty(text))
            {
                int ofsSpace = text.LastIndexOf(' ');

                if (ofsSpace != -1)
                {
                    string targetTerm = text.Substring(0, ofsSpace).Trim();

                    if (targetTerm != record.TargetText)
                        record.AddAlternateText(record.TargetLanguageID, targetTerm);

                    text = text.Substring(ofsSpace + 1);
                }

                int ofsLParen = text.LastIndexOf('(');
                int ofsRParen = text.LastIndexOf(')');

                if ((ofsRParen != -1) && (ofsLParen != -1))
                {
                    if (ofsLParen != 0)
                    {
                        string typeOrTargetTerm = text.Substring(0, ofsLParen).Trim();

                        if (TypeStrings.Contains(typeOrTargetTerm))
                            ProcessCommonType(record, typeOrTargetTerm, true);
                        else if (typeOrTargetTerm != record.TargetText)
                            record.AddAlternateText(record.TargetLanguageID, typeOrTargetTerm);

                        text = text.Substring(ofsRParen + 1);
                    }
                    else
                    {
                        string gender = text.Substring(ofsLParen + 1, (ofsRParen - ofsLParen) - 1);

                        ProcessCommonType(record, gender, false);

                        text = text.Remove(ofsLParen, (ofsRParen - ofsLParen) + 1).Trim();
                    }
                }

                int ofsPeriod = text.LastIndexOf('.');

                if (ofsPeriod != -1)
                {
                    string ipaOrType = text.Substring(0, ofsPeriod);

                    foreach (string type in RawTypeStrings)
                    {
                        if (ipaOrType.EndsWith(type))
                        {
                            ProcessCommonType(record, type, true);

                            if (ipaOrType.Length != type.Length)
                            {
                                string ipaReading = ipaOrType.Substring(0, ipaOrType.Length - type.Length);
                                record.IpaReading = ipaReading;
                            }
                        }
                    }
                }
            }
        }

        protected void ProcessCommonType(
            FormatDictionaryRecord record,
            string type,
            bool prepend)
        {
            switch (type)
            {
                case "adj":
                    record.Category = LexicalCategory.Adjective;
                    break;
                case "adv":
                    record.Category = LexicalCategory.Adverb;
                    break;
                case "conj":
                    record.Category = LexicalCategory.Conjunction;
                    break;
                case "interj":
                case "welcomeinterj":
                    record.Category = LexicalCategory.Interjection;
                    break;
                case "n":
                    record.Category = LexicalCategory.Noun;
                    break;
                case "pref":
                    record.Category = LexicalCategory.Prefix;
                    break;
                case "prep":
                    record.Category = LexicalCategory.Preposition;
                    break;
                case "pron":
                    record.Category = LexicalCategory.Pronoun;
                    break;
                case "v":
                    record.Category = LexicalCategory.Verb;
                    break;
                case "m o f":
                case "m/f":
                case "mf":
                    type = "mf";
                    break;
                case "m":
                case "ms":
                    type = "m,s";
                    break;
                case "f":
                case "fs":
                    type = "f,s";
                    break;
                case "mpl":
                    type = "m,pl";
                    break;
                case "fpl":
                    type = "f,pl";
                    break;
                case "mfs":
                    type = "mf,s";
                    break;
                case "mfpl":
                    type = "mf,pl";
                    break;
                case ";":
                    return;
                default:
                    if (char.IsUpper(type[0]))
                    {
                        if ((record.Category == LexicalCategory.Unknown) || (record.Category == LexicalCategory.Noun))
                            record.Category = LexicalCategory.ProperNoun;

                        if (record.TargetText.Contains("."))
                            record.Category = LexicalCategory.Abbreviation;
                    }
                    if (type != record.TargetText)
                        record.AddAlternateText(record.TargetLanguageID, type);
                    return;
            }

            if (prepend)
                record.PrependCategoryString(type);
            else
                record.AppendCategoryString(type);

            if (SaveCategories)
            {
                if (!UsedCategories.Contains(type))
                    UsedCategories.Add(type);
            }
        }

        protected string ExtractType(ref string text)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            int ofs = text.IndexOf('(');

            if (ofs != -1)
            {
                int endOfs = text.IndexOf(')', ofs + 1);

                if (endOfs != -1)
                {
                    string type = text.Substring(ofs + 1, (endOfs - ofs) - 1);
                    text = text.Remove(ofs, (endOfs - ofs) + 1).Trim();
                    return type;
                }
            }

            return null;
        }

        protected static string[] RemoveThese =
        {
            "\"",
            "“",
            "”",
            "{",
            "}"
        };

        protected void ProcessHostSynonyms(
            FormatDictionaryRecord record,
            string synonymsString)
        {
            string text = synonymsString;

            text = TextUtilities.RemoveStrings(text, RemoveThese);

            if (!text.Contains(",") && !text.Contains(";") && !text.Contains(".") && !text.Contains("/"))
                record.AppendHostSynonym(text);
            else
            {
                int index;
                int length = text.Length;
                StringBuilder sb = new StringBuilder();
                int parenLevel = 0;
                string hostSynonym;

                for (index = 0; index < length; index++)
                {
                    char chr = text[index];

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
                                if (!char.IsWhiteSpace(text[i]))
                                    break;
                            }
                            break;
                        case ';':
                        case ',':
                            if (parenLevel <= 0)
                            {
                                hostSynonym = sb.ToString().Trim();
                                if (hostSynonym.Length != 0)
                                    record.AppendHostSynonym(hostSynonym);
                                sb.Clear();
                                for (int i = index + 1; i < length; i++, index++)
                                {
                                    if (!char.IsWhiteSpace(text[i]))
                                        break;
                                }
                            }
                            break;
                        case '.':
                            if (parenLevel <= 0)
                            {
                                sb.Append(chr);
                                record.AppendHostSynonym(sb.ToString().Trim());
                                sb.Clear();
                                for (int i = index + 1; i < length; i++, index++)
                                {
                                    if (!char.IsWhiteSpace(text[i]))
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
                        record.AppendHostSynonym(hostSynonym);
                }
            }
        }

        protected string GetElementText(XElement element)
        {
            StringBuilder sb = new StringBuilder();

            foreach (XNode node in element.Nodes())
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    XText textNode = (XText)node;
                    sb.Append(textNode.Value);
                }
                else if (node.NodeType == XmlNodeType.Element)
                {
                    XElement elementNode = (XElement)node;
                    sb.Append(elementNode.Value);
                }
            }

            string text = sb.ToString().Trim();

            return text;
        }

        protected string GetElementLocalText(XElement element)
        {
            StringBuilder sb = new StringBuilder();

            foreach (XNode node in element.Nodes())
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    XText textNode = (XText)node;
                    sb.Append(textNode.Value);
                }
            }

            string text = sb.ToString().Trim();

            return text;
        }

        protected void ProcessLingoesDictionary(MemoryBuffer dataBuffer)
        {
            int offsetData = dataBuffer.GetInteger(0x5C, sizeof(int), true, false) + 0x60;

            if (dataBuffer.Length > offsetData)
            {
                int counter;
                int type = dataBuffer.GetInteger(offsetData, sizeof(int), true, false);
                int offsetWithInfo = dataBuffer.GetInteger(offsetData + 4, sizeof(int), true, false) + offsetData + 12;
                if (type == 3)
                    counter = ReadDictionary(dataBuffer, offsetData);
                else if (dataBuffer.Length > (offsetWithInfo + 0x1C))
                    counter = ReadDictionary(dataBuffer, offsetWithInfo);
                else
                    SetError("Unexpected file type.");
            }
            else
                SetError("Unexpected file format.");
        }

        private int ReadDictionary(MemoryBuffer dataBuffer, int offsetData)
        {
            int counter = 0;
            int limit = dataBuffer.GetInteger(offsetData + 4, sizeof(int), true, false) + offsetData + 8;
            int offsetIndex = offsetData + 0x1C;
            int offsetCompressedDataHeader = dataBuffer.GetInteger(offsetData + 8, sizeof(int), true, false) + offsetIndex;
            int inflatedWordsIndexLength = dataBuffer.GetInteger(offsetData + 12, sizeof(int), true, false);
            int inflatedWordsLength = dataBuffer.GetInteger(offsetData + 16, sizeof(int), true, false);
            List<int> deflateStreams = new List<int>();
            int position = offsetCompressedDataHeader + 8;
            int offset = dataBuffer.GetInteger(position, sizeof(int), true, false);

            while ((offset + position) < limit)
            {
                offset = dataBuffer.GetInteger(position, sizeof(int), true, false);
                deflateStreams.Add(offset);
                position += sizeof(int);
            }

            MemoryBuffer inflatedBytes = Inflate(dataBuffer, deflateStreams, position);

            counter = Extract(
                inflatedBytes,
                inflatedWordsIndexLength,
                inflatedWordsIndexLength + inflatedWordsLength);

            return counter;
        }

        private int Extract(
            MemoryBuffer inflatedBytes,
            int offsetDefs,
            int offsetXml)
        {
            int counter = 0;
            StringBuilder outputWriter = new StringBuilder();

            try
            {
                int dataLen = 10;
                int defTotal = (offsetDefs / dataLen) - 1;

                SetStatus(defTotal, 0, 0);

                int failCounter = 0;

                byte[] inflatedData = inflatedBytes.GetAllBytes();
                inflatedBytes.Open(PortableFileMode.Open);

                for (int i = 0; i < defTotal; i++)
                {
                    string word;
                    List<string> definitions = new List<string>();

                    ReadDefinitionData(
                        inflatedBytes,
                        inflatedData,
                        offsetDefs,
                        offsetXml,
                        dataLen,
                        ApplicationData.Encoding,
                        ApplicationData.Encoding,
                        out word,
                        definitions,
                        i);

                    if (String.IsNullOrEmpty(word) || (definitions.Count() == 0))
                    {
                        failCounter++;
                    }

                    if (failCounter > (defTotal * 0.01))
                    {
                        SetError("??");
                        SetError(word + " = " + ObjectUtilities.GetStringFromStringList(definitions));
                    }

                    if (!String.IsNullOrEmpty(RawDisplayFilePath))
                    {
                        PutRawDisplayLine("W: " + word);      // Word

                        for (int j = 0; j < definitions.Count(); j++)
                            PutRawDisplayLine("D[" + j.ToString() + "]: " + definitions[j]);      // Xml or raw definition
                    }

                    //SplitDefinitions(definitions);

                    // Extract the dictionary entry from the word and definitions.
                    ProcessWordDefinition(word, definitions);

                    counter++;
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception during conversion", exc);
            }
            finally
            {
                inflatedBytes.Close();
            }

            return counter;
        }

        private void SplitDefinitions(List<string> definitions)
        {
            for (int i = 0; i < definitions.Count(); i++)
            {
                string definition = definitions[i].Trim();

                if (String.IsNullOrEmpty(definition))
                    definitions.RemoveAt(i);
                else if (definition[0] != '<')
                {
                    int startIndex = 0;

                    again:
                    int ofs = definition.IndexOf('.', startIndex);

                    if (ofs != -1)
                    {
                        if ((definition.IndexOf('(', ofs) != -1) && (definition.IndexOf(')', ofs) == -1))
                        {
                            startIndex = ofs + 1;
                            goto again;
                        }
                    }

                    if (ofs != -1)
                    {
                    restart:
                        int secondOfs = definition.IndexOf('.', ofs + 1);

                        if (secondOfs != -1)
                        {
                            int testOfs = secondOfs - 1;

                            while ((testOfs > ofs) && !char.IsWhiteSpace(definition[testOfs]))
                                testOfs--;

                            if (testOfs != ofs)
                            {
                                string def1 = definition.Substring(0, testOfs);
                                string def2 = definition.Substring(testOfs + 1);
                                definitions.Insert(i, def1);
                                definitions[i + 1] = def2;
                            }
                            else
                            {
                                ofs = secondOfs;
                                secondOfs = definition.IndexOf('.', secondOfs + 1);
                                goto restart;
                            }
                        }
                    }
                }
            }
        }

        private void ReadDefinitionData(
            MemoryBuffer inflatedBytes,
            byte[] inflatedData,
            int offsetWords,
            int offsetXml,
            int dataLen,
            Encoding wordDecoder,
            Encoding valueDecoder,
            out string word,
            List<string> definitions,
            int idx)
        {
            int[] wordIdxData = new int[6];
            GetIdxData(inflatedBytes, dataLen * idx, wordIdxData);
            int lastWordPos = wordIdxData[0];
            int lastXmlPos = wordIdxData[1];
            int refs = wordIdxData[3];
            int currentWordOffset = wordIdxData[4];
            int currenXmlOffset = wordIdxData[5];
            String xml = valueDecoder.GetString(
                inflatedData,
                offsetXml + lastXmlPos,
                currenXmlOffset - lastXmlPos).Trim();

            if (!String.IsNullOrEmpty(xml))
            {
                if (!definitions.Contains(xml))
                    definitions.Insert(0, xml);
            }

            while (refs-- > 0)
            {
                int refnum = inflatedBytes.GetInteger(offsetWords + lastWordPos, sizeof(int), true, true);
                GetIdxData(inflatedBytes, dataLen * refnum, wordIdxData);
                lastXmlPos = wordIdxData[1];
                currenXmlOffset = wordIdxData[5];
                xml = Strip(
                    valueDecoder.GetString(
                        inflatedData,
                        offsetXml + lastXmlPos,
                        currenXmlOffset - lastXmlPos));

                if (!String.IsNullOrEmpty(xml))
                {
                    if (!definitions.Contains(xml))
                        definitions.Insert(0, xml);
                }

                lastWordPos += 4;
            }

            word = wordDecoder.GetString(
                inflatedData,
                offsetWords + lastWordPos,
                currentWordOffset - lastWordPos).Trim();
        }

        private String Strip(String xml)
        {
            int open = 0;
            int end = 0;
            int start;
            int length;

            if ((open = xml.IndexOf("<![CDATA[")) != -1)
            {
                if ((end = xml.IndexOf("]]>", open)) != -1)
                {
                    start = open + "<![CDATA[".Length;
                    length = end - start;
                    return xml.Substring(start, length).Replace('\t', ' ')
                            .Replace('\n', ' ').Replace('\u001e', ' ').Replace('\u001f', ' ').Trim();
                }
            }
            else if ((open = xml.IndexOf("<Ô")) != -1)
            {
                if ((end = xml.IndexOf("</Ô", open)) != -1)
                {
                    open = xml.IndexOf(">", open + 1);
                    start = open + 1;
                    length = end - start;
                    return xml.Substring(start, length).Replace('\t', ' ').Replace('\n', ' ')
                            .Replace('\u001e', ' ').Replace('\u001f', ' ').Trim();
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                end = 0;
                open = xml.IndexOf('<');
                do
                {
                    if ((open - end) > 1)
                    {
                        start = end + 1;
                        length = open - start;
                        sb.Append(xml.Substring(start, length));
                    }
                    open = xml.IndexOf('<', open + 1);
                    end = xml.IndexOf('>', end + 1);
                }
                while ((open != -1) && (end != -1));

                return sb.ToString().Replace('\t', ' ').Replace('\n', ' ').Replace('\u001e', ' ')
                        .Replace('\u001f', ' ').Trim();
            }
            return String.Empty;
        }

        private void GetIdxData(
            MemoryBuffer dataBuffer,
            int position,
            int[] wordIdxData)
        {
            wordIdxData[0] = dataBuffer.GetInteger(position, sizeof(int), true, true);
            position += sizeof(int);
            wordIdxData[1] = dataBuffer.GetInteger(position, sizeof(int), true, true);
            position += sizeof(int);
            wordIdxData[2] = dataBuffer.GetByte(position) & 0xff;
            position += sizeof(byte);
            wordIdxData[3] = dataBuffer.GetByte(position) & 0xff;
            position += sizeof(byte);
            wordIdxData[4] = dataBuffer.GetInteger(position, sizeof(int), true, true);
            position += sizeof(int);
            wordIdxData[5] = dataBuffer.GetInteger(position, sizeof(int), true, true);
            position += sizeof(int);
        }

        private MemoryBuffer Inflate(
            MemoryBuffer dataBuffer,
            List<int> deflateStreams,
            int position)
        {
            int startOffset = position;
            int offset = -1;
            int lastOffset = startOffset;
            MemoryStream output = new MemoryStream();
            IArchiveFile archiveFile = FileSingleton.Archive();

            for (int index = 0; index < deflateStreams.Count(); index++)
            {
                int offsetRelative = deflateStreams[index];
                offset = startOffset + offsetRelative;
                Decompress(output, archiveFile, dataBuffer, lastOffset, offset - lastOffset);
                lastOffset = offset;
            }

            output.Seek(0, SeekOrigin.Begin);

            return new MemoryBuffer(output);
        }

        private long Decompress(
            MemoryStream output,
            IArchiveFile archiveFile,
            MemoryBuffer data,
            int offset,
            int length)
        {
            long bytesRead = -1;
            byte[] compressed;
            byte[] inflated;

            try
            {
                compressed = data.GetBytes(offset, length);

                if (compressed == null)
                    return bytesRead;

                if (compressed.Length == 0)
                    return 0;

                if (archiveFile.Decompress(compressed, out inflated))
                {
                    output.Write(inflated, 0, inflated.Length);
                    bytesRead = inflated.Length;
                }
                else
                    SetError("Error inflating chunk.");
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception inflating chunk", exc);
            }

            return bytesRead;
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

            if ((LingoesBreakpoint != null) && (targetWord == LingoesBreakpoint))
                ApplicationData.Global.PutConsoleMessage("LingoesBreakpoint 2: " + targetWord);

            string stemCategoryString;
            string targetStem = Tool.GetStem(targetWord, TargetLanguageID, out stemCategoryString);
            DictionaryEntry stemDictionaryEntry;

            if (targetStem == null)
                return;

            string categoryString = sense.CategoryString;
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
                        LingoesDictionarySourceIDList,
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
                LingoesDictionarySourceIDList,
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

        public void SetStatus(long total, long finished, int numPerSecond)
        {
        }

        public void SetError(string errorMessage)
        {
            PutError(errorMessage);
            ApplicationData.Global.PutConsoleErrorMessage(errorMessage);
        }

        public static string LingoesDictionarySourceName = "Lingoes";

        protected static int _LingoesDictionarySourceID = 0;
        public static int LingoesDictionarySourceID
        {
            get
            {
                if (_LingoesDictionarySourceID == 0)
                    _LingoesDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(LingoesDictionarySourceName);

                return _LingoesDictionarySourceID;
            }
        }

        protected static List<int> _LingoesDictionarySourceIDList = null;
        public static List<int> LingoesDictionarySourceIDList
        {
            get
            {
                if (_LingoesDictionarySourceIDList == null)
                    _LingoesDictionarySourceIDList = new List<int>(1) { LingoesDictionarySourceID };

                return _LingoesDictionarySourceIDList;
            }
        }

        public static new string TypeStringStatic { get { return "LingoesDict"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
