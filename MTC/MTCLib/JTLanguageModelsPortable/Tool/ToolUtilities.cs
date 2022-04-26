using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Tool
{
    public class ToolUtilities : ControllerUtilities
    {
        public BaseObjectNodeTree Tree { get; set; }
        public LanguageID ToolHostLanguageID;
        public LanguageID ToolTargetLanguageID;
        public LanguageID ToolTargetAlternate1LanguageID;
        public LanguageID ToolTargetAlternate2LanguageID;
        public LanguageID ToolTargetAlternate3LanguageID;
        public List<LanguageID> ToolHostLanguageIDs;
        public List<LanguageID> ToolTargetLanguageIDs;
        public List<LanguageID> ToolTargetHostLanguageIDs;

        public ToolUtilities(IMainRepository repositories, IApplicationCookies cookies,
                UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
                LanguageUtilities languageUtilities, BaseObjectNodeTree tree, ToolProfile toolProfile)
            : base(repositories, cookies, userRecord, userProfile, translator, languageUtilities)
        {
            Tree = tree;
            UpdateLanguages(tree, toolProfile);
        }

        public ToolUtilities()
        {
            UpdateLanguages(null, null);
        }

        public void UpdateLanguages(BaseObjectNodeTree tree, ToolProfile toolProfile)
        {
            if (toolProfile != null)
            {
                ToolHostLanguageIDs = toolProfile.HostLanguageIDs;
                ToolTargetLanguageIDs = toolProfile.TargetLanguageIDs;
                ToolTargetHostLanguageIDs = toolProfile.TargetHostLanguageIDs;
                ToolHostLanguageID = toolProfile.FirstHostLanguageID;
                ToolTargetLanguageID = toolProfile.FirstTargetLanguageID;
            }
            else if (UserProfile != null)
            {
                ToolHostLanguageIDs = UserProfile.HostLanguageIDs;
                ToolTargetLanguageIDs = UserProfile.TargetLanguageIDs;
                ToolTargetHostLanguageIDs = UserProfile.TargetHostLanguageIDs;
                ToolHostLanguageID = UserProfile.HostLanguageID;
                ToolTargetLanguageID = UserProfile.TargetLanguageIDSafe;
            }
            else if (tree != null)
            {
                ToolHostLanguageIDs = tree.HostLanguageIDs;
                ToolTargetLanguageIDs = tree.TargetLanguageIDs;
                ToolTargetHostLanguageIDs = tree.TargetHostLanguageIDs;
                ToolHostLanguageID = tree.FirstHostLanguageID;
                ToolTargetLanguageID = tree.FirstTargetLanguageID;
            }

            if (ToolTargetLanguageIDs.Count() >= 2)
                ToolTargetAlternate1LanguageID = ToolTargetLanguageIDs[1];
            else
                ToolTargetAlternate1LanguageID = null;

            if (ToolTargetLanguageIDs.Count() >= 3)
                ToolTargetAlternate2LanguageID = ToolTargetLanguageIDs[2];
            else
                ToolTargetAlternate2LanguageID = null;

            if (ToolTargetLanguageIDs.Count() >= 4)
                ToolTargetAlternate3LanguageID = ToolTargetLanguageIDs[3];
            else
                ToolTargetAlternate3LanguageID = null;
        }

        // Keep in sync with ToolTypeCode.
        public static List<string> ToolTypeStrings = new List<string>()
        {
            "Unknown",
            "Flash",
            "Flip",
            "Hands Free",
            "Match",
            "Test",
            "Text",
            "List"
        };

        public static ToolTypeCode GetToolTypeCodeFromString(string str)
        {
            ToolTypeCode toolType = ToolTypeCode.Unknown;

            if (str == null)
                return toolType;

            switch (str.ToLower())
            {
                case "unknown":
                    toolType = ToolTypeCode.Unknown;
                    break;
                case "flash":
                    toolType = ToolTypeCode.Flash;
                    break;
                case "flip":
                    toolType = ToolTypeCode.Flip;
                    break;
                case "hands free":
                case "handsfree":
                    toolType = ToolTypeCode.HandsFree;
                    break;
                case "match":
                    toolType = ToolTypeCode.Match;
                    break;
                case "test":
                    toolType = ToolTypeCode.Test;
                    break;
                case "text":
                    toolType = ToolTypeCode.Text;
                    break;
                case "list":
                    toolType = ToolTypeCode.List;
                    break;
                default:
                    break;
            }

            return toolType;
        }

        public static string GetToolTypeStringFromCode(ToolTypeCode toolType)
        {
            string str = "Unknown";

            switch (toolType)
            {
                case ToolTypeCode.Unknown:
                    str = "Unknown";
                    break;
                case ToolTypeCode.Flash:
                    str = "Flash";
                    break;
                case ToolTypeCode.Flip:
                    str = "Flip";
                    break;
                case ToolTypeCode.HandsFree:
                    str = "Hands Free";
                    break;
                case ToolTypeCode.Match:
                    str = "Match";
                    break;
                case ToolTypeCode.Test:
                    str = "Test";
                    break;
                case ToolTypeCode.Text:
                    str = "Text";
                    break;
                case ToolTypeCode.List:
                    str = "List";
                    break;
                default:
                    break;
            }

            return str;
        }

        // Keep in sync with ToolSourceCode.
        public static List<string> ToolSourceStrings = new List<string>()
        {
            "Unknown",
            "StudyList",
            "VocabularyList",
            "StudyListInflections",
            "VocabularyListInflections"
        };

        public static ToolSourceCode GetToolSourceCodeFromString(string str)
        {
            ToolSourceCode toolSource = ToolSourceCode.Unknown;

            if (str == null)
                return toolSource;

            switch (str.ToLower())
            {
                case "unknown":
                    toolSource = ToolSourceCode.Unknown;
                    break;
                case "studylist":
                case "study list":
                    toolSource = ToolSourceCode.StudyList;
                    break;
                case "vocabulary":
                case "vocabularylist":
                case "vocabulary list":
                    toolSource = ToolSourceCode.VocabularyList;
                    break;
                case "studylistinflections":
                case "study list inflections":
                    toolSource = ToolSourceCode.StudyListInflections;
                    break;
                case "vocabularyinflections":
                case "vocabulary inflections":
                case "vocabularylistinflections":
                case "vocabulary list inflections":
                    toolSource = ToolSourceCode.VocabularyListInflections;
                    break;
                default:
                    break;
            }

            return toolSource;
        }

        public static string GetToolSourceStringFromCode(ToolSourceCode toolSource)
        {
            string str = "Unknown";

            switch (toolSource)
            {
                case ToolSourceCode.Unknown:
                    str = "Unknown";
                    break;
                case ToolSourceCode.StudyList:
                    str = "StudyList";
                    break;
                case ToolSourceCode.VocabularyList:
                    str = "VocabularyList";
                    break;
                case ToolSourceCode.StudyListInflections:
                    str = "StudyListInflections";
                    break;
                case ToolSourceCode.VocabularyListInflections:
                    str = "VocabularyListInflections";
                    break;
                default:
                    break;
            }

            return str;
        }

        // Keep in sync with ToolViewCode.
        public static List<string> ToolViewStrings = new List<string>()
        {
            "Unknown",
            "Study",
            "Select",
            "List"
        };

        public static ToolViewCode GetToolViewCodeFromString(string str)
        {
            ToolViewCode toolView = ToolViewCode.Unknown;

            if (str == null)
                return toolView;

            switch (str.ToLower())
            {
                case "unknown":
                    toolView = ToolViewCode.Unknown;
                    break;
                case "study":
                    toolView = ToolViewCode.Study;
                    break;
                case "select":
                    toolView = ToolViewCode.Select;
                    break;
                case "list":
                    toolView = ToolViewCode.List;
                    break;
                default:
                    break;
            }

            return toolView;
        }

        public static string GetToolViewStringFromCode(ToolViewCode toolView)
        {
            string str = "Unknown";

            switch (toolView)
            {
                case ToolViewCode.Unknown:
                    str = "Unknown";
                    break;
                case ToolViewCode.Study:
                    str = "Study";
                    break;
                case ToolViewCode.Select:
                    str = "Select";
                    break;
                case ToolViewCode.List:
                    str = "List";
                    break;
                default:
                    break;
            }

            return str;
        }

        public static string ComposeToolStudyListKey(
            UserRecord userRecord,
            BaseObjectContent content,
            ToolSourceCode toolSource)
        {
            if (userRecord == null)
                return null;

            if (content == null)
                return null;

            if (content.ContentStorageKey < 1)
                return null;

            BaseObjectNodeTree tree = content.Tree;

            if (tree == null)
                return null;

            string treeHash = tree.SourceKeyHash;
            string sourceHash = "S";

            switch (toolSource)
            {
                case ToolSourceCode.StudyList:
                default:
                    sourceHash = "S";
                    break;
                case ToolSourceCode.VocabularyList:
                    sourceHash = "V";
                    break;
                case ToolSourceCode.StudyListInflections:
                    sourceHash = "SI";
                    break;
                case ToolSourceCode.VocabularyListInflections:
                    sourceHash = "VI";
                    break;
            }

            string toolStudyListKey = userRecord.UserNameOrGuest
                + ":" + userRecord.CurrentUserProfile.ProfileOrdinal.ToString()
                + ":" + treeHash
                + ":" + content.KeyString
                + ":" + content.ContentStorageKey.ToString()
                + ":" + sourceHash;

            return toolStudyListKey;
        }

        public static string ComposeToolStudyListSearchKeyPattern(BaseObjectContent content)
        {
            if (content == null)
                return null;

            if (content.ContentStorageKey < 1)
                return null;

            BaseObjectNodeTree tree = content.Tree;

            if (tree == null)
                return null;

            string treeHash = tree.SourceKeyHash;

            string toolStudyListKeyPattern = ".*"
                + ":" + ".*"
                + ":" + treeHash
                + ":" + content.KeyString
                + ":" + content.ContentStorageKey.ToString()
                + ":" + ".*";

            return toolStudyListKeyPattern;
        }

        public static string ComposeToolSessionKey(UserRecord userRecord, int index)
        {
            if (userRecord == null)
                return null;

            if ((index < 0) || (index > 3))
                return null;

            string toolSessionKey = userRecord.UserNameOrGuest
                + ":" + userRecord.CurrentUserProfile.ProfileOrdinal.ToString()
                + ":" + index.ToString();

            return toolSessionKey;
        }

        public static string ComposeToolProfileKey(UserRecord userRecord, string name)
        {
            if (userRecord == null)
                return null;

            string toolProfileKey = userRecord.UserNameOrGuest
                + ":" + userRecord.CurrentUserProfile.ProfileOrdinal.ToString()
                + ":" + name;

            return toolProfileKey;
        }

        public static string ComposeToolProfileDefaultKey(UserRecord userRecord)
        {
            if (userRecord == null)
                return null;

            string toolProfileKey = userRecord.UserNameOrGuest + "_Default"
                + ":" + userRecord.CurrentUserProfile.ProfileOrdinal.ToString();

            return toolProfileKey;
        }

        public static string GetToolProfileName(string toolProfileKey)
        {
            string name = String.Empty;

            if (!String.IsNullOrEmpty(toolProfileKey))
            {
                int ofs = toolProfileKey.LastIndexOf(':');

                if (ofs != -1)
                    name = toolProfileKey.Substring(ofs + 1);
            }

            return name;
        }

        public static string ComposeToolStudyItemKey(int index)
        {
            string toolStudyItemKey = "I" + index.ToString();
            return toolStudyItemKey;
        }

        public static void FilterToolStudyItems(
            List<ToolStudyItem> toolStudyItems,
            object configurationKey,
            bool showNew,
            bool showActive,
            bool showLearned)
        {
            if (!showNew || !showActive || !showLearned)
            {
                for (int i = toolStudyItems.Count() - 1; i >= 0; i--)
                {
                    ToolStudyItem testItem = toolStudyItems[i];
                    ToolItemStatus toolItemStatus = testItem.GetStatus(configurationKey);

                    switch (toolItemStatus.StatusCode)
                    {
                        case ToolItemStatusCode.Future:
                            if (!showNew)
                                toolStudyItems.RemoveAt(i);
                            break;
                        case ToolItemStatusCode.Active:
                            if (!showActive)
                                toolStudyItems.RemoveAt(i);
                            break;
                        case ToolItemStatusCode.Learned:
                            if (!showLearned)
                                toolStudyItems.RemoveAt(i);
                            break;
                        default:
                            toolStudyItems.RemoveAt(i);
                            break;
                    }
                }
            }
        }

        public static void FilterHiddenToolStudyItems(
            List<ToolStudyItem> toolStudyItems)
        {
            for (int i = toolStudyItems.Count() - 1; i >= 0; i--)
            {
                ToolStudyItem testItem = toolStudyItems[i];

                if (testItem.IsStudyItemHidden)
                {
                    toolStudyItems.RemoveAt(i);
                    continue;
                }
            }
        }

        public static void SortToolStudyItems(
            List<ToolStudyItem> toolStudyItems,
            object configurationKey,
            LanguageID targetLanguageID,
            List<LanguageDescriptor> languageDescriptors,
            List<string> sortOrder)
        {
            if ((sortOrder != null) && (sortOrder.Count() != 0))
                toolStudyItems.Sort(ToolStudyItem.GetComparer(sortOrder, targetLanguageID, configurationKey, languageDescriptors));
        }

        private enum ConfigurationSpecifier
        {
            Name,           // {Name:_}
            Select,         // {Select:_}
            Level,          // {Level:_}
            ChoiceSize,     // {ChoiceSize:_}
            ChunkSize,      // {ChunkSize:_}
            HistorySize,    // {HistorySize:_}
            IsShowIndex,    // {IsShowIndex:_}
            TextFormat,     // {TextFormat:_}
            FontFamily,     // {FontFamily:_}
            FlashFontSize,  // {FlashFontSize:_}
            ListFontSize,   // {ListFontSize:_}
            MaximumLineLength,    // {MaximumLineLength:_}
            Subs,           // {Subs:_}  i.e. {Subs:Read0, Trans0}
            Read,
            Translate,
            Listen,
            See,
            Speak,
            Recognize,
            Dictate,
            Descramble,
            Choice,
            Blanks,
            Hybrid,
            Text,
            Picture,
            Audio,
            Video,
            Write,
            T,              // {T}
            TA,             // {TA}
            AT,             // {AT}
            TL,             // {TL}
            TH,             // {TH}
            TAH,            // {TAH}
            H,              // {H}
            HA,             // {HA}
            AH,             // {AH}
            HL,             // {HL}
            HT,             // {HT}
            HAT,            // {HAT}
            Colon,          // :
            Plus,           // +
            CardSep,        // /
            None
        };

        private class Token
        {
            public ConfigurationSpecifier Specifier { get; set; }
            public string Value { get; set; }

            public Token(ConfigurationSpecifier specifier, string value)
            {
                Specifier = specifier;
                Value = value;
            }

            public Token(ConfigurationSpecifier specifier)
            {
                Specifier = specifier;
                Value = null;
            }
        }

        private Token NextToken(string configurationSpecification, int index, out int outIndex, out string message)
        {
            Token token = null;
            int count = configurationSpecification.Length;

            while ((index < count) && Char.IsWhiteSpace(configurationSpecification[index]))
                index++;

            outIndex = index + 1;
            message = "";

            if (index >= count)
                return null;

            switch (configurationSpecification[index])
            {
                case '{':
                    {
                        index++;
                        int nameStartIndex = index;
                        string value = null;
                        while ((index < count) && Char.IsLetter(configurationSpecification[index]))
                            index++;
                        string tag = configurationSpecification.Substring(nameStartIndex, index - nameStartIndex);
                        if ((configurationSpecification[index] == ':') || (configurationSpecification[index] == '='))
                        {
                            index++;
                            if (configurationSpecification[index] == '"')
                            {
                                index++;
                                int valueStartIndex = index;
                                while ((index < count) && (configurationSpecification[index] != '"'))
                                    index++;
                                value = configurationSpecification.Substring(valueStartIndex, index - valueStartIndex);
                                if (configurationSpecification[index] == '"')
                                    index++;
                            }
                            else
                            {
                                int valueStartIndex = index;
                                while ((index < count) && (configurationSpecification[index] != '}'))
                                    index++;
                                value = configurationSpecification.Substring(valueStartIndex, index - valueStartIndex);
                            }
                        }
                        index++;    // Skip '}'
                        outIndex = index;
                        ConfigurationSpecifier spec;
                        switch (tag)
                        {
                            case "Name":
                                spec = ConfigurationSpecifier.Name;
                                break;
                            case "Select":
                                spec = ConfigurationSpecifier.Select;
                                break;
                            case "Level":
                                spec = ConfigurationSpecifier.Level;
                                break;
                            case "ChoiceSize":
                                spec = ConfigurationSpecifier.ChoiceSize;
                                break;
                            case "ChunkSize":
                                spec = ConfigurationSpecifier.ChunkSize;
                                break;
                            case "HistorySize":
                                spec = ConfigurationSpecifier.HistorySize;
                                break;
                            case "IsShowIndex":
                                spec = ConfigurationSpecifier.IsShowIndex;
                                break;
                            case "TextFormat":
                                spec = ConfigurationSpecifier.TextFormat;
                                break;
                            case "FontFamily":
                                spec = ConfigurationSpecifier.FontFamily;
                                break;
                            case "FlashFontSize":
                                spec = ConfigurationSpecifier.FlashFontSize;
                                break;
                            case "ListFontSize":
                                spec = ConfigurationSpecifier.ListFontSize;
                                break;
                            case "MaximumLineLength":
                                spec = ConfigurationSpecifier.MaximumLineLength;
                                break;
                            case "Subs":
                                spec = ConfigurationSpecifier.Subs;
                                break;
                            case "Read":
                                spec = ConfigurationSpecifier.Read;
                                break;
                            case "Translate":
                            case "Trans":
                            case "Tran":
                                spec = ConfigurationSpecifier.Translate;
                                break;
                            case "Listen":
                                spec = ConfigurationSpecifier.Listen;
                                break;
                            case "See":
                                spec = ConfigurationSpecifier.See;
                                break;
                            case "Speak":
                                spec = ConfigurationSpecifier.Speak;
                                break;
                            case "Recognize":
                                spec = ConfigurationSpecifier.Recognize;
                                break;
                            case "Dictate":
                                spec = ConfigurationSpecifier.Dictate;
                                break;
                            case "Descramble":
                                spec = ConfigurationSpecifier.Descramble;
                                break;
                            case "Choice":
                                spec = ConfigurationSpecifier.Choice;
                                break;
                            case "Blanks":
                                spec = ConfigurationSpecifier.Blanks;
                                break;
                            case "Hybrid":
                                spec = ConfigurationSpecifier.Hybrid;
                                break;
                            case "Text":
                                spec = ConfigurationSpecifier.Text;
                                break;
                            case "Picture":
                                spec = ConfigurationSpecifier.Picture;
                                break;
                            case "Audio":
                                spec = ConfigurationSpecifier.Audio;
                                break;
                            case "Video":
                                spec = ConfigurationSpecifier.Video;
                                break;
                            case "Write":
                                spec = ConfigurationSpecifier.Write;
                                break;
                            case "T":
                                spec = ConfigurationSpecifier.T;
                                break;
                            case "TA":
                                spec = ConfigurationSpecifier.TA;
                                break;
                            case "AT":
                                spec = ConfigurationSpecifier.AT;
                                break;
                            case "TL":
                                spec = ConfigurationSpecifier.TL;
                                break;
                            case "TH":
                                spec = ConfigurationSpecifier.TH;
                                break;
                            case "TAH":
                                spec = ConfigurationSpecifier.TAH;
                                break;
                            case "H":
                                spec = ConfigurationSpecifier.H;
                                break;
                            case "HA":
                                spec = ConfigurationSpecifier.HA;
                                break;
                            case "AH":
                                spec = ConfigurationSpecifier.AH;
                                break;
                            case "HL":
                                spec = ConfigurationSpecifier.HL;
                                break;
                            case "HT":
                                spec = ConfigurationSpecifier.HT;
                                break;
                            case "HAT":
                                spec = ConfigurationSpecifier.HAT;
                                break;
                            default:
                                message = "Unknown specifier: " + tag;
                                return null;
                        }
                        token = new Token(spec, value);
                    }
                    break;
                case ':':
                    return new Token(ConfigurationSpecifier.Colon);
                case '+':
                    return new Token(ConfigurationSpecifier.Plus);
                case '/':
                    return new Token(ConfigurationSpecifier.CardSep);
                default:
                    {
                        int nameStartIndex = index;
                        string value = null;
                        while ((index < count) && Char.IsLetter(configurationSpecification[index]))
                            index++;
                        if (index == nameStartIndex)
                        {
                            message = "Unexpected token: " + configurationSpecification[index].ToString();
                            return null;
                        }
                        string tag = configurationSpecification.Substring(nameStartIndex, index - nameStartIndex);
                        outIndex = index;
                        ConfigurationSpecifier spec;
                        switch (tag)
                        {
                            case "Read":
                                spec = ConfigurationSpecifier.Read;
                                break;
                            case "Translate":
                            case "Trans":
                            case "Tran":
                                spec = ConfigurationSpecifier.Translate;
                                break;
                            case "Listen":
                                spec = ConfigurationSpecifier.Listen;
                                break;
                            case "See":
                                spec = ConfigurationSpecifier.See;
                                break;
                            case "Speak":
                                spec = ConfigurationSpecifier.Speak;
                                break;
                            case "Recognize":
                                spec = ConfigurationSpecifier.Recognize;
                                break;
                            case "Descramble":
                                spec = ConfigurationSpecifier.Descramble;
                                break;
                            case "Choice":
                                spec = ConfigurationSpecifier.Choice;
                                break;
                            case "Blanks":
                                spec = ConfigurationSpecifier.Blanks;
                                break;
                            case "Hybrid":
                                spec = ConfigurationSpecifier.Hybrid;
                                break;
                            case "Dictate":
                                spec = ConfigurationSpecifier.Dictate;
                                break;
                            case "Text":
                                spec = ConfigurationSpecifier.Text;
                                break;
                            case "Picture":
                                spec = ConfigurationSpecifier.Picture;
                                break;
                            case "Audio":
                                spec = ConfigurationSpecifier.Audio;
                                break;
                            case "Video":
                                spec = ConfigurationSpecifier.Video;
                                break;
                            case "Write":
                                spec = ConfigurationSpecifier.Write;
                                break;
                            default:
                                message = "Unknown specifier: " + tag;
                                return null;
                        }
                        token = new Token(spec, value);
                    }
                    break;
            }

            while ((outIndex < count) && Char.IsWhiteSpace(configurationSpecification[outIndex]))
                outIndex++;

            return token;
        }

        private bool ParseConfigurationSpecification(string configurationSpecification, List<Token> tokens, out string message)
        {
            int index;
            int outIndex;
            int count = configurationSpecification.Length;
            Token token;

            message = null;

            for (index = 0; index < count; )
            {
                if ((token = NextToken(configurationSpecification, index, out outIndex, out message)) != null)
                {
                    if (outIndex <= index)
                    {
                        message = "Parse error.  index didn't change.";
                        return false;
                    }

                    tokens.Add(token);
                    index = outIndex;
                }
                else
                    return false;
            }

            return true;
        }

        public LanguageID GetLanguageIDFromDescriptors(string name, List<LanguageDescriptor> languageDescriptors)
        {
            LanguageDescriptor languageDescriptor = languageDescriptors.FirstOrDefault(x => x.Used && (x.LanguageID != null) && (x.Name == name));

            if (languageDescriptor != null)
                return languageDescriptor.LanguageID;

            return null;
        }

        public string GetLanguageAbbreviation(LanguageID languageID)
        {
            string str;
            string prefix = null;
            string suffix = null;

            if (!String.IsNullOrEmpty(languageID.CultureCode))
                suffix = languageID.CultureCode.ToUpper();
            else if (!String.IsNullOrEmpty(languageID.ExtensionCode))
                suffix = languageID.ExtensionCode.ToUpper();

            if ((suffix != null) && (UserProfile.UILanguageID == LanguageLookup.English))
            {
                prefix = languageID.LanguageOnly;
                if (prefix.Length > 4)
                    prefix = prefix.Substring(0, 3);
                if (suffix.Length >= 3)
                    suffix = suffix.Substring(suffix.Length - 1);
                str = prefix + suffix;
            }
            else
            {
                str = S(languageID.Language);
                if (str.Length > 4)
                    str = str.Substring(0, 3);
            }

            return str;
        }

        public string GetLanguageAbbreviationList(string initial, List<LanguageID> languageIDs, bool backwards)
        {
            string str = initial;

            if (languageIDs != null)
            {
                if (backwards)
                {
                    for (int index = languageIDs.Count() - 1; index >= 0; index--)
                    {
                        LanguageID languageID = languageIDs[index];

                        if (str.Length != 0)
                            str += "+";

                        str += GetLanguageAbbreviation(languageID);
                    }
                }
                else
                {
                    foreach (LanguageID languageID in languageIDs)
                    {
                        if (str.Length != 0)
                            str += "+";

                        str += GetLanguageAbbreviation(languageID);
                    }
                }
            }

            return str;
        }

        public string GetLanguageAbbreviationList(List<LanguageID> languageIDs)
        {
            return GetLanguageAbbreviationList("", languageIDs, false);
        }

        public string GetLanguageToken(ToolProfile toolProfile, LanguageID languageID)
        {
            LanguageDescriptor languageDescriptor = UserProfile.LanguageDescriptors.FirstOrDefault(x => x.LanguageID == languageID);

            if (languageDescriptor == null)
            {
                string languageCode = languageID.LanguageCode;
                languageDescriptor = UserProfile.LanguageDescriptors.FirstOrDefault(x => x.LanguageID.LanguageCode == languageCode);
            }

            if (languageDescriptor != null)
                return languageDescriptor.TokenAbbreviation;

            return String.Empty;
        }

        public string GetLanguageTokenList(ToolProfile toolProfile, List<LanguageID> languageIDs, bool isMedia)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{");

            if (languageIDs != null)
            {
                LanguageID targetLanguageID = null;
                LanguageID targetAlternate1LanguageID = null;
                LanguageID targetAlternate2LanguageID = null;
                LanguageID targetAlternate3LanguageID = null;
                LanguageID hostLanguageID = null;
                List<LanguageID> targetLanguageIDs = new List<LanguageID>();
                List<LanguageID> alternateLanguageIDs = new List<LanguageID>();
                List<LanguageID> hostLanguageIDs = new List<LanguageID>();
                List<LanguageID> targetAlternativeLanguageIDs = new List<LanguageID>();
                List<LanguageID> alternativeTargetLanguageIDs = new List<LanguageID>();
                List<LanguageID> hostAlternativeLanguageIDs = new List<LanguageID>();
                List<LanguageID> alternativeHostLanguageIDs = new List<LanguageID>();
                List<LanguageID> targetHostLanguageIDs = new List<LanguageID>();
                List<LanguageID> hostTargetLanguageIDs = new List<LanguageID>();
                List<LanguageID> targetAlternativeHostLanguageIDs = new List<LanguageID>();
                List<LanguageID> hostAlternativeTargetLanguageIDs = new List<LanguageID>();
                string mediaChar = "";

                if ((toolProfile.TargetLanguageIDs != null) && (toolProfile.TargetLanguageIDs.Count() != 0))
                    targetLanguageID = toolProfile.TargetLanguageIDs[0];

                if ((toolProfile.TargetLanguageIDs != null) && (toolProfile.TargetLanguageIDs.Count() >= 2))
                    targetAlternate1LanguageID = toolProfile.TargetLanguageIDs[1];

                if ((toolProfile.TargetLanguageIDs != null) && (toolProfile.TargetLanguageIDs.Count() >= 3))
                    targetAlternate2LanguageID = toolProfile.TargetLanguageIDs[2];

                if ((toolProfile.TargetLanguageIDs != null) && (toolProfile.TargetLanguageIDs.Count() >= 4))
                    targetAlternate3LanguageID = toolProfile.TargetLanguageIDs[3];

                if ((toolProfile.HostLanguageIDs != null) && (toolProfile.HostLanguageIDs.Count() != 0))
                    hostLanguageID = toolProfile.HostLanguageIDs[0];

                if (hostLanguageID == null)
                    hostLanguageID = UILanguageID;

                if (targetLanguageID == null)
                    targetLanguageID = hostLanguageID;

                if (isMedia)
                {
                    if (targetLanguageID != null)
                        targetLanguageID = new LanguageID(targetLanguageID.LanguageCode);

                    if (targetAlternate1LanguageID != null)
                        targetAlternate1LanguageID = new LanguageID(targetAlternate1LanguageID.LanguageCode);

                    if (targetAlternate2LanguageID != null)
                        targetAlternate2LanguageID = new LanguageID(targetAlternate2LanguageID.LanguageCode);

                    if (targetAlternate3LanguageID != null)
                        targetAlternate3LanguageID = new LanguageID(targetAlternate3LanguageID.LanguageCode);

                    if (hostLanguageID != null)
                        hostLanguageID = new LanguageID(hostLanguageID.LanguageCode);

                    mediaChar = "L";
                }

                hostLanguageIDs.Add(hostLanguageID);
                hostAlternativeLanguageIDs.Add(hostLanguageID);
                alternativeHostLanguageIDs.Add(hostLanguageID);

                targetLanguageIDs.Add(targetLanguageID);
                targetAlternativeLanguageIDs.Add(targetLanguageID);
                alternativeTargetLanguageIDs.Add(targetLanguageID);

                if (targetAlternate1LanguageID != null)
                {
                    if (String.IsNullOrEmpty(targetAlternate1LanguageID.ExtensionCode))
                        targetLanguageIDs.Add(targetAlternate1LanguageID);
                    else
                    {
                        alternateLanguageIDs.Add(targetAlternate1LanguageID);
                        hostAlternativeLanguageIDs.Add(targetAlternate1LanguageID);
                        alternativeHostLanguageIDs.Insert(0, targetAlternate1LanguageID);
                    }
                    targetAlternativeLanguageIDs.Add(targetAlternate1LanguageID);
                    alternativeTargetLanguageIDs.Insert(0, targetAlternate1LanguageID);
                }

                if (targetAlternate2LanguageID != null)
                {
                    if (String.IsNullOrEmpty(targetAlternate2LanguageID.ExtensionCode))
                        targetLanguageIDs.Add(targetAlternate2LanguageID);
                    else
                    {
                        alternateLanguageIDs.Add(targetAlternate2LanguageID);
                        hostAlternativeLanguageIDs.Add(targetAlternate2LanguageID);
                        alternativeHostLanguageIDs.Insert(0, targetAlternate2LanguageID);
                    }
                    targetAlternativeLanguageIDs.Add(targetAlternate2LanguageID);
                    alternativeTargetLanguageIDs.Insert(0, targetAlternate2LanguageID);
                }

                if (targetAlternate3LanguageID != null)
                {
                    if (String.IsNullOrEmpty(targetAlternate3LanguageID.ExtensionCode))
                        targetLanguageIDs.Add(targetAlternate3LanguageID);
                    else
                    {
                        alternateLanguageIDs.Add(targetAlternate3LanguageID);
                        hostAlternativeLanguageIDs.Add(targetAlternate3LanguageID);
                        alternativeHostLanguageIDs.Insert(0, targetAlternate3LanguageID);
                    }
                    targetAlternativeLanguageIDs.Add(targetAlternate3LanguageID);
                    alternativeTargetLanguageIDs.Insert(0, targetAlternate3LanguageID);
                }

                targetHostLanguageIDs.AddRange(targetLanguageIDs);
                targetHostLanguageIDs.AddRange(hostLanguageIDs);

                targetAlternativeHostLanguageIDs.AddRange(targetAlternativeLanguageIDs);
                targetAlternativeHostLanguageIDs.AddRange(hostLanguageIDs);

                hostTargetLanguageIDs.AddRange(hostLanguageIDs);
                hostTargetLanguageIDs.AddRange(targetLanguageIDs);

                hostAlternativeTargetLanguageIDs.AddRange(hostAlternativeLanguageIDs);
                hostAlternativeTargetLanguageIDs.AddRange(targetLanguageIDs);

                if (LanguageID.CompareLanguageIDLists(languageIDs, targetLanguageIDs) == 0)
                    sb.Append("T" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, alternateLanguageIDs) == 0)
                    sb.Append("A" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, hostLanguageIDs) == 0)
                    sb.Append("H" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, targetAlternativeLanguageIDs) == 0)
                    sb.Append("T" + mediaChar + "A" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, alternativeTargetLanguageIDs) == 0)
                    sb.Append("A" + mediaChar + "T" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, hostAlternativeLanguageIDs) == 0)
                    sb.Append("H" + mediaChar + "A" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, alternativeHostLanguageIDs) == 0)
                    sb.Append("A" + mediaChar + "H" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, targetHostLanguageIDs) == 0)
                    sb.Append("T" + mediaChar + "H" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, hostTargetLanguageIDs) == 0)
                    sb.Append("H" + mediaChar + "T" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, targetAlternativeHostLanguageIDs) == 0)
                    sb.Append("T" + mediaChar + "A" + mediaChar + "H" + mediaChar);
                else if (LanguageID.CompareLanguageIDLists(languageIDs, hostAlternativeTargetLanguageIDs) == 0)
                    sb.Append("H" + mediaChar + "A" + mediaChar + "T" + mediaChar);
                else
                {
                    foreach (LanguageID languageID in languageIDs)
                    {
                        string tokenString = GetLanguageToken(toolProfile, languageID) + mediaChar;

                        if (!sb.ToString().Contains(tokenString))
                            sb.Append(tokenString);
                    }
                }
            }

            sb.Append("}");

            return sb.ToString();
        }

        public string GetToolConfigurationName(ToolConfiguration toolConfiguration)
        {
            string label = toolConfiguration.Label;
            StringBuilder sb = new StringBuilder();

            if (label != null)
            {
                sb.Append(label);
                sb.Append(" ");
            }

            if (toolConfiguration.SubConfigurationCount() != 0)
            {
                string subConfigurationsString = TextUtilities.GetStringFromStringList(toolConfiguration.SubConfigurationKeys);
                subConfigurationsString = subConfigurationsString.Replace(",", "+");
                sb.Append(subConfigurationsString);
                sb.Append(" ");
            }

            foreach (ToolSide toolSide in toolConfiguration.CardSides)
            {
                bool cont = false;

                if (toolSide.Side != 1)
                    sb.Append("/");

                if (toolSide.HasTextOutput)
                {
                    sb.Append(S("Text"));
                    sb.Append(":");
                    sb.Append(GetLanguageAbbreviationList("", toolSide.TextLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasPictureOutput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Picture"));
                    sb.Append(":");
                    sb.Append(GetLanguageAbbreviationList("", toolSide.MediaLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasAudioOutput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Audio"));
                    sb.Append(":");
                    sb.Append(GetLanguageAbbreviationList("", toolSide.MediaLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasVideoOutput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Video"));
                    sb.Append(":");
                    sb.Append(GetLanguageAbbreviationList("", toolSide.MediaLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasTextInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Write"));
                    string textLanguages = GetLanguageAbbreviationList("", toolSide.TextLanguageIDs, false);
                    string writeLanguages = GetLanguageAbbreviationList("", toolSide.WriteLanguageIDs, false);
                    if (writeLanguages != textLanguages)
                    {
                        sb.Append(":");
                        sb.Append(writeLanguages);
                    }
                    cont = true;
                }

                if (toolSide.HasAudioInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Speak"));
                    sb.Append(":");
                    sb.Append(GetLanguageAbbreviationList("", toolSide.MediaLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasVoiceRecognition)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Recognize"));
                    sb.Append(":");
                    sb.Append(GetLanguageAbbreviationList("", toolSide.MediaLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasDescrambleInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Descramble"));
                    string textLanguages = GetLanguageAbbreviationList("", toolSide.TextLanguageIDs, false);
                    string writeLanguages = GetLanguageAbbreviationList("", toolSide.WriteLanguageIDs, false);
                    if (writeLanguages != textLanguages)
                    {
                        sb.Append(":");
                        sb.Append(writeLanguages);
                    }
                    cont = true;
                }

                if (toolSide.HasChoiceInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Choice"));
                    string textLanguages = GetLanguageAbbreviationList("", toolSide.TextLanguageIDs, false);
                    string writeLanguages = GetLanguageAbbreviationList("", toolSide.WriteLanguageIDs, false);
                    if (writeLanguages != textLanguages)
                    {
                        sb.Append(":");
                        sb.Append(writeLanguages);
                    }
                    cont = true;
                }

                if (toolSide.HasBlanksInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Blanks"));
                    string textLanguages = GetLanguageAbbreviationList("", toolSide.TextLanguageIDs, false);
                    string writeLanguages = GetLanguageAbbreviationList("", toolSide.WriteLanguageIDs, false);
                    if (writeLanguages != textLanguages)
                    {
                        sb.Append(":");
                        sb.Append(writeLanguages);
                    }
                    cont = true;
                }
            }

            sb.Append(" ");
            sb.Append(S(ToolProfile.GetSelectorStringFromCodeString(toolConfiguration.SelectorAlgorithm.ToString())));

            return sb.ToString();
        }

        public void SetToolConfigurationMultiLanguageString(ToolConfiguration toolConfiguration, MultiLanguageString multiLanguageString)
        {
            string name = null;

            foreach (LanguageString languageString in multiLanguageString.LanguageStrings)
            {
                if (String.IsNullOrEmpty(languageString.Text))
                {
                    if (String.IsNullOrEmpty(name))
                        name = GetToolConfigurationName(toolConfiguration);

                    languageString.Text = name;
                }
            }
        }

        public void SetToolConfigurationDefaultInformation(ToolProfile toolProfile, ToolConfiguration toolConfiguration)
        {
            if ((toolProfile == null) || (toolConfiguration == null))
                return;

            toolConfiguration.Profile = toolProfile;

            if (String.IsNullOrEmpty(toolConfiguration.KeyString))
            {
                string keyBase = toolConfiguration.Label;
                int index = 0;
                string hintKey = String.Empty;
                do
                {
                    hintKey = keyBase + index.ToString();
                    index++;
                }
                while (toolProfile.ToolConfigurations.FirstOrDefault(x => (x != toolConfiguration) && x.MatchKey(hintKey)) != null);
                toolConfiguration.Key = hintKey;
            }

            if (String.IsNullOrEmpty(toolConfiguration.GetTitleString()))
                SetToolConfigurationMultiLanguageString(toolConfiguration, toolConfiguration.Title);

            if (String.IsNullOrEmpty(toolConfiguration.GetDescriptionString()))
                SetToolConfigurationMultiLanguageString(toolConfiguration, toolConfiguration.Description);

            toolConfiguration.SetDefaultProfileInformation();
        }

        public string GetToolConfigurationSpecifier(ToolConfiguration toolConfiguration)
        {
            string label = toolConfiguration.Label;
            StringBuilder sb = new StringBuilder();

            if (label != null)
            {
                sb.Append(label);
                sb.Append(" ");
            }

            foreach (ToolSide toolSide in toolConfiguration.CardSides)
            {
                ToolProfile toolProfile = toolConfiguration.Profile;
                bool cont = false;

                if (toolSide.Side != 1)
                    sb.Append("/");

                if (toolSide.HasTextOutput)
                {
                    sb.Append(S("Text"));
                    sb.Append(":");
                    sb.Append(GetLanguageTokenList(toolProfile, toolSide.TextLanguageIDs, false));
                    cont = true;
                }

                if (toolSide.HasPictureOutput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Picture"));
                    sb.Append(":");
                    sb.Append(GetLanguageTokenList(toolProfile, toolSide.MediaLanguageIDs, true));
                    cont = true;
                }

                if (toolSide.HasAudioOutput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Audio"));
                    sb.Append(":");
                    sb.Append(GetLanguageTokenList(toolProfile, toolSide.MediaLanguageIDs, true));
                    cont = true;
                }

                if (toolSide.HasVideoOutput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Video"));
                    sb.Append(":");
                    sb.Append(GetLanguageTokenList(toolProfile, toolSide.MediaLanguageIDs, true));
                    cont = true;
                }

                if (toolSide.HasTextInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Write"));
                    string textLanguages = GetLanguageTokenList(toolProfile, toolSide.TextLanguageIDs, false);
                    string writeLanguages = GetLanguageTokenList(toolProfile, toolSide.WriteLanguageIDs, false);
                    if (String.IsNullOrEmpty(writeLanguages))
                    {
                        sb.Append(":");
                        sb.Append(writeLanguages);
                    }
                    else if (String.IsNullOrEmpty(textLanguages))
                    {
                        sb.Append(":");
                        sb.Append(textLanguages);
                    }
                    cont = true;
                }

                if (toolSide.HasAudioInput)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Speak"));
                    sb.Append(":");
                    sb.Append(GetLanguageTokenList(toolProfile, toolSide.MediaLanguageIDs, true));
                    cont = true;
                }

                if (toolSide.HasVoiceRecognition)
                {
                    if (cont)
                        sb.Append("+");
                    sb.Append(S("Recognize"));
                    sb.Append(":");
                    sb.Append(GetLanguageTokenList(toolProfile, toolSide.MediaLanguageIDs, true));
                    cont = true;
                }
            }

            return sb.ToString();
        }

        public static List<string> ConfigurationLabels = new List<string>()
        {
            "Read",
            "Listen",
            "Dictate",
            "Write",
            "Trans",
            "Speak",
            "See",
            "Descramble",
            "Choice",
            "Blanks",
            "Hybrid",
            "(select)"
        };

        public string GetToolProfileSpecifier(ToolProfile toolProfile)
        {
            StringBuilder sb = new StringBuilder();

            if (toolProfile.ToolConfigurationCount() != 0)
            {
                foreach (ToolConfiguration toolConfiguration in toolProfile.ToolConfigurations)
                    sb.AppendLine(GetToolConfigurationSpecifier(toolConfiguration));
            }

            return sb.ToString();
        }

        public bool ParseToolConfiguration(ToolProfile toolProfile, string configurationSpecification,
            List<LanguageDescriptor> languageDescriptors, LanguageID uiLanguageID, string owner,
            out ToolConfiguration toolConfiguration, out string message)
        {
            string name = null;
            StringBuilder nameSB = new StringBuilder();
            string label = null;
            LanguageID targetLanguageID = ToolTargetLanguageID;
            LanguageID targetAlternate1LanguageID = ToolTargetAlternate1LanguageID;
            LanguageID targetAlternate2LanguageID = ToolTargetAlternate2LanguageID;
            LanguageID targetAlternate3LanguageID = ToolTargetAlternate3LanguageID;
            LanguageID hostLanguageID = ToolHostLanguageID;
            string targetLanguageCode = (targetLanguageID != null ? targetLanguageID.LanguageCode : "en");
            string hostLanguageCode = (hostLanguageID != null ? hostLanguageID.LanguageCode : "en");
            LanguageID targetMediaLanguageID = new LanguageID(targetLanguageCode);
            LanguageID hostMediaLanguageID = new LanguageID(hostLanguageCode);
            List<LanguageID> languageIDs = new List<LanguageID>();
            List<LanguageID> targetLanguageIDs = new List<LanguageID>();
            List<LanguageID> alternateLanguageIDs = new List<LanguageID>();
            List<LanguageID> hostLanguageIDs = new List<LanguageID>();
            List<LanguageID> targetAlternativeLanguageIDs = new List<LanguageID>();
            List<LanguageID> alternativeTargetLanguageIDs = new List<LanguageID>();
            List<LanguageID> hostAlternativeLanguageIDs = new List<LanguageID>();
            List<LanguageID> alternativeHostLanguageIDs = new List<LanguageID>();
            List<LanguageID> targetHostLanguageIDs = new List<LanguageID>();
            List<LanguageID> hostTargetLanguageIDs = new List<LanguageID>();
            List<LanguageID> targetAlternativeHostLanguageIDs = new List<LanguageID>();
            List<LanguageID> hostAlternativeTargetLanguageIDs = new List<LanguageID>();
            string targetRootName = (targetLanguageID != null ? targetLanguageCode : null);
            string hostRootName = (hostLanguageID != null ? hostLanguageCode : null);
            SelectorAlgorithmCode selectorAlgorithm = toolProfile.SelectorAlgorithm;
            string selectorAlgorithmName = toolProfile.SelectorAlgorithm.ToString();
            int reviewLevel = toolProfile.ReviewLevel;
            int chunkSize = toolProfile.ChunkSize;
            int historySize = toolProfile.HistorySize;
            bool isShowIndex = toolProfile.IsShowIndex;
            string profileLabel = "(" + S("profile") + ")";
            string fontFamily = profileLabel;
            string flashFontSize = profileLabel;
            string listFontSize = profileLabel;
            int maximumLineLength = toolProfile.MaximumLineLength;
            List<Token> tokens = new List<Token>(32);
            int sideNumber = 1;
            ToolSide toolSide = new ToolSide(sideNumber, toolProfile);
            ConfigurationSpecifier currentSpecifier = ConfigurationSpecifier.None;
            bool returnValue = true;

            toolConfiguration = new ToolConfiguration();
            toolConfiguration.Profile = toolProfile;
            message = "";

            if (!ParseConfigurationSpecification(configurationSpecification, tokens, out message))
                return false;

            if (hostLanguageID == null)
            {
                hostLanguageID = uiLanguageID;
                hostRootName = "en";
                hostMediaLanguageID = new LanguageID("en");
            }

            hostLanguageIDs.Add(hostLanguageID);
            hostAlternativeLanguageIDs.Add(hostLanguageID);
            alternativeHostLanguageIDs.Add(hostLanguageID);

            if (targetLanguageID == null)
            {
                targetLanguageID = hostLanguageID;
                targetRootName = hostRootName;
                targetMediaLanguageID = hostMediaLanguageID;
            }

            targetLanguageIDs.Add(targetLanguageID);
            targetAlternativeLanguageIDs.Add(targetLanguageID);
            alternativeTargetLanguageIDs.Add(targetLanguageID);
            languageIDs.Add(targetLanguageID);

            if (targetAlternate1LanguageID != null)
            {
                if (String.IsNullOrEmpty(targetAlternate1LanguageID.ExtensionCode))
                    targetLanguageIDs.Add(targetAlternate1LanguageID);
                else
                {
                    alternateLanguageIDs.Add(targetAlternate1LanguageID);
                    hostAlternativeLanguageIDs.Add(targetAlternate1LanguageID);
                    alternativeHostLanguageIDs.Insert(0, targetAlternate1LanguageID);
                }
                targetAlternativeLanguageIDs.Add(targetAlternate1LanguageID);
                alternativeTargetLanguageIDs.Insert(0, targetAlternate1LanguageID);
                languageIDs.Add(targetAlternate1LanguageID);
            }

            if (targetAlternate2LanguageID != null)
            {
                if (String.IsNullOrEmpty(targetAlternate2LanguageID.ExtensionCode))
                    targetLanguageIDs.Add(targetAlternate2LanguageID);
                else
                {
                    alternateLanguageIDs.Add(targetAlternate2LanguageID);
                    hostAlternativeLanguageIDs.Add(targetAlternate2LanguageID);
                    alternativeHostLanguageIDs.Insert(0, targetAlternate2LanguageID);
                }
                targetAlternativeLanguageIDs.Add(targetAlternate2LanguageID);
                alternativeTargetLanguageIDs.Insert(0, targetAlternate2LanguageID);
                languageIDs.Add(targetAlternate2LanguageID);
            }

            if (targetAlternate3LanguageID != null)
            {
                if (String.IsNullOrEmpty(targetAlternate3LanguageID.ExtensionCode))
                    targetLanguageIDs.Add(targetAlternate3LanguageID);
                else
                {
                    alternateLanguageIDs.Add(targetAlternate3LanguageID);
                    hostAlternativeLanguageIDs.Add(targetAlternate3LanguageID);
                    alternativeHostLanguageIDs.Insert(0, targetAlternate3LanguageID);
                }
                targetAlternativeLanguageIDs.Add(targetAlternate3LanguageID);
                alternativeTargetLanguageIDs.Insert(0, targetAlternate3LanguageID);
                languageIDs.Add(targetAlternate3LanguageID);
            }

            targetHostLanguageIDs.AddRange(targetLanguageIDs);
            targetHostLanguageIDs.AddRange(hostLanguageIDs);

            targetAlternativeHostLanguageIDs.AddRange(targetAlternativeLanguageIDs);
            targetAlternativeHostLanguageIDs.AddRange(hostLanguageIDs);

            hostTargetLanguageIDs.AddRange(hostLanguageIDs);
            hostTargetLanguageIDs.AddRange(targetLanguageIDs);

            hostAlternativeTargetLanguageIDs.AddRange(hostAlternativeLanguageIDs);
            hostAlternativeTargetLanguageIDs.AddRange(targetLanguageIDs);

            languageIDs.Add(hostLanguageID);

            toolConfiguration.HostLanguageIDs = hostLanguageIDs;
            toolConfiguration.TargetLanguageIDs = targetLanguageIDs;

            toolSide.TextFormat = null;
            toolSide.FontFamily = fontFamily;
            toolSide.FlashFontSize = flashFontSize;
            toolSide.ListFontSize = listFontSize;
            toolSide.MaximumLineLength = maximumLineLength;

            toolConfiguration.AddCardSide(toolSide);

            foreach (Token token in tokens)
            {
                switch (token.Specifier)
                {
                    case ConfigurationSpecifier.Name:
                        name = token.Value;
                        break;
                    case ConfigurationSpecifier.Select:
                        selectorAlgorithm = ToolProfile.GetSelectorAlgorithmCodeFromString(token.Value);
                        selectorAlgorithmName = selectorAlgorithm.ToString();
                        break;
                    case ConfigurationSpecifier.Level:
                        reviewLevel = Convert.ToInt32(token.Value);
                        break;
                    case ConfigurationSpecifier.ChunkSize:
                        chunkSize = Convert.ToInt32(token.Value);
                        break;
                    case ConfigurationSpecifier.HistorySize:
                        historySize = Convert.ToInt32(token.Value);
                        break;
                    case ConfigurationSpecifier.IsShowIndex:
                        isShowIndex = (token.Value == "false" ? false : true);
                        break;
                    case ConfigurationSpecifier.TextFormat:
                        toolSide.TextFormat = token.Value;
                        break;
                    case ConfigurationSpecifier.FontFamily:
                        toolSide.FontFamily = token.Value;
                        break;
                    case ConfigurationSpecifier.FlashFontSize:
                        toolSide.FlashFontSize = token.Value;
                        break;
                    case ConfigurationSpecifier.ListFontSize:
                        toolSide.ListFontSize = token.Value;
                        break;
                    case ConfigurationSpecifier.MaximumLineLength:
                        toolSide.MaximumLineLength = Convert.ToInt32(token.Value);
                        break;
                    case ConfigurationSpecifier.Subs:
                        toolConfiguration.SubConfigurationKeys = TextUtilities.GetStringListFromString(token.Value);
                        break;
                    case ConfigurationSpecifier.Read:
                        label = S(token.Specifier.ToString());
                        break;
                    case ConfigurationSpecifier.Translate:
                        label = (uiLanguageID.LanguageCode == "en" ? "Trans" : S(token.Specifier.ToString()));
                        break;
                    case ConfigurationSpecifier.Listen:
                        label = S(token.Specifier.ToString());
                        break;
                    case ConfigurationSpecifier.See:
                        label = S(token.Specifier.ToString());
                        break;
                    case ConfigurationSpecifier.Text:
                        toolSide.HasTextOutput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Picture:
                        toolSide.HasPictureOutput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Audio:
                        toolSide.HasAudioOutput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Video:
                        toolSide.HasVideoOutput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Speak:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasAudioInput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Recognize:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasVoiceRecognition = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Write:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasTextInput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Dictate:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasTextInput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Descramble:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasDescrambleInput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Choice:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasChoiceInput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Blanks:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        else
                            toolSide.HasBlanksInput = true;
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.Hybrid:
                        if (label == null)
                            label = S(token.Specifier.ToString());
                        currentSpecifier = token.Specifier;
                        break;
                    case ConfigurationSpecifier.T:
                        SetSpecifierLanguages(toolSide, currentSpecifier, targetLanguageIDs);
                        break;
                    case ConfigurationSpecifier.TA:
                        SetSpecifierLanguages(toolSide, currentSpecifier, targetAlternativeLanguageIDs);
                        break;
                    case ConfigurationSpecifier.AT:
                        SetSpecifierLanguages(toolSide, currentSpecifier, alternativeTargetLanguageIDs);
                        break;
                    case ConfigurationSpecifier.TL:
                        SetSpecifierLanguages(toolSide, currentSpecifier, new List<LanguageID>() { targetMediaLanguageID });
                        break;
                    case ConfigurationSpecifier.TH:
                        SetSpecifierLanguages(toolSide, currentSpecifier, targetHostLanguageIDs);
                        break;
                    case ConfigurationSpecifier.TAH:
                        SetSpecifierLanguages(toolSide, currentSpecifier, targetAlternativeHostLanguageIDs);
                        break;
                    case ConfigurationSpecifier.H:
                        SetSpecifierLanguages(toolSide, currentSpecifier, hostLanguageIDs);
                        break;
                    case ConfigurationSpecifier.HA:
                        SetSpecifierLanguages(toolSide, currentSpecifier, hostAlternativeLanguageIDs);
                        break;
                    case ConfigurationSpecifier.AH:
                        SetSpecifierLanguages(toolSide, currentSpecifier, alternativeHostLanguageIDs);
                        break;
                    case ConfigurationSpecifier.HL:
                        SetSpecifierLanguages(toolSide, currentSpecifier, new List<LanguageID>() { hostMediaLanguageID });
                        break;
                    case ConfigurationSpecifier.HT:
                        SetSpecifierLanguages(toolSide, currentSpecifier, hostTargetLanguageIDs);
                        break;
                    case ConfigurationSpecifier.HAT:
                        SetSpecifierLanguages(toolSide, currentSpecifier, hostAlternativeTargetLanguageIDs);
                        break;
                    case ConfigurationSpecifier.Colon:
                        break;
                    case ConfigurationSpecifier.Plus:
                        break;
                    case ConfigurationSpecifier.CardSep:
                        currentSpecifier = ConfigurationSpecifier.None;
                        toolSide = new ToolSide(++sideNumber, toolProfile);
                        toolSide.TextFormat = null;
                        toolSide.FontFamily = fontFamily;
                        toolSide.FlashFontSize = flashFontSize;
                        toolSide.ListFontSize = listFontSize;
                        toolSide.MaximumLineLength = maximumLineLength;
                        toolConfiguration.AddCardSide(toolSide);
                        break;
                    default:
                        message = "Unknown token: " + token.Specifier.ToString();
                        return false;
                }
            }

            toolConfiguration.Label = label;

            if (name == null)
                name = GetToolConfigurationName(toolConfiguration);
            else
            {
                name.Replace("{T}", GetLanguageAbbreviationList(targetLanguageIDs));
                name.Replace("{TA}", GetLanguageAbbreviationList(targetAlternativeLanguageIDs));
                name.Replace("{AT}", GetLanguageAbbreviationList(alternativeTargetLanguageIDs));
                name.Replace("{TL}", targetMediaLanguageID.LanguageCode);
                name.Replace("{TH}", GetLanguageAbbreviationList(targetHostLanguageIDs));
                name.Replace("{TAH}", GetLanguageAbbreviationList(targetAlternativeHostLanguageIDs));
                name.Replace("{H}", GetLanguageAbbreviationList(hostLanguageIDs));
                name.Replace("{HA}", GetLanguageAbbreviationList(hostAlternativeLanguageIDs));
                name.Replace("{AH}", GetLanguageAbbreviationList(alternativeHostLanguageIDs));
                name.Replace("{HL}", hostMediaLanguageID.LanguageCode);
                name.Replace("{HT}", GetLanguageAbbreviationList(hostTargetLanguageIDs));
                name.Replace("{HAT}", GetLanguageAbbreviationList(hostAlternativeTargetLanguageIDs));
            }

            toolConfiguration.Title = new MultiLanguageString("Title", uiLanguageID, name);
            toolConfiguration.Description = new MultiLanguageString("Description", uiLanguageID, GetToolConfigurationName(toolConfiguration));
            toolConfiguration.Owner = owner;
            toolConfiguration.Modified = false;

            return returnValue;
        }

        private void SetSpecifierLanguages(ToolSide toolSide, ConfigurationSpecifier currentSpecifier, List<LanguageID> currentLanguageIDs)
        {
            switch (currentSpecifier)
            {
                case ConfigurationSpecifier.Text:
                    toolSide.TextLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Picture:
                    toolSide.MediaLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Audio:
                    toolSide.MediaLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Video:
                    toolSide.MediaLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Speak:
                    toolSide.MediaLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Recognize:
                    toolSide.MediaLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Write:
                    toolSide.WriteLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Descramble:
                    toolSide.WriteLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Choice:
                    toolSide.WriteLanguageIDs = currentLanguageIDs;
                    break;
                case ConfigurationSpecifier.Blanks:
                    toolSide.WriteLanguageIDs = currentLanguageIDs;
                    break;
                default:
                    break;
            }
        }

        public bool AddToolProfileConfigurations(ToolProfile toolProfile, string configurationSpecifications, out string message)
        {
            message = "";

            if (String.IsNullOrEmpty(configurationSpecifications))
            {
                message = "No configurations specified.";
                return false;
            }

            string[] specs = configurationSpecifications.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ToolConfiguration toolConfiguration;
            List<ToolConfiguration> configurations = new List<ToolConfiguration>();
            List<LanguageDescriptor> languageDescriptors = toolProfile.LanguageDescriptors;

            foreach (string configurationSpecification in specs)
            {
                if (!ParseToolConfiguration(toolProfile, configurationSpecification, languageDescriptors, UserProfile.UILanguageID,
                        toolProfile.Owner, out toolConfiguration, out message))
                    return false;

                toolConfiguration.Key = toolConfiguration.Label + toolProfile.ToolConfigurationLabelCount(toolConfiguration.Label).ToString();
                toolConfiguration.Index = configurations.Count();
                configurations.Add(toolConfiguration);
                toolConfiguration.TouchAndClearModified();
                toolProfile.AddToolConfiguration(toolConfiguration);
            }

            return true;
        }

        public static string DefaultToolConfigurationSpecifications =
            "Read Text:{T}/Text:{AH}+Audio:{TL}+Picture:{TL}\r\n" +
            "Read Text:{T}/Text:{AH}\r\n" +
            "Listen Audio:{TL}/Text:{TAH}\r\n" +
            "Listen Audio:{TL}/Text:{TAH}+Audio:{HL}\r\n" +
            "Speak Text:{T}/Text:{AH}+Speak:{TL}\r\n" +
            "Speak Text:{T}/Text:{AH}+Recognize:{TL}\r\n" +
            "Write Text:{T}/Text:{AH}+Write:{H}\r\n" +
            "Write Text:{TAH}/Text:{T}+Write:{T}\r\n" +
            "Dictate Audio:{TL}/Text:{TAH}+Write:{T}\r\n" +
            "Translate Text:{H}+Picture:{HL}:{HL}/Text:{TA}+Audio:{TL}\r\n" +
            "Translate Text:{H}/Text:{TA}\r\n" +
            "Translate Text:{H}/Text:{TA}+Speak:{TL}\r\n" +
            "Translate Text:{H}/Text:{TA}+Recognize:{TL}\r\n" +
            "Translate Text:{H}/Text:{TA}+Write:{T}+Audio:{TL}\r\n" +
            "Translate Text:{H}/Text:{TA}+Write:{T}\r\n" +
            "Translate Audio:{HL}/Text:{TAH}+Write:{T}\r\n" +
            "Translate Audio:{HL}/Text:{TAH}+Audio:{TL}\r\n" +
            "See Picture:{TL}/Text:{TAH}+Audio:{TL}\r\n" +
            "Descramble Text:{T}/Text:{AH}+Descramble:{H}\r\n" +
            "Descramble Text:{T}/Text:{AH}+Descramble:{H}\r\n" +
            "Descramble Text:{H}/Text:{TA}+Descramble:{T}\r\n" +
            "Descramble Text:{H}/Text:{TA}+Descramble:{T}\r\n" +
            "Choice Text:{T}/Text:{AH}+Choice:{H}\r\n" +
            "Choice Text:{T}/Text:{AH}+Audio:{TL}+Picture:{TL}+Choice:{H}\r\n" +
            "Choice Text:{H}/Text:{TA}+Choice:{T}\r\n" +
            "Choice Text:{H}/Text:{TA}+Audio:{TL}+Choice:{T}\r\n" +
            "Blanks Text:{T}/Text:{AH}+Blanks:{H}\r\n" +
            "Blanks Text:{T}/Text:{AH}+Audio:{TL}+Picture:{TL}+Blanks:{H}\r\n" +
            "Blanks Text:{H}/Text:{TA}+Blanks:{T}\r\n" +
            "Blanks Text:{H}/Text:{TA}+Audio:{TL}+Blanks:{T}\r\n" +
            "Hybrid {Name:Hybrid no media no write} {Subs:Read0,Trans0,Choice0,Choice2,Descramble0,Descramble2}\r\n" +
            "Hybrid {Name:Hybrid with media no write} {Subs:Read1,Listen0,Trans1,Trans3,Choice1,Choice3,Descramble1,Descramble3}\r\n" +
            "Hybrid {Name:Hybrid no media with write} {Subs:Read0,Dictate0,Trans0,Trans4,Choice0,Choice2,Descramble0,Descramble2}\r\n" +
            "Hybrid {Name:Hybrid with media with write} {Subs:Read1,Dictate0,Listen0,Trans1,Trans3,Trans4,Choice1,Choice3,Descramble1,Descramble3}\r\n" +
            "Listen {Name:HandsFree T H T} Text:{T}+Audio:{TL}/Text:{AH}+Audio:{HL}/Text:{T}+Audio:{TL}\r\n" +
            "Listen {Name:HandsFree H T H} Text:{H}+Audio:{HL}/Text:{T}+Audio:{TL}/Text:{HA}+Audio:{HL}\r\n";

        public static string DefaultGenerateConfigurationSpecifications =
            "Read Text:{T}/Text:{AH}\r\n";

        public bool SetToolDefaultToolConfigurations(ToolProfile toolProfile, out string message)
        {
            if (toolProfile.ToolConfigurationCount() != 0)
                toolProfile.DeleteAllToolConfigurations();

            string specs = UserProfile.GetUserOptionString("DefaultToolConfigurations", DefaultToolConfigurationSpecifications);

            return AddToolProfileConfigurations(toolProfile, specs, out message);
        }

        public ToolProfile CreateDefaultToolProfile(string profileKey, string defaultConfigurationSpecs,
            out string message)
        {
            string title = "Default";
            string description = "Default tool profile.";
            ToolProfile toolProfile = new ToolProfile(
                profileKey,                                                                                         // string key,
                CreateUIMultiLanguageString("Title", title),                                                        // MultiLanguageString title,
                CreateUIMultiLanguageString("Description", description),                                            // MultiLanguageString description,
                "ToolProfiles",                                                                                     // string source,
                null,                                                                                               // string package,
                null,                                                                                               // string label,
                null,                                                                                               // string imageFileName,
                0,                                                                                                  // int index,
                true,                                                                                               // bool isPublic,
                UserProfile.TargetLanguageIDs,                                                                      // List<LanguageID> targetLanguageIDs,
                UserProfile.HostLanguageIDs,                                                                        // List<LanguageID> hostLanguageIDs,
                UserName,                                                                                           // string owner,
                ToolProfile.DefaultGradeCount,                                                                      // int gradeCount,
                ToolProfile.DefaultSelectorAlgorithm,                                                               // SelectorAlgorithmCode selectorAlgorithm,
                ToolProfile.DefaultNewLimit,                                                                        // int newLimit,
                ToolProfile.DefaultReviewLimit,                                                                     // int review,
                ToolProfile.DefaultIsRandomUnique,                                                                  // bool isRandomUnique,
                ToolProfile.DefaultIsRandomNew,                                                                     // bool isRandomNew,
                ToolProfile.DefaultIsAdaptiveMixNew,                                                                // bool isAdaptiveMixNew,
                ToolProfile.DefaultReviewLevel,                                                                     // int reviewLevel,
                ToolProfile.DefaultChoiceSize,                                                                      // int selectSize,
                ToolProfile.DefaultChunkSize,                                                                       // int chunkSize,
                ToolProfile.DefaultHistorySize,                                                                     // int historySize,
                ToolProfile.DefaultIsShowIndex,                                                                     // bool isShowIndex,
                ToolProfile.DefaultIsShowOrdinal,                                                                   // bool isShowOrdinal,
                ToolProfile.DefaultSpacedIntervalTable,                                                             // List<TimeSpan> intervalTable,
                UserProfile.GetUserOptionString("DefaultToolFontFamily", "(settings)"),                             // string fontFamily,
                UserProfile.GetUserOptionString("DefaultToolFlashFontSize", "(settings)"),                          // string flashFontSize,
                UserProfile.GetUserOptionString("DefaultToolListFontSize", "(settings)"),                           // string listFontSize,
                ToolProfile.DefaultMaximumLineLength,                                                               // int maximumLineLength,
                new List<ToolConfiguration>());                                                                     // List<ToolConfiguration> toolConfigurations);
            AddToolProfileConfigurations(toolProfile, defaultConfigurationSpecs, out message);
            return toolProfile;
        }

        public ToolProfile CreateToolProfile(string name, int index, out string message)
        {
            string toolProfileKey = ComposeToolProfileKey(UserRecord, name);
            string title = name;
            string description = name + " tool profile.";
            string specs = UserProfile.GetUserOptionString("DefaultToolConfigurations", DefaultToolConfigurationSpecifications);
            ToolProfile toolProfile = new ToolProfile(
                ToolUtilities.ComposeToolProfileKey(UserRecord, name),                                              // string key,
                CreateUIMultiLanguageString("Title", title),                                                        // MultiLanguageString title,
                CreateUIMultiLanguageString("Description", description),                                            // MultiLanguageString description,
                "ToolProfiles",                                                                                     // string source,
                null,                                                                                               // string package,
                null,                                                                                               // string label,
                null,                                                                                               // string imageFileName,
                index,                                                                                              // int index,
                true,                                                                                               // bool isPublic,
                UserProfile.TargetLanguageIDs,                                                                      // List<LanguageID> targetLanguageIDs,
                UserProfile.HostLanguageIDs,                                                                        // List<LanguageID> hostLanguageIDs,
                UserName,                                                                                           // string owner,
                ToolProfile.DefaultGradeCount,                                                                      // int gradeCount,
                ToolProfile.DefaultSelectorAlgorithm,                                                               // SelectorAlgorithmCode selectorAlgorithm,
                ToolProfile.DefaultNewLimit,                                                                        // int newLimit,
                ToolProfile.DefaultReviewLimit,                                                                     // int reviewLimit,
                ToolProfile.DefaultIsRandomUnique,                                                                  // bool isRandomUnique,
                ToolProfile.DefaultIsRandomNew,                                                                     // bool isRandomNew,
                ToolProfile.DefaultIsAdaptiveMixNew,                                                                // bool isAdaptiveMixNew,
                ToolProfile.DefaultReviewLevel,                                                                     // int reviewLevel,
                ToolProfile.DefaultChoiceSize,                                                                      // int selectSize,
                ToolProfile.DefaultChunkSize,                                                                       // int chunkSize,
                ToolProfile.DefaultHistorySize,                                                                     // int historySize,
                ToolProfile.DefaultIsShowIndex,                                                                     // bool isShowIndex,
                ToolProfile.DefaultIsShowOrdinal,                                                                   // bool isShowOrdinal,
                ToolProfile.DefaultSpacedIntervalTable,                                                             // List<TimeSpan> intervalTable,
                UserProfile.GetUserOptionString("DefaultToolFontFamily", "(settings)"),                             // string fontFamily,
                UserProfile.GetUserOptionString("DefaultToolFlashFontSize", "(settings)"),                          // string flashFontSize,
                UserProfile.GetUserOptionString("DefaultToolListFontSize", "(settings)"),                           // string listFontSize,
                ToolProfile.DefaultMaximumLineLength,                                                               // int maximumLineLength,
                new List<ToolConfiguration>());                                                                     // List<ToolConfiguration> toolConfigurations);
            AddToolProfileConfigurations(toolProfile, specs, out message);
            return toolProfile;
        }

        public ToolProfile CreateToolProfileFromDefault(string name, int index, out string message)
        {
            ToolProfile toolProfile = null;
            ToolProfile defaultProfile = Repositories.ToolProfiles.Get(ComposeToolProfileDefaultKey(UserRecord));
            if (defaultProfile != null)
            {
                if (name == "NewProfile")
                    name = defaultProfile.SelectorAlgorithmString;
                string title = name;
                string description = name + " tool profile.";
                toolProfile = new ToolProfile(defaultProfile, ToolUtilities.ComposeToolProfileKey(UserRecord, name));
                toolProfile.Title = CreateUIMultiLanguageString("Title", title);
                toolProfile.Description = CreateUIMultiLanguageString("Description", description);
                toolProfile.Index = index;
                message = null;
            }
            else
            {
                if (name == "NewProfile")
                    name = ToolProfile.GetSelectorStringFromCodeString(ToolProfile.DefaultSelectorAlgorithm.ToString());
                toolProfile = CreateToolProfile(name, index, out message);
            }
            return toolProfile;
        }

        public ToolProfile CreateToolProfile(string profileName, int index, string copyProfileKey, ToolProfile profileData)
        {
            ToolProfile profile = null;
            if (profileData == null)
            {
                string specs = UserProfile.GetUserOptionString("DefaultToolConfigurations", DefaultToolConfigurationSpecifications);
                profileData = CreateDefaultToolProfile(ComposeToolProfileDefaultKey(UserRecord), specs, out Error);
            }
            switch (copyProfileKey)
            {
                case "(Use profile default)":
                    profile = Repositories.ToolProfiles.Get(ComposeToolProfileDefaultKey(UserRecord));
                    if (profile != null)
                    {
                        profile = new ToolProfile(profile, ToolUtilities.ComposeToolProfileKey(UserRecord, profileName));
                        if (profileData != null)
                        {
                            profile.Title = new MultiLanguageString(profileData.Title);
                            profile.Description = new MultiLanguageString(profileData.Description);
                        }
                        profile.Index = index;
                    }
                    else
                        profile = CreateToolProfile(profileName, index, out Error);
                    break;
                case "(Don't copy profile)":
                    profile = new ToolProfile(profileData, ComposeToolProfileKey(UserRecord, profileName));
                    profile.Index = index;
                    break;
                case null:
                case "":
                    profile = new ToolProfile(profileData, ToolUtilities.ComposeToolProfileKey(UserRecord, profileName));
                    profile.Index = index;
                    break;
                default:
                    profile = Repositories.ToolProfiles.Get(copyProfileKey);
                    if (profile != null)
                    {
                        profile = new ToolProfile(profile, ToolUtilities.ComposeToolProfileKey(UserRecord, profileName));
                        if (profileData != null)
                        {
                            profile.Title = new MultiLanguageString(profileData.Title);
                            profile.Description = new MultiLanguageString(profileData.Description);
                        }
                        profile.Index = index;
                    }
                    else
                        profile = CreateToolProfile(profileName, index, out Error);
                    break;
            }
            if (profile.ToolConfigurationCount() == 0)
            {
                UpdateLanguages(null, profile);
                string specs = UserProfile.GetUserOptionString("DefaultToolConfigurations", DefaultToolConfigurationSpecifications);
                AddToolProfileConfigurations(profile, specs, out Error);
            }
            return profile;
        }

        public List<ToolProfile> GetToolProfiles()
        {
            return Repositories.ToolProfiles.GetList(UserRecord);
        }

        public ToolProfile GetToolProfile(string name, out string message)
        {
            string toolProfileKey = ComposeToolProfileKey(UserRecord, name);
            ToolProfile toolProfile = Repositories.ToolProfiles.GetNamed(UserRecord, name);

            message = String.Empty;

            if (toolProfile == null)
                message = S("Can't find profile") + ": " + name;

            return toolProfile;
        }

        public ToolProfile GetOrCreateToolProfile(string name, int index, out string message)
        {
            string toolProfileKey = ComposeToolProfileKey(UserRecord, name);
            ToolProfile toolProfile = Repositories.ToolProfiles.GetNamed(UserRecord, name);

            message = String.Empty;

            if (toolProfile == null)
            {
                toolProfile = CreateToolProfileFromDefault(name, index, out message);

                if (toolProfile != null)
                    AddToolProfile(toolProfile, out message);
            }

            return toolProfile;
        }

        public ToolProfile CreateAndAddToolProfile(string name, int index, ToolProfile profileData, out string message)
        {
            string toolProfileKey = ComposeToolProfileKey(UserRecord, name);
            ToolProfile toolProfile = CreateToolProfile(name, index, null, profileData);

            message = String.Empty;

            if (toolProfile != null)
                AddToolProfile(toolProfile, out message);

            return toolProfile;
        }

        public bool AddToolProfile(ToolProfile toolProfile, out string message)
        {
            bool returnValue = false;

            message = String.Empty;

            toolProfile.TouchAndClearModified();

            try
            {
                if (Repositories.ToolProfiles.Add(toolProfile))
                    returnValue = UpdateToolSessionProfiles(out message);
                else
                    message = S("Error adding tool profile: ") + toolProfile.Name;
            }
            catch (Exception exception)
            {
                message = S("Exception adding tool profile: ") + exception.Message;

                if (exception.InnerException != null)
                    message += ": " + exception.InnerException.Message;
            }

            return returnValue;
        }

        public bool UpdateToolProfile(ToolProfile toolProfile, out string message)
        {
            bool returnValue = false;

            message = String.Empty;

            toolProfile.TouchAndClearModified();

            try
            {
                if (Repositories.ToolProfiles.Update(toolProfile))
                    returnValue = UpdateToolSessionProfiles(out message);
                else
                    message = S("Error updating tool profile: ") + toolProfile.Name;
            }
            catch (Exception exception)
            {
                message = S("Exception updating tool profile: ") + exception.Message;

                if (exception.InnerException != null)
                    message += ": " + exception.InnerException.Message;
            }

            return returnValue;
        }

        public List<ToolProfile> MoveToolProfile(List<ToolProfile> toolProfiles, ToolProfile toolProfile, string direction, out string message)
        {
            message = String.Empty;

            int index;
            int count = toolProfiles.Count();

            for (index = 0; index < count; index++)
            {
                if (toolProfiles[index].Name == toolProfile.Name)
                {
                    toolProfile = toolProfiles[index];
                    break;
                }
            }

            if (index == count)
                return toolProfiles;

            if (direction == "up")
            {
                if (index > 0)
                {
                    toolProfiles.Remove(toolProfile);
                    toolProfiles.Insert(index - 1, toolProfile);
                }
                else
                    return toolProfiles;
            }
            else
            {
                if (index < count - 1)
                {
                    toolProfiles.Remove(toolProfile);
                    toolProfiles.Insert(index + 1, toolProfile);
                }
                else
                    return toolProfiles;
            }

            for (index = 0; index < count; index++)
            {
                if (toolProfiles[index].Index != index)
                {
                    toolProfile = toolProfiles[index];
                    toolProfile.Index = index;

                    UpdateToolProfile(toolProfile, out message);
                }
            }

            return toolProfiles;
        }

        public bool DeleteToolProfile(ToolProfile toolProfile, out string message)
        {
            bool returnValue = false;

            message = String.Empty;

            try
            {
                if (Repositories.ToolProfiles.Delete(toolProfile))
                    returnValue = UpdateToolSessionProfiles(out message);
                else
                    message = S("Error deleting tool profile: ") + toolProfile.Name;
            }
            catch (Exception exception)
            {
                message = S("Error deleting tool profile: ") + exception.Message;

                if (exception.InnerException != null)
                    message += ": " + exception.InnerException.Message;
            }

            return returnValue;
        }

        protected bool UpdateToolSession(ToolSession toolSession, out string message)
        {
            message = null;

            if (toolSession != null)
            {
                try
                {
                    toolSession.TouchAndClearModified();

                    if (Repositories.ToolSessions.Update(toolSession))
                        return true;
                    else
                        message = S("Error updating tool session.");
                }
                catch (Exception exception)
                {
                    message = S("Error updating tool session") + ": " + exception.Message;

                    if (exception.InnerException != null)
                        message += ": " + exception.InnerException.Message;
                }
            }

            return false;
        }

        public bool UpdateToolSessionProfiles(out string message)
        {
            List<ToolProfile> toolProfiles = GetToolProfiles();
            bool returnValue = true;

            message = null;

            for (int index = 0; index < ToolSession.MaxSessions; index++)
            {
                string toolSessionKey = ComposeToolSessionKey(UserRecord, index);
                ToolSession toolSession = Repositories.ToolSessions.Get(toolSessionKey);

                if (toolSession != null)
                {
                    toolSession.ToolProfiles = toolProfiles;

                    if (!UpdateToolSession(toolSession, out message))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public ToolConfiguration CreateToolConfiguration(ToolProfile toolProfile, string name, string label, int index)
        {
            string toolProfileKey = ComposeToolProfileKey(UserRecord, name);
            string title = (!String.IsNullOrEmpty(name) ? name : String.Empty);
            List<ToolSide> cardSides = new List<ToolSide>();
            ToolConfiguration toolConfiguration = new ToolConfiguration(
                toolProfile,                                                                            // ToolProfile toolProfile,
                name,                                                                                   // string key,
                CreateUIMultiLanguageString("Title", title),                                            // MultiLanguageString title,
                CreateUIMultiLanguageString("Description", title),                                      // MultiLanguageString description,
                label,                                                                                  // string label,
                index,                                                                                  // int index,
                cardSides);                                                                             // List<ToolSide> cardSides
            toolConfiguration.SetCardSideCount(2);
            toolConfiguration.SetDefaultProfileInformation();
            toolConfiguration.TouchAndClearModified();
            return toolConfiguration;
        }

        public bool MoveToolConfiguration(ToolProfile toolProfile, ToolConfiguration toolConfiguration,
            string direction, out string message)
        {
            bool returnValue = false;
            message = String.Empty;

            List<ToolConfiguration> toolConfigurations = toolProfile.ToolConfigurations;
            int index;
            int count = toolProfile.ToolConfigurationCount();

            for (index = 0; index < count; index++)
            {
                if (toolConfigurations[index].Name == toolConfiguration.Name)
                    break;
            }

            if (index == count)
            {
                message = S("Tool configuration not found.");
                return false;
            }

            if (direction == "up")
            {
                if (index > 0)
                {
                    toolConfigurations.Remove(toolConfiguration);
                    toolConfigurations.Insert(index - 1, toolConfiguration);
                    returnValue = true;
                }
                else
                    message = S("Already at beginning.");
            }
            else
            {
                if (index < count - 1)
                {
                    toolConfigurations.Remove(toolConfiguration);
                    toolConfigurations.Insert(index + 1, toolConfiguration);
                    returnValue = true;
                }
                else
                    message = S("Already at end.");
            }

            for (index = 0; index < count; index++)
            {
                if (toolConfigurations[index].Index != index)
                {
                    toolConfiguration = toolConfigurations[index];
                    toolConfiguration.Index = index;
                    toolConfiguration.TouchAndClearModified();
                }
            }

            if (returnValue)
                returnValue = UpdateToolProfile(toolProfile, out message);

            return returnValue;
        }

        public static ToolItemStatusCode GetToolItemStatusCodeFromUserRunStateCode(
            UserRunStateCode userRunState)
        {
            ToolItemStatusCode toolItemStatus;

            switch (userRunState)
            {
                case UserRunStateCode.Future:
                    toolItemStatus = ToolItemStatusCode.Future;
                    break;
                case UserRunStateCode.Active:
                    toolItemStatus = ToolItemStatusCode.Active;
                    break;
                case UserRunStateCode.Learned:
                    toolItemStatus = ToolItemStatusCode.Learned;
                    break;
                default:
                    toolItemStatus = ToolItemStatusCode.Future;
                    break;
            }

            return toolItemStatus;
        }

        public static UserRunStateCode GetUserRunStateCodeFromToolItemStatusCode(ToolItemStatusCode toolItemStatusCode)
        {
            UserRunStateCode userRunStateCode;

            switch (toolItemStatusCode)
            {
                case ToolItemStatusCode.Future:
                    userRunStateCode = UserRunStateCode.Future;
                    break;
                case ToolItemStatusCode.Active:
                    userRunStateCode = UserRunStateCode.Active;
                    break;
                case ToolItemStatusCode.Learned:
                    userRunStateCode = UserRunStateCode.Learned;
                    break;
                default:
                    userRunStateCode = UserRunStateCode.Future;
                    break;
            }

            return userRunStateCode;
        }

        public static List<string> ContentProgressColors = new List<string>() { "orange", "red", "green", "blue" };
        public static List<string> ContentProgressColorLabels = new List<string>() { "orange", "red", "green", "blue" };
        public static string ContentProgressEmptyColor = "palegoldenrod";
        public static string ContentProgressDisabledColor = "white";
        public static string ContentProgressOutlineColor = "#808080";
            // Number of media seconds that equals 1 study item, for the case where there is no study lists mapped to the media.
        public static double ContentProgressMediaLengthUnit = 5;

        public static string GetToolItemStatusColor(ToolItemStatus toolItemStatus, DateTime now)
        {
            string color;

            switch (toolItemStatus.StatusCode)
            {
                case ToolItemStatusCode.Future:
                    color = ContentProgressColors[0];
                    break;
                case ToolItemStatusCode.Active:
                    if (toolItemStatus.NextTouchTime < now)
                        color = ContentProgressColors[1];
                    else
                        color = ContentProgressColors[2];
                    break;
                case ToolItemStatusCode.Learned:
                    color = ContentProgressColors[3];
                    break;
                default:
                    color = ContentProgressEmptyColor;
                    break;
            }

            return color;
        }

        public static string VocabularyNewColor = "#f3f791";
        public static List<string> VocabularyActiveColors = new List<string>()
        {
            "#80ff80",
            "#99ff99",
            "#b3ffb3",
            "#ccffcc",
            "#E6ffE6"
        };
        public static string VocabularyLearned = "white";
        public static string VocabularyEmpty = "white";

        public static string GetVocabularyItemStatusColor(UserRunItem userRunItem)
        {
            string color;

            switch (userRunItem.UserRunState)
            {
                case UserRunStateCode.Future:
                    color = ContentProgressColors[0];
                    break;
                case UserRunStateCode.Active:
                    if ((userRunItem.Grade > 0) && (userRunItem.Grade <= 5))
                        color = VocabularyActiveColors[userRunItem.Grade - 1];
                    else
                        color = VocabularyEmpty;
                    break;
                case UserRunStateCode.Learned:
                    color = VocabularyLearned;
                    break;
                default:
                    color = VocabularyEmpty;
                    break;
            }

            return color;
        }
    }
}
