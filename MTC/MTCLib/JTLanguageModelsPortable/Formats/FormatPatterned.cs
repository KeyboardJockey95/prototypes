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
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatPatterned : Format
    {
        public string Arrangement { get; set; } // "Line" or "Block".
        public static string DefaultPatternLine = @"%{t}\t%{h}";
        public static string DefaultPatternBlock = @"%p{%{t}}\n%p{%{h}}";
        public string Pattern { get; set; }
        public bool UseComments { get; set; }
        public string CommentPrefix { get; set; }
        public string MediaItemKey { get; set; }
        public string LanguageMediaItemKey { get; set; }
        public string MediaRunKey { get; set; }
        protected int RowCount = 1;  // >1 if multiple lines per item.
        private static string FormatDescription = "This is the base class for line or block-oriented format subclasses,"
            + " where items are formatted according to a user-provided substitution pattern,"
            + " or an optional comment or directive.";

        protected enum ReadState
        {
            Target,
            Header,
            Body,
            Footer
        };

        protected enum DirectiveCodes
        {
            Start,
            End,
            Annotation,
            Master,
            MarkupTemplate,
            MarkupReference,
            Option,
            LanguageMediaItem,
            MediaDescription,
            SpeakerName,
            Mapping
        };

        public FormatPatterned(string arrangement)
            : base("Patterned", "FormatPatterned", FormatDescription, String.Empty, String.Empty,
                  "text/plain", ".txt", null, null, null, null, null)
        {
            Arrangement = arrangement;
            Pattern = DefaultPattern;
            UseComments = true;
            CommentPrefix = "#";
            MediaItemKey = String.Empty;
            LanguageMediaItemKey = String.Empty;
            MediaRunKey = String.Empty;
        }

        public FormatPatterned(FormatPatterned other)
            : base(other)
        {
            Arrangement = other.Arrangement;
            Pattern = other.Pattern;
            UseComments = other.UseComments;
            CommentPrefix = other.CommentPrefix;
            MediaItemKey = String.Empty;
            LanguageMediaItemKey = String.Empty;
            MediaRunKey = String.Empty;
        }

        // For derived classes.
        public FormatPatterned(string arrangement, string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            Arrangement = arrangement;
            Pattern = DefaultPattern;
            UseComments = true;
            CommentPrefix = "#";
            MediaItemKey = String.Empty;
            LanguageMediaItemKey = String.Empty;
            MediaRunKey = String.Empty;
            Ordinal = 0;
            PreserveTargetNames = false;
            Stack = null;
        }

        public override Format Clone()
        {
            return new FormatPatterned(this);
        }

        public bool IsBlockArrangement
        {
            get
            {
                if (Arrangement == "Block")
                    return true;
                return false;
            }
        }

        public bool IsLineArrangement
        {
            get
            {
                if (Arrangement == "Line")
                    return true;
                return false;
            }
        }

        public string DefaultPattern
        {
            get
            {
                if (IsBlockArrangement)
                    return DefaultPatternBlock;

                return DefaultPatternLine;
            }
            set
            {
                switch (Arrangement)
                {
                    case "Line":
                        DefaultPatternLine = value;
                        break;
                    case "Block":
                        DefaultPatternBlock = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Read(Stream stream)
        {
            PreRead(1);

            ReadPatterned(stream);

            PostRead();
        }

        public void ReadPatterned(Stream stream)
        {
            if (!string.IsNullOrEmpty(MasterName) && (MasterName != "(none)") && (Master == null))
            {
                Master = Repositories.ResolveNamedReference("NodeMasters", null, UserRecord.UserName, MasterName) as NodeMaster;

                if (Master == null)
                    throw new Exception(FormatErrorPrefix() + "Can't find node master: " + MasterName);
            }

            DeleteMediaFiles = UserProfile.GetUserOptionFlag("DeleteMediaFiles", true);
            LineNumber = 0;

            if (DeleteBeforeImport)
                DeleteFirst();

            if (IsBlockArrangement &&
                ((TargetType == "DictionaryEntry") ||
                    (TargetType == "MultiLanguageString") ||
                    (TargetType == "MultiLanguageItem")))
            {
                BlockObjects = new List<IBaseObjectKeyed>();
                RowCount = TextUtilities.CountStrings(Pattern, "\\n") + 1;

                using (StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding(true)))
                {
                    TitleCount = 0;
                    ComponentIndex = 0;

                    try
                    {
                        if ((Targets != null) && (Targets.Count != 0))
                        {
                            foreach (IBaseObject target in Targets)
                                ReadObject(reader, target);
                        }
                        else
                        {
                            while (!reader.EndOfStream)
                                ReadLines(reader);

                            SaveBlockObjects();
                        }
                    }
                    finally
                    {
                        BlockObjects = null;
                    }
                }
            }
            else
            {
                RowCount = TextUtilities.CountStrings(Pattern, "\\n") + 1;

                using (StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding(true)))
                {
                    TitleCount = 0;
                    ComponentIndex = 0;

                    Stack = new List<IBaseObject>();

                    if ((Targets != null) && (Targets.Count != 0))
                    {
                        foreach (IBaseObject target in Targets)
                        {
                            ReadObject(reader, target);
                            Stack.Clear();
                        }
                    }
                    else
                    {
                        while (!reader.EndOfStream)
                        {
                            ReadObject(reader, null);
                            Stack.Clear();
                        }
                    }
                }
            }
        }

        protected void ReadObject(StreamReader reader, IBaseObject obj)
        {
            if (obj == null)
            {
                if (TargetType == "BaseObjectNodeTree")
                {
                    if ((TargetLabel == "Lessons") || (TargetLabel == "Plans"))
                    {
                        string label = (TargetLabel == "Lessons" ? "Course" : "Plan");
                        string source = (TargetLabel == "Lessons" ? "Courses" : "Plans");
                        Tree = NodeUtilities.CreateTree(UserRecord, UserProfile, label, source);
                        obj = Tree;
                    }
                    else
                    {
                        Error = FormatErrorPrefix() + "Unknown or unexpect target label: " + TargetLabel;
                        throw new Exception(Error);
                    }
                }
                else if (TargetType == "BaseObjectNode")
                {
                    if (Tree != null)
                    {
                        if ((TargetLabel == "Group") || (TargetLabel == "Lesson"))
                        {
                            BaseObjectNode node = NodeUtilities.CreateNode(UserRecord, UserProfile, Tree, null, TargetLabel);
                            obj = node;
                        }
                        else
                        {
                            Error = FormatErrorPrefix() + "Unknown or unexpect target label: " + TargetLabel;
                            throw new Exception(Error);
                        }
                    }
                    else
                    {
                        Error = FormatErrorPrefix() + "No tree to add the target to.";
                        throw new Exception(Error);
                    }
                }
                else if (TargetType == "MultiLanguageItem")
                {
                    ReadMultiLanguageItems(reader, Pattern);
                    return;
                }
                else if (TargetType == "MultiLanguageString")
                {
                    ReadMultiLanguageStrings(reader, Pattern);
                    return;
                }
                else if (TargetType == "DictionaryEntry")
                {
                    ReadDictionaryEntries(reader, Pattern);
                    return;
                }
                else
                {
                    Error = FormatErrorPrefix() + "Unexpected target type: " + TargetType;
                    throw new Exception(Error);
                }

                Component = null;

                if (IsSupportedStatic("Import", TargetType, "Support"))
                    ReadComponent(reader);
            }
            else if ((obj is BaseObjectContent) || (obj is BaseObjectNode) || (obj is ToolStudyList))
            {
                Component = obj;

                if (IsSupportedStatic("Import", TargetType, "Support"))
                    ReadComponent(reader);

                Component = null;
            }
            else
            {
                Error = FormatErrorPrefix() + "Read not supported for this object type: " + obj.GetType().Name;
                throw new Exception(Error);
            }
        }

        protected void ReadComponent(StreamReader reader)
        {
            string componentType = String.Empty;
            string contentType = String.Empty;
            string contentSubType = String.Empty;
            string titleString = String.Empty;
            BaseObjectTitled titledObject = null;
            BaseObjectContent content = null;
            BaseObjectNode node = null;

            ItemCount = 0;
            ReadStudyLists = null;

            if (Component is BaseObjectTitled)
            {
                titledObject = GetComponent<BaseObjectTitled>();
                titleString = titledObject.GetTitleString(HostLanguageID);
                titledObject.IsPublic = MakeTargetPublic;

                if (titledObject is BaseObjectContent)
                {
                    content = GetComponent<BaseObjectContent>();
                    contentType = content.ContentType;
                    contentSubType = content.ContentSubType;
                    componentType = "BaseObjectContent";
                    ContentStudyList studyList = content.ContentStorageStudyList;
                    if (!IsDoMerge &&
                            (studyList != null) &&
                            (TargetType == "BaseObjectContent"))
                        ItemIndex = studyList.StudyItemCount();
                    else
                        ItemIndex = 0;
                }
                else if (titledObject is BaseObjectNode)
                {
                    node = titledObject as BaseObjectNode;

                    if (node.IsTree())
                    {
                        Tree = node as BaseObjectNodeTree;
                        componentType = "BaseObjectNodeTree";
                    }
                    else
                    {
                        Tree = node.Tree;
                        componentType = "BaseObjectNode";
                    }
                }
            }

            while (!reader.EndOfStream)
                ReadLines(reader);

            ComponentIndex = ComponentIndex + 1;
        }

        protected void ReadLines(StreamReader reader)
        {
            if (IsLineArrangement)
                ReadLinesLine(reader);
            else
                ReadLinesBlock(reader);
        }

        protected void ReadLinesLine(StreamReader reader)
        {
            List<Annotation> annotations = null;
            ReadState state = ReadState.Target;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                LineNumber++;

                if (!String.IsNullOrEmpty(CommentPrefix) && line.StartsWith(CommentPrefix))
                {
                    if (UseComments)
                    {
                        line = line.Substring(CommentPrefix.Length).Trim();
                        ReadDirective(line, reader, ref state, ref annotations);
                    }
                }
                else
                {
                    // Handle multi-row patterns.
                    for (int i = 1; i < RowCount; i++)
                    {
                        line += "\n" + reader.ReadLine();
                        LineNumber++;
                    }

                    ScanLine(Pattern, line, ref annotations);
                    state = ReadState.Body;
                }
            }

            if (state != ReadState.Footer)
                PopComponent();

            FixupMediaRuns();
            SubDivideStudyLists();
            ReadStudyLists = null;
        }

        protected void ReadLinesBlock(StreamReader reader)
        {
            string blockPattern = Pattern;
            StringBuilder sb = new StringBuilder();
            int patternCount = blockPattern.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;
            int blockIndex = 0;
            LanguageID currentLanguageID = null;
            List<Annotation> annotations = null;
            ReadState state = ReadState.Target;

            ItemCount = 0;

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(blockPattern, patternIndex, patternCount, out nextPatternIndex);

                if (patternChr == '%')
                {
                    int tokenStart = nextPatternIndex;
                    string controlStr = ParseAlphabeticString(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                    char braceOpenChr = EscapedChar(blockPattern, nextPatternIndex, patternCount, out tmpIndex);
                    char nextChar;
                    string argument = String.Empty;

                    if (braceOpenChr == '{')
                    {
                        nextPatternIndex = tmpIndex;
                        int argumentStart = nextPatternIndex;
                        do
                        {
                            nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                            if (nextChar == '%')
                            {
                                nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out tmpIndex);
                                if (nextChar == '{')
                                {
                                    nextPatternIndex = tmpIndex;
                                    do
                                    {
                                        nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                                    }
                                    while ((nextPatternIndex < patternCount) && (nextChar != '}'));
                                }
                            }
                        }
                        while ((nextPatternIndex < patternCount) && (nextChar != '}'));
                        nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                        if (nextChar != '}')
                        {
                            Error = "Missing '}' in directive.";
                            return;
                        }
                        argument = blockPattern.Substring(argumentStart, (nextPatternIndex - argumentStart) - 1);
                    }

                    if (UseComments)
                    {
                        ReadDirectives(reader, ref state, ref annotations);
                        if (state == ReadState.Footer)
                            break;
                    }

                    string blockPrefix = sb.ToString();
                    if (!String.IsNullOrEmpty(blockPrefix))
                    {
                        sb.Clear();
                        if (!ReadMatch(reader, blockPrefix))
                        {
                            if (reader.EndOfStream)
                                break;
                            Error += "Block prefix didn't match.  Line: " + LineNumber.ToString()
                                + " Block prefix: " + blockPrefix;
                            return;
                        }
                        LineNumber += TextUtilities.CountChars(blockPrefix, '\n');
                    }

                    switch (controlStr)
                    {
                        case "p":
                            char blockEndChr = EscapedChar(blockPattern, nextPatternIndex, patternCount, out tmpIndex);
                            ReadBlock(reader, argument, blockEndChr, blockIndex++, ref state, ref annotations);
                            break;
                        case "lt":
                        case "lt0":
                            currentLanguageID = ReadLanguage(reader, "Target", 0);
                            break;
                        case "lt1":
                            currentLanguageID = ReadLanguage(reader, "Target", 1);
                            break;
                        case "lt2":
                            currentLanguageID = ReadLanguage(reader, "Target", 2);
                            break;
                        case "lt3":
                            currentLanguageID = ReadLanguage(reader, "Target", 3);
                            break;
                        case "lh":
                        case "lh0":
                            currentLanguageID = ReadLanguage(reader, "Host", 0);
                            break;
                        case "lh1":
                            currentLanguageID = ReadLanguage(reader, "Host", 1);
                            break;
                        case "lh2":
                            currentLanguageID = ReadLanguage(reader, "Host", 2);
                            break;
                        case "lh3":
                            currentLanguageID = ReadLanguage(reader, "Host", 3);
                            break;
                        case "lct":
                        case "lct0":
                            currentLanguageID = ReadLanguageCode(reader, "Target", 0);
                            break;
                        case "lct1":
                            currentLanguageID = ReadLanguageCode(reader, "Target", 1);
                            break;
                        case "lct2":
                            currentLanguageID = ReadLanguageCode(reader, "Target", 2);
                            break;
                        case "lct3":
                            currentLanguageID = ReadLanguageCode(reader, "Target", 3);
                            break;
                        case "lch":
                        case "lch0":
                            currentLanguageID = ReadLanguageCode(reader, "Host", 0);
                            break;
                        case "lch1":
                            currentLanguageID = ReadLanguageCode(reader, "Host", 1);
                            break;
                        case "lch2":
                            currentLanguageID = ReadLanguageCode(reader, "Host", 2);
                            break;
                        case "lch3":
                            currentLanguageID = ReadLanguageCode(reader, "Host", 3);
                            break;
                        default:
                            string tokenString = blockPattern.Substring(tokenStart, nextPatternIndex - tokenStart);
                            LineNumber += TextUtilities.CountChars(tokenString, '\n');
                            sb.Append(tokenString);
                            break;
                    }
                }
                else
                {
                    sb.Append(patternChr);
                    if (patternChr == '\n')
                        LineNumber++;
                }

                if (state == ReadState.Footer)
                    break;
            }

            string blockSuffix = sb.ToString();
            if (!String.IsNullOrEmpty(blockSuffix))
            {
                if (!ReadMatch(reader, blockSuffix))
                {
                    //if (!reader.EndOfStream)
                    //{
                    //    Error += "Block suffix didn't match.  Line: " + LineNumber.ToString()
                    //        + " Block suffix: " + blockSuffix;
                    //    return;
                    //}
                }
                LineNumber += TextUtilities.CountChars(blockSuffix, '\n');
            }

            if (state != ReadState.Footer)
                PopComponent();

            FixupMediaRuns();
            SubDivideStudyLists();
            ReadStudyLists = null;
        }

        protected void ReadBlock(StreamReader reader, string linePattern,
            char blockEndChr, int blockIndex,
            ref ReadState state, ref List<Annotation> annotations)
        {
            string line;

            RowCount = TextUtilities.CountStrings(linePattern, "\\n") + 1;
            ItemIndex = 0;

            for (;;)
            {
                int peekChr = reader.Peek();

                if (peekChr == -1)
                    return;

                if (peekChr == '\r')
                {
                    if (blockEndChr == '\n')
                        peekChr = blockEndChr;
                }

                if (peekChr == blockEndChr)
                    return;

                if ((line = reader.ReadLine()) == null)
                    return;

                LineNumber++;

                if (!String.IsNullOrEmpty(CommentPrefix) && line.StartsWith(CommentPrefix))
                {
                    if (UseComments)
                    {
                        line = line.Substring(CommentPrefix.Length).Trim();
                        ReadDirective(line, reader, ref state, ref annotations);
                        if (state == ReadState.Footer)
                            break;
                    }
                }
                else
                {
                    // Handle multi-row patterns.
                    for (int i = 1; i < RowCount; i++)
                    {
                        line += "\n" + reader.ReadLine();
                        LineNumber++;
                    }

                    if ((blockIndex == 0) || (BlockObjects == null))
                    {
                        if (TargetType == "MultiLanguageItem")
                            ScanMultiLanguageItem(linePattern, line, ref annotations);
                        else if (TargetType == "MultiLanguageString")
                            ScanMultiLanguageString(linePattern, line, ref annotations);
                        else if (TargetType == "DictionaryEntry")
                            ScanDictionaryEntry(linePattern, line, ref annotations);
                        else
                            ScanLine(linePattern, line, ref annotations);
                    }
                    else
                    {
                        if (ItemIndex >= BlockObjects.Count())
                            throw new Exception("ItemIndex mismatch.");

                        IBaseObjectKeyed obj = BlockObjects[ItemIndex];

                        if (TargetType == "MultiLanguageItem")
                            RescanMultiLanguageItem(linePattern, line, (MultiLanguageItem)obj, ref annotations);
                        else if (TargetType == "MultiLanguageString")
                            RescanMultiLanguageString(linePattern, line, (MultiLanguageString)obj, ref annotations);
                        else if (TargetType == "DictionaryEntry")
                            RescanDictionaryEntry(linePattern, line, (DictionaryEntry)obj, ref annotations);
                        else
                            RescanLine(linePattern, line, ref annotations);
                    }

                    state = ReadState.Body;
                }
            }
        }

        protected void ReadDirectives(StreamReader reader,
            ref ReadState state, ref List<Annotation> annotations)
        {
            string line;

            if (String.IsNullOrEmpty(CommentPrefix))
                return;

            for (;;)
            {
                int peekChr = reader.Peek();

                if (peekChr == -1)
                    return;

                if (peekChr != CommentPrefix[0])
                {
                    if ((Component == null) || (GetComponentType() == "BaseObjectContent"))
                        return;
                }

                if ((line = reader.ReadLine()) == null)
                    return;

                LineNumber++;

                if (String.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith(CommentPrefix))
                {
                    if (UseComments)
                    {
                        line = line.Substring(CommentPrefix.Length).Trim();
                        ReadDirective(line, reader, ref state, ref annotations);
                        if (state == ReadState.Footer)
                            break;
                    }
                }
                else
                    Error = "Comment prefix partially matched.";
            }
        }

        protected void ReadDirective(string line, StreamReader reader,
            ref ReadState state, ref List<Annotation> annotations)
        {
            string componentType = String.Empty;
            string contentType = String.Empty;
            string contentSubType = String.Empty;
            MultiLanguageString title = null;
            MultiLanguageString description = null;
            NodeMasterReference masterReference = null;
            MarkupTemplate markupTemplate = null;
            MarkupTemplateReference markupReference = null;
            List<IBaseObjectKeyed> options = null;
            LanguageMediaItem languageMediaItem = null;

            DirectiveCodes directive = ParseDirective(
                line, ref componentType, ref contentType, ref contentSubType,
                ref title, ref description, ref annotations,
                ref masterReference, ref markupTemplate, ref markupReference,
                ref options, ref languageMediaItem);

            switch (directive)
            {
                case DirectiveCodes.Start:
                    if ((state == ReadState.Target) && (Component != null)
                        && (GetComponentType() == componentType))
                    {
                        if (!PreserveTargetNames)
                            SetComponentTypesAndTitle(componentType, contentType, contentSubType, title, description);
                    }
                    else
                        PushComponent(componentType, contentType, contentSubType, title, description,
                            null, null, null, null);
                    state = ReadState.Body;
                    break;
                case DirectiveCodes.End:
                    if ((state == ReadState.Body) || (state == ReadState.Footer))
                        PopComponent();
                    state = ReadState.Footer;
                    break;
                case DirectiveCodes.Annotation:
                    break;
                case DirectiveCodes.Master:
                    SetComponentOptions(masterReference, null, null, null);
                    break;
                case DirectiveCodes.MarkupTemplate:
                    ReadMarkupTemplate(reader, ref markupTemplate, ref markupReference);
                    SetComponentOptions(null, markupTemplate, markupReference, null);
                    break;
                case DirectiveCodes.MarkupReference:
                    SetComponentOptions(null, null, markupReference, null);
                    break;
                case DirectiveCodes.Option:
                    SetComponentOptions(null, null, null, options);
                    break;
                case DirectiveCodes.LanguageMediaItem:
                    ReadLanguageMediaItem(reader, ref languageMediaItem);
                    break;
                case DirectiveCodes.MediaDescription:
                    break;
                case DirectiveCodes.SpeakerName:
                    break;
                case DirectiveCodes.Mapping:
                    break;
                default:
                    break;
            }
        }

        protected LanguageID ReadLanguage(StreamReader reader, string languageDescriptorName, int index)
        {
            List<LanguageID> languageIDs = null;
            LanguageID languageID = null;
            string languageName = languageDescriptorName;

            switch (languageDescriptorName)
            {
                case "Target":
                    languageIDs = TargetLanguageIDs;
                    break;
                case "Host":
                    languageIDs = HostLanguageIDs;
                    break;
                case "":
                case null:
                default:
                    return languageID;
            }

            if (languageIDs == null)
                return languageID;

            if ((index >= 0) && (index < languageIDs.Count))
                languageID = languageIDs[index];

            if (languageID != null)
                languageName = languageID.LanguageName(UserProfile.UILanguageID);

            if (ReadMatch(reader, languageName))
                return languageID;

            return null;
        }

        protected LanguageID ReadLanguageCode(StreamReader reader, string languageDescriptorName, int index)
        {
            List<LanguageID> languageIDs = null;
            LanguageID languageID = null;
            string languageName = languageDescriptorName;

            switch (languageDescriptorName)
            {
                case "Target":
                    languageIDs = TargetLanguageIDs;
                    break;
                case "Host":
                    languageIDs = HostLanguageIDs;
                    break;
                case "":
                case null:
                default:
                    return languageID;
            }

            if (languageIDs == null)
                return languageID;

            if ((index >= 0) && (index < languageIDs.Count))
                languageID = languageIDs[index];

            if (languageID != null)
                languageName = languageID.LanguageCultureExtensionCode;

            if (ReadMatch(reader, languageName))
                return languageID;

            return null;
        }

        protected bool ReadMatch(StreamReader reader, string pattern)
        {
            foreach (char p in pattern)
            {
                int b = reader.Read();

                if (b == -1)
                    return false;

                if (b != p)
                {
                    if ((p == '\n') && (b == '\r'))
                    {
                        b = reader.Read();

                        if (b == -1)
                            return false;
                        else if (b != '\n')
                            return false;
                    }
                    else
                        return false;
                }
            }

            return true;
        }

        protected void ReadMarkupTemplate(StreamReader reader, ref MarkupTemplate markupTemplate,
            ref MarkupTemplateReference markupReference)
        {
            StringBuilder sb = new StringBuilder();

            for (;;)
            {
                string line = reader.ReadLine();

                if (line.StartsWith(CommentPrefix))
                {
                    string testLine = line.Substring(CommentPrefix.Length).Trim();
                    if (testLine.ToLower().StartsWith("end markuptemplate"))
                    {
                        string xmlText = sb.ToString();
                        try
                        {
                            XElement element = XElement.Parse(xmlText, LoadOptions.PreserveWhitespace);
                            markupTemplate = new MarkupTemplate(element);
                            if (markupReference == null)
                                markupReference = new Markup.MarkupTemplateReference(markupTemplate);
                            return;
                        }
                        catch (Exception exc)
                        {
                            Error = exc.Message;
                            if (exc.InnerException != null)
                                Error = Error + ": " + exc.InnerException.Message;
                            Error = Error + "\r\n";
                        }
                    }
                }

                sb.AppendLine(line);
            }
        }

        protected void ReadLanguageMediaItem(StreamReader reader, ref LanguageMediaItem languageMediaItem)
        {
            ContentMediaItem mediaItem = null;

            if (Component is BaseObjectContent)
                mediaItem = GetComponent<BaseObjectContent>().ContentStorageMediaItem;
            else
                mediaItem = GetComponent<ContentMediaItem>();

            if (mediaItem != null)
            {
                LanguageMediaItem testLanguageMediaItem = mediaItem.GetLanguageMediaItem(languageMediaItem.Key);
                if (testLanguageMediaItem != null)
                    languageMediaItem = testLanguageMediaItem;
                else
                    mediaItem.AddLanguageMediaItem(languageMediaItem);
            }

            for (;;)
            {
                string line = reader.ReadLine();

                if (line.StartsWith(CommentPrefix))
                {
                    string testLine = line.Substring(CommentPrefix.Length).Trim();
                    string testLineLower = testLine.ToLower();
                    if (testLineLower.StartsWith("end languagemediaitem"))
                    {
                        if (languageMediaItem.MediaDescriptionCount() == 0)
                            languageMediaItem.GenerateMediaDescriptions(mediaItem.MediaFileNamePattern);
                        return;
                    }
                    else if (testLineLower.StartsWith("mediadescription"))
                    {
                        MediaDescription mediaDescription = null;
                        string mimeType = String.Empty;
                        string file = String.Empty;
                        int dash1 = line.IndexOf('-');
                        if (dash1 >= 0)
                        {
                            line = line.Substring(dash1 + 1);
                            int dash2 = line.IndexOf("-");
                            if (dash2 >= 0)
                            {
                                mimeType = line.Substring(0, dash2).Trim();
                                line = line.Substring(dash2 + 1);
                                file = line.Trim();
                            }
                            else
                                mimeType = line.Trim();
                        }
                        if (!String.IsNullOrEmpty(mimeType))
                        {
                            MediaTypeCode mediaType = MediaUtilities.GetMediaTypeFromMimeType(mimeType);
                            mediaDescription = languageMediaItem.GetMediaDescription(mimeType);
                            if (mediaDescription == null)
                            {
                                mediaDescription = new MediaDescription(mimeType, mediaType, mimeType, file);
                                languageMediaItem.AddMediaDescription(mediaDescription);
                            }
                            else if (!String.IsNullOrEmpty(file))
                                mediaDescription.FileName = file;
                        }
                    }
                }
            }
        }

        protected void ReadMultiLanguageItems(
            StreamReader reader,
            string linePattern)
        {
            throw new Exception("ReadMultiLanguageItems not implemented.");
        }

        protected void ScanMultiLanguageItem(
            string pattern,
            string line,
            ref List<Annotation> annotations)
        {
            throw new Exception("ScanMultiLanguageItem not implemented.");
        }

        protected void RescanMultiLanguageItem(
            string pattern,
            string line,
            MultiLanguageItem obj,
            ref List<Annotation> annotations)
        {
            throw new Exception("RescanMultiLanguageItem not implemented.");
        }

        public void ScanMultiLanguageItemString(
            string pattern,
            string str,
            List<MultiLanguageItem> multiLanguageItems)
        {
            throw new Exception("ScanMultiLanguageItemString not implemented.");
        }

        protected void WriteMultiLanguageItem(
            StreamWriter writer,
            MultiLanguageItem multiLanguageItem,
            string linePattern)
        {
            throw new Exception("WriteMultiLanguageItemnot implemented.");
        }

        protected void ReadMultiLanguageStrings(
            StreamReader reader,
            string linePattern)
        {
            throw new Exception("ReadMultiLanguageStrings not implemented.");
        }

        protected void ScanMultiLanguageString(
            string pattern,
            string line,
            ref List<Annotation> annotations)
        {
            throw new Exception("ScanMultiLanguageString not implemented.");
        }

        protected void RescanMultiLanguageString(
            string pattern,
            string line,
            MultiLanguageString obj,
            ref List<Annotation> annotations)
        {
            throw new Exception("RescanMultiLanguageString not implemented.");
        }

        public void ScanMultiLanguageStringString(
            string pattern,
            string str,
            List<MultiLanguageString> multiLanguageStrings)
        {
            throw new Exception("ScanMultiLanguageStringString not implemented.");
        }

        protected void WriteMultiLanguageString(
            StreamWriter writer,
            MultiLanguageString multiLanguageString,
            string linePattern)
        {
            throw new Exception("WriteMultiLanguageString not implemented.");
        }

        protected void ReadDictionaryEntries(
            StreamReader reader,
            string linePattern)
        {
            List<Annotation> annotations = null;
            ReadState state = ReadState.Target;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                LineNumber++;

                if (!String.IsNullOrEmpty(CommentPrefix) && line.StartsWith(CommentPrefix))
                {
                    if (UseComments)
                    {
                        line = line.Substring(CommentPrefix.Length).Trim();
                        ReadDirective(line, reader, ref state, ref annotations);
                    }
                }
                else
                {
                    // Handle multi-row patterns.
                    for (int i = 1; i < RowCount; i++)
                    {
                        line += "\n" + reader.ReadLine();
                        LineNumber++;
                    }

                    ScanDictionaryEntry(linePattern, line, ref annotations);
                    state = ReadState.Body;
                }
            }

            SaveDictionaryEntries();
        }

        protected void ScanDictionaryEntry(
            string pattern,
            string line,
            ref List<Annotation> annotations)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            if (RowCount > 1)
                line = line.Replace("\r", "");

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>();

            ScanDictionaryString(pattern, line, dictionaryEntries);

            foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
            {
                if ((annotations != null) && (annotations.Count() != 0))
                {
                    //dictionaryEntry.AddAnnotations(annotations);
                    annotations = null;
                }

                try
                {
                    AddDictionaryEntry(dictionaryEntry);
                }
                catch (Exception exc)
                {
                    string message = exc.Message;

                    if (exc.InnerException != null)
                        message += ": " + exc.InnerException.Message;

                    Exception newException = new Exception(FormatErrorPrefix() + message);
                    throw newException;
                }
            }
        }

        protected void RescanDictionaryEntry(
            string pattern,
            string line,
            DictionaryEntry obj,
            ref List<Annotation> annotations)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            if (RowCount > 1)
                line = line.Replace("\r", "");

            List<DictionaryEntry> dictionaryEntries = new List<DictionaryEntry>() { obj };

            ScanDictionaryString(pattern, line, dictionaryEntries);
        }

        public void ScanDictionaryString(
            string pattern,
            string str,
            List<DictionaryEntry> dictionaryEntries)
        {
            str = ScanSubstitutionCheck(str);

            int patternCount = pattern.Length;
            int strCount = str.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;
            int strIndex = 0;
            DictionaryEntry dictionaryEntry = null;
            LanguageDescriptor targetLanguageDescriptor = (LanguageDescriptors != null ? LanguageDescriptors.FirstOrDefault(x => x.Name == "Target") : null);
            LanguageID targetLanguageID = (targetLanguageDescriptor != null ? targetLanguageDescriptor.LanguageID : UniqueLanguageIDs.FirstOrDefault());
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = null;
            string value;

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(pattern, patternIndex, patternCount, out nextPatternIndex);
                char nextPatternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                if ((patternChr == '%') && (nextPatternChr == '{'))
                {
                    int i;

                    for (i = tmpIndex; i < patternCount; i++)
                    {
                        if (pattern[i] == '}')
                            break;
                    }

                    string control = pattern.Substring(tmpIndex, i - tmpIndex);
                    int controlLength = control.Length;
                    nextPatternIndex = i + 1;
                    patternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                    for (i = 0; i < controlLength; i++)
                    {
                        if (!Char.IsDigit(control[i]))
                            break;
                    }

                    if (i != 0)
                        controlLength = Convert.ToInt32(control.Substring(0, i));
                    else
                        controlLength = 0;

                    string descriptorName = null;
                    string mediaDescriptorName = null;
                    int descriptorIndex = 0;
                    int mediaDescriptorIndex = 0;

                    switch (control)
                    {
                        case "t":
                            descriptorName = "Target";
                            descriptorIndex = 0;
                            break;
                        case "t1":
                        case "ta1":
                            descriptorName = "Target";
                            descriptorIndex = 1;
                            break;
                        case "t2":
                        case "ta2":
                            descriptorName = "Target";
                            descriptorIndex = 2;
                            break;
                        case "t3":
                        case "ta3":
                            descriptorName = "Target";
                            descriptorIndex = 3;
                            break;
                        case "h":
                            descriptorName = "Host";
                            descriptorIndex = 0;
                            break;
                        case "h1":
                        case "ha1":
                            descriptorName = "Host";
                            descriptorIndex = 1;
                            break;
                        case "h2":
                        case "ha2":
                            descriptorName = "Host";
                            descriptorIndex = 2;
                            break;
                        case "h3":
                        case "ha3":
                            descriptorName = "Host";
                            descriptorIndex = 3;
                            break;
                        case "mt":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = 0;
                            break;
                        case "mt1":
                        case "mta1":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = 1;
                            break;
                        case "mt2":
                        case "mta2":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = 2;
                            break;
                        case "mt3":
                        case "mta3":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = 3;
                            break;
                        case "mh":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = 0;
                            break;
                        case "mh1":
                        case "mha1":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = 1;
                            break;
                        case "mh2":
                        case "mha2":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = 2;
                            break;
                        case "mh3":
                        case "mha3":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = 3;
                            break;
                        case "d":
                        case "o":
                            for (i = strIndex; i < strCount; i++)
                            {
                                if (!Char.IsDigit(str[i]))
                                    break;
                            }
                            strIndex = i;
                            break;
                        case "s":
                            if (controlLength == 0)
                            {
                                for (i = strIndex; i < strCount; i++)
                                {
                                    if (str[i] == patternChr)
                                        break;
                                }
                                strIndex = i;
                            }
                            else
                            {
                                if (strIndex + controlLength > strCount)
                                    controlLength = strCount - strIndex;
                                strIndex += controlLength;
                            }
                            break;
                        case "c":
                            for (i = strIndex; i < strCount; i++)
                            {
                                if (str[i] == patternChr)
                                    break;
                            }
                            value = str.Substring(strIndex, i - strIndex);
                            strIndex = i;
                            category = Sense.GetLexicalCategoryFromString(value);
                            break;
                        case "cs":
                            for (i = strIndex; i < strCount; i++)
                            {
                                if (str[i] == patternChr)
                                    break;
                            }
                            categoryString = str.Substring(strIndex, i - strIndex);
                            strIndex = i;
                            break;
                        default:
                            if (String.IsNullOrEmpty(Error))
                                Error = FormatErrorPrefix();
                            Error = Error + "\nUnknown %() control: " + control.ToString();
                            break;
                    }

                    if ((descriptorName != null) || (mediaDescriptorName != null))
                    {
                        string name = (descriptorName != null ? descriptorName : mediaDescriptorName);
                        descriptorIndex = (descriptorName != null ? descriptorIndex : mediaDescriptorIndex);

                        for (i = strIndex; i < strCount;)
                        {
                            switch (str[i])
                            {
                                case '(':
                                    SkipToChar(str, ')', patternChr, i, strCount, out i);
                                    break;
                                case '"':
                                    SkipToChar(str, '"', patternChr, i, strCount, out i);
                                    break;
                                case '<':
                                    SkipToChar(str, '>', patternChr, i, strCount, out i);
                                    break;
                                case '[':
                                    SkipToChar(str, ']', patternChr, i, strCount, out i);
                                    break;
                                default:
                                    if (str[i] == patternChr)
                                        goto skip;
                                    i++;
                                    break;
                            }
                        }

                        skip:
                        value = str.Substring(strIndex, i - strIndex).Trim();
                        strIndex = i;
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && (ld.Name == descriptorName))
                                {
                                    if (index == descriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == descriptorName) && x.Used && (x.LanguageID != null));
                        }

                        if (languageDescriptor != null)
                        {
                            LanguageID languageID = languageDescriptor.LanguageID;

                            if (descriptorName != null)
                            {
                                dictionaryEntry = dictionaryEntries.FirstOrDefault();

                                if (dictionaryEntry == null)
                                    dictionaryEntries.Add(dictionaryEntry = new DictionaryEntry(String.Empty, targetLanguageID));

                                dictionaryEntry.ParseDefinition(languageID, value);
                            }
                        }
                    }
                }
                else
                {
                    if (strIndex >= strCount)
                        break;

                    char strChr = str[strIndex++];

                    if (strChr != patternChr)
                    {
                        if (String.IsNullOrEmpty(Error))
                            Error = FormatErrorPrefix();
                        Error = Error + "\nPattern mismatch: " + str;
                        break;
                    }
                }
            }
        }

        protected void WriteDictionaryEntry(
            StreamWriter writer,
            DictionaryEntry dictionaryEntry,
            string linePattern)
        {
            string line = FormatDictionaryLine(linePattern, dictionaryEntry, null);
            writer.WriteLine(line);
            LineNumber += 1 + TextUtilities.GetSubstringCount(line, "\r\n");
        }

        protected string FormatDictionaryLine(
            string pattern,
            DictionaryEntry dictionaryEntry,
            List<object> arguments = null)
        {
            string returnValue = null;

            if (dictionaryEntry != null)
            {
                returnValue = FormatDictionaryString(pattern, dictionaryEntry, arguments);

                if ((RowCount > 1) && (returnValue != null))
                {
                    returnValue = returnValue.Replace("\r\n", "\n");
                    returnValue = returnValue.Replace("\n", "\r\n");
                }
            }

            return returnValue;
        }

        protected string FormatDictionaryString(string pattern, DictionaryEntry dictionaryEntry, List<object> arguments = null)
        {
            string returnValue = null;

            if (String.IsNullOrEmpty(pattern) || dictionaryEntry == null)
                return returnValue;

            StringBuilder sb = new StringBuilder();
            int patternCount = pattern.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;
            int argIndex = 0;
            string argumentString;
            LanguageID languageID = null;
            LexicalCategory category = LexicalCategory.Unknown;
            string categoryString = null;

            if (dictionaryEntry.SenseCount >= 1)
            {
                Sense sense = dictionaryEntry.GetSenseIndexed(0);
                category = sense.Category;
                categoryString = sense.CategoryString;
            }

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(pattern, patternIndex, patternCount, out nextPatternIndex);
                char nextPatternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                if ((patternChr == '%') && (nextPatternChr == '{'))
                {
                    int i;

                    for (i = tmpIndex; i < patternCount; i++)
                    {
                        if (pattern[i] == '}')
                            break;
                    }

                    string control = pattern.Substring(tmpIndex, i - tmpIndex);
                    int controlLength = control.Length;
                    nextPatternIndex = i + 1;
                    patternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                    for (i = 0; i < controlLength; i++)
                    {
                        if (!Char.IsDigit(control[i]))
                            break;
                    }

                    if (i != 0)
                        controlLength = Convert.ToInt32(control.Substring(0, i));
                    else
                        controlLength = 0;

                    string descriptorName = null;
                    int descriptorIndex = 0;

                    switch (control)
                    {
                        case "t":
                            descriptorName = "Target";
                            descriptorIndex = 0;
                            break;
                        case "t1":
                        case "ta1":
                            descriptorName = "Target";
                            descriptorIndex = 1;
                            break;
                        case "t2":
                        case "ta2":
                            descriptorName = "Target";
                            descriptorIndex = 2;
                            break;
                        case "t3":
                        case "ta3":
                            descriptorName = "Target";
                            descriptorIndex = 3;
                            break;
                        case "h":
                            descriptorName = "Host";
                            descriptorIndex = 0;
                            break;
                        case "h1":
                        case "ha1":
                            descriptorName = "Host";
                            descriptorIndex = 1;
                            break;
                        case "h2":
                        case "ha2":
                            descriptorName = "Host";
                            descriptorIndex = 2;
                            break;
                        case "h3":
                        case "ha3":
                            descriptorName = "Host";
                            descriptorIndex = 3;
                            break;
                        case "d":
                            if ((arguments != null) && (argIndex < arguments.Count()))
                                argumentString = String.Format("{0:d" + controlLength.ToString() + "}", arguments[argIndex]);
                            else
                                argumentString = "(no value)";
                            sb.Append(argumentString);
                            argIndex++;
                            break;
                        case "s":
                            if ((arguments != null) && (argIndex < arguments.Count()))
                            {
                                if (controlLength == 0)
                                    argumentString = arguments[argIndex].ToString();
                                else
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", arguments[argIndex]);
                            }
                            else
                                argumentString = "(no value)";
                            sb.Append(argumentString);
                            argIndex++;
                            break;
                        case "o":
                            argumentString = String.Format("{0:d" + controlLength.ToString() + "}", Ordinal);
                            Ordinal = Ordinal + 1;
                            sb.Append(argumentString);
                            break;
                        case "c":
                            argumentString = String.Format("{0:" + controlLength.ToString() + "}", category.ToString());
                            sb.Append(argumentString);
                            break;
                        case "cs":
                            argumentString = String.Format("{0:" + controlLength.ToString() + "}", categoryString);
                            sb.Append(argumentString);
                            break;
                        default:
                            if (String.IsNullOrEmpty(Error))
                                Error = FormatErrorPrefix();
                            Error = Error + "\nUnknown %() control: " + control.ToString();
                            break;
                    }

                    if (descriptorName != null)
                    {
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && (ld.Name == descriptorName))
                                {
                                    if (index == descriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == descriptorName) && x.Used && (x.LanguageID != null));
                            if (languageDescriptor == null)
                                languageDescriptor = UserProfile.UILanguageDescriptor;
                        }

                        if (languageDescriptor != null)
                        {
                            languageID = languageDescriptor.LanguageID;
                            string def = dictionaryEntry.GetExportDefinition(languageID, true, true);
                            sb.Append(def);
                        }
                    }
                }
                else
                    sb.Append(patternChr);
            }

            return sb.ToString();
        }

        public override void Write(Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream, new System.Text.UTF8Encoding(true));
            {
                TitleCount = 0;
                ComponentIndex = 0;
                LineNumber = 0;

                RowCount = TextUtilities.CountStrings(Pattern, "\\n") + 1;

                if (Targets != null)
                {
                    if (IsBlockArrangement &&
                            (TargetType == "DictionaryEntry") ||
                                (TargetType == "MultiLanguageString") ||
                                (TargetType == "MultiLanguageItem"))
                        WriteComponent(writer);
                    else
                    {
                        foreach (IBaseObject target in Targets)
                            WriteObject(writer, target);
                    }
                }

                writer.Flush();
            }
        }

        protected void WriteObject(StreamWriter writer, IBaseObject obj)
        {
            if (obj is BaseObjectContent)
            {
                BaseObjectContent content = obj as BaseObjectContent;

                if (!IsSupportedVirtual("Export", "BaseObjectContent", "Support"))
                    return;

                Boolean exportIt = true;

                Component = obj;

                if (ContentKeyFlags != null)
                {
                    if (!ContentKeyFlags.TryGetValue(content.KeyString, out exportIt))
                        exportIt = true;
                }

                if (exportIt)
                {
                    WriteObjectHeader(writer, obj);
                    WriteComponent(writer);
                }

                if (content.ContentChildrenCount() != 0)
                {
                    foreach (BaseObjectContent childContent in content.ContentChildren)
                    {
                        if (childContent.KeyString == content.KeyString)
                            continue;

                        WriteObject(writer, childContent);
                    }
                }

                if (exportIt)
                {
                    WriteObjectFooter(writer, obj);
                    ComponentIndex = ComponentIndex + 1;
                }

                Component = null;
            }
            else if (obj is BaseObjectNode)
            {
                BaseObjectNode node = obj as BaseObjectNode;

                if (!IsSupportedVirtual("Export", "BaseObjectNode", "Support"))
                    return;

                bool exportIt = true;

                if (NodeKeyFlags != null)
                {
                    if (!NodeKeyFlags.TryGetValue(node.KeyInt, out exportIt))
                        exportIt = true;
                }

                if (exportIt)
                    WriteObjectHeader(writer, obj);

                if (node.ChildCount() != 0)
                {
                    foreach (BaseObjectNode childNode in node.Children)
                        WriteObject(writer, childNode);
                }

                if (exportIt)
                {
                    if (node.ContentChildrenCount() != 0)
                    {
                        foreach (BaseObjectContent content in node.ContentChildren)
                        {
                            ItemIndex = 0;
                            WriteObject(writer, content);
                        }

                        if (UseComments && (node.IsTree() || node.IsGroup()))
                            writer.WriteLine();
                    }
                }

                if (exportIt)
                    WriteObjectFooter(writer, obj);
            }
            else if (obj is MultiLanguageItem)
            {
                WriteMultiLanguageItem(writer, obj as MultiLanguageItem, Pattern);
            }
            else if (obj is MultiLanguageString)
            {
                WriteMultiLanguageString(writer, obj as MultiLanguageString, Pattern);
            }
            else if (obj is DictionaryEntry)
            {
                WriteDictionaryEntry(writer, obj as DictionaryEntry, Pattern);
            }
        }

        protected virtual void WriteObjectHeader(StreamWriter writer, IBaseObject obj)
        {
            IBaseObjectKeyed keyedObject = obj as IBaseObjectKeyed;
            BaseObjectTitled titledObject = obj as BaseObjectTitled;
            BaseObjectNode node = null;
            BaseObjectContent content = null;
            BaseContentStorage contentStorage = null;
            ContentMediaItem mediaItem = null;
            string label;

            TitleCount = TitleCount + 1;

            if (UseComments && !String.IsNullOrEmpty(CommentPrefix))
            {
                string typeLabel;

                if (titledObject != null)
                {
                    string title = FormatMultiLanguageString(titledObject.Title);
                    string description = FormatMultiLanguageString(titledObject.Description);
                    label = typeLabel = titledObject.TypeLabel;
                    label += " - " + title;
                    if (!String.IsNullOrEmpty(description))
                        label += " - " + description;
                }
                else if (keyedObject != null)
                {
                    typeLabel = keyedObject.TypeLabel;
                    label = keyedObject.TypeLabel + " - " + keyedObject.Name;
                }
                else
                    typeLabel = label = obj.GetType().Name;
                writer.WriteLine(CommentPrefix + " " + label);
                if (obj is BaseObjectNode)
                    node = obj as BaseObjectNode;
                else if (obj is BaseObjectContent)
                {
                    content = obj as BaseObjectContent;
                    contentStorage = content.ContentStorage;
                }
                else if (obj is BaseContentStorage)
                    contentStorage = obj as BaseContentStorage;
                if (contentStorage != null)
                {
                    if ((contentStorage.MarkupReference != null) &&
                        (contentStorage.MarkupReference.KeyString != "(none)") &&
                        (contentStorage.MarkupReference.KeyString != String.Empty))
                    {
                        writer.WriteLine(CommentPrefix + " MarkupReference - " + contentStorage.MarkupReference.Name);
                        if ((contentStorage.MarkupReference.KeyString == "(local)") &&
                                (contentStorage.LocalMarkupTemplate != null))
                            WriteXmlObject(writer, "MarkupTemplate", contentStorage.LocalMarkupTemplate);
                    }
                    if (contentStorage is ContentMediaItem)
                    {
                        mediaItem = contentStorage as ContentMediaItem;
                        List<LanguageMediaItem> languageMediaItems = mediaItem.LanguageMediaItems;
                        if (languageMediaItems != null)
                        {
                            foreach (LanguageMediaItem languageMediaItem in languageMediaItems)
                                WriteLanguageMediaItem(writer, languageMediaItem);
                        }
                        List<MultiLanguageString> localSpeakerNames = mediaItem.LocalSpeakerNames;
                        if (localSpeakerNames != null)
                        {
                            foreach (MultiLanguageString localSpeakerName in localSpeakerNames)
                                WriteSpeakerName(writer, localSpeakerName);
                        }
                    }
                    else if (contentStorage is ContentStudyList)
                    {
                        ContentStudyList studyList = contentStorage as ContentStudyList;
                        List<MultiLanguageString> speakerNames = studyList.SpeakerNames;
                        if (speakerNames != null)
                        {
                            foreach (MultiLanguageString speakerName in speakerNames)
                                WriteSpeakerName(writer, speakerName);
                        }
                    }
                }
                if (content != null)
                {
                    if (content.OptionCount() != 0)
                        WriteOptions(writer, content.Options, content.OptionDescriptors);
                }
                else if (contentStorage != null)
                {
                    if (contentStorage.OptionCount() != 0)
                        WriteOptions(writer, contentStorage.Options, contentStorage.OptionDescriptors);
                }
                else if (node != null)
                {
                    if ((node.MarkupReference != null) &&
                        (node.MarkupReference.KeyString != "(none)") &&
                        (node.MarkupReference.KeyString != String.Empty))
                    {
                        writer.WriteLine(CommentPrefix + " MarkupReference - " + node.MarkupReference.Name);
                        if ((node.MarkupReference.KeyString == "(local)") &&
                                (node.LocalMarkupTemplate != null))
                            WriteXmlObject(writer, "MarkupTemplate", node.LocalMarkupTemplate);
                    }
                    if (node.OptionCount() != 0)
                        WriteOptions(writer, node.Options, node.OptionDescriptors);
                    if (node.Master != null)
                        writer.WriteLine(CommentPrefix + " Master - " + node.Master.Name);
                }
                switch (typeLabel)
                {
                    case "Course":
                    case "Plan":
                    case "Group":
                        writer.WriteLine();
                        break;
                    default:
                        break;
                }
                LineNumber++;
            }
        }

        protected void WriteComponent(StreamWriter writer)
        {
            if (IsLineArrangement)
                WriteComponentLine(writer);
            else
                WriteComponentBlock(writer);
        }

        protected void WriteComponentLine(StreamWriter writer)
        {
            ItemCount = GetEntryCount();

            while (ItemIndex < ItemCount)
            {
                string line = FormatLine(Pattern);

                if (line == null)
                    break;
                else if (!String.IsNullOrEmpty(line))
                {
                    writer.WriteLine(line);
                    LineNumber += 1 + TextUtilities.GetSubstringCount(line, "\r\n");
                }
            }
        }

        protected void WriteComponentBlock(StreamWriter writer)
        {
            string blockPattern = Pattern;
            StringBuilder sb = new StringBuilder();
            int patternCount = blockPattern.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;

            ItemCount = GetEntryCount();

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(blockPattern, patternIndex, patternCount, out nextPatternIndex);

                if (patternChr == '%')
                {
                    int tokenStart = nextPatternIndex;
                    string controlStr = ParseAlphabeticString(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                    char braceOpenChr = EscapedChar(blockPattern, nextPatternIndex, patternCount, out tmpIndex);
                    char nextChar;
                    string argument = String.Empty;

                    if (braceOpenChr == '{')
                    {
                        nextPatternIndex = tmpIndex;
                        int argumentStart = nextPatternIndex;
                        do
                        {
                            nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                            if (nextChar == '%')
                            {
                                nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out tmpIndex);
                                if (nextChar == '{')
                                {
                                    nextPatternIndex = tmpIndex;
                                    do
                                    {
                                        nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                                    }
                                    while ((nextPatternIndex < patternCount) && (nextChar != '}'));
                                }
                            }
                        }
                        while ((nextPatternIndex < patternCount) && (nextChar != '}'));
                        nextChar = EscapedChar(blockPattern, nextPatternIndex, patternCount, out nextPatternIndex);
                        if (nextChar != '}')
                        {
                            Error = "Missing '}' in directive.";
                            return;
                        }
                        argument = blockPattern.Substring(argumentStart, (nextPatternIndex - argumentStart) - 1);
                    }

                    string blockPrefix = sb.ToString();
                    if (!String.IsNullOrEmpty(blockPrefix))
                    {
                        sb.Clear();
                        writer.Write(blockPrefix);
                        LineNumber += TextUtilities.CountChars(blockPrefix, '\n');
                    }

                    switch (controlStr)
                    {
                        case "p":
                            WriteBlock(writer, argument);
                            break;
                        case "lt":
                        case "lt0":
                            WriteLanguage(writer, "Target", 0);
                            break;
                        case "lt1":
                            WriteLanguage(writer, "Target", 1);
                            break;
                        case "lt2":
                            WriteLanguage(writer, "Target", 2);
                            break;
                        case "lt3":
                            WriteLanguage(writer, "Target", 3);
                            break;
                        case "lh":
                        case "lh0":
                            WriteLanguage(writer, "Host", 0);
                            break;
                        case "lh1":
                            WriteLanguage(writer, "Host", 1);
                            break;
                        case "lh2":
                            WriteLanguage(writer, "Host", 2);
                            break;
                        case "lh3":
                            WriteLanguage(writer, "Host", 3);
                            break;
                        case "lct":
                        case "lct0":
                            WriteLanguageCode(writer, "Target", 0);
                            break;
                        case "lct1":
                            WriteLanguageCode(writer, "Target", 1);
                            break;
                        case "lct2":
                            WriteLanguageCode(writer, "Target", 2);
                            break;
                        case "lct3":
                            WriteLanguageCode(writer, "Target", 3);
                            break;
                        case "lch":
                        case "lch0":
                            WriteLanguageCode(writer, "Host", 0);
                            break;
                        case "lch1":
                            WriteLanguageCode(writer, "Host", 1);
                            break;
                        case "lch2":
                            WriteLanguageCode(writer, "Host", 2);
                            break;
                        case "lch3":
                            WriteLanguageCode(writer, "Host", 3);
                            break;
                        default:
                            string tokenString = blockPattern.Substring(tokenStart, nextPatternIndex - tokenStart);
                            LineNumber += TextUtilities.CountChars(tokenString, '\n');
                            sb.Append(tokenString);
                            break;
                    }
                }
                else
                {
                    sb.Append(patternChr);
                    if (patternChr == '\n')
                        LineNumber++;
                }
            }

            string blockSuffix = sb.ToString();
            if (!String.IsNullOrEmpty(blockSuffix))
            {
                writer.Write(blockSuffix);
                LineNumber += TextUtilities.CountChars(blockSuffix, '\n');
            }
        }

        protected void WriteBlock(StreamWriter writer, string linePattern)
        {
            if (TargetType == "MultiLanguageItem")
            {
                WriteMultiLanguageItemsBlock(writer, linePattern);
                return;
            }
            else if (TargetType == "MultiLanguageString")
            {
                WriteMultiLanguageStringsBlock(writer, linePattern);
                return;
            }
            else if (TargetType == "DictionaryEntry")
            {
                WriteDictionaryEntriesBlock(writer, linePattern);
                return;
            }
            else
            {
                ItemIndex = 0;

                while (ItemIndex < ItemCount)
                {
                    string line = FormatLine(linePattern);

                    if (line != null)
                    {
                        if (line.Length != 0)
                        {
                            writer.WriteLine(line);
                            LineNumber += 1 + TextUtilities.GetSubstringCount(line, "\r\n");
                        }
                    }
                    else
                        break;
                }
            }
        }

        protected void WriteMultiLanguageItemsBlock(StreamWriter writer, string linePattern)
        {
            if (Targets != null)
            {
                foreach (IBaseObject target in Targets)
                    WriteMultiLanguageItem(writer, target as MultiLanguageItem, linePattern);
            }
        }

        protected void WriteMultiLanguageStringsBlock(StreamWriter writer, string linePattern)
        {
            if (Targets != null)
            {
                foreach (IBaseObject target in Targets)
                    WriteMultiLanguageString(writer, target as MultiLanguageString, linePattern);
            }
        }

        protected void WriteDictionaryEntriesBlock(StreamWriter writer, string linePattern)
        {
            if (Targets != null)
            {
                foreach (IBaseObject target in Targets)
                    WriteDictionaryEntry(writer, target as DictionaryEntry, linePattern);
            }
        }

        protected virtual void WriteObjectFooter(StreamWriter writer, IBaseObject obj)
        {
            if (UseComments && !String.IsNullOrEmpty(CommentPrefix))
            {
                IBaseObjectKeyed keyedObject = obj as IBaseObjectKeyed;
                string typeLabel;

                WriteMappings(writer, obj);

                if (keyedObject != null)
                    typeLabel = keyedObject.TypeLabel;
                else
                    typeLabel = obj.GetType().Name;

                writer.WriteLine(CommentPrefix + " End " + typeLabel);

                switch (typeLabel)
                {
                    case "Course":
                    case "Plan":
                    case "Group":
                    case "Lesson":
                        writer.WriteLine();
                        break;
                    default:
                        break;
                }
            }
        }

        protected void WriteXmlObject(StreamWriter writer, string tag, IBaseObject obj)
        {
            writer.WriteLine(CommentPrefix + " " + tag);
            XElement element = obj.Xml;
            element.Save(writer);
            writer.WriteLine();
            writer.WriteLine(CommentPrefix + " End " + tag);
        }

        protected void WriteLanguageMediaItem(StreamWriter writer, LanguageMediaItem languageMediaItem)
        {
            writer.WriteLine(CommentPrefix + " LanguageMediaItem - "
                + FormatLanguageList(languageMediaItem.TargetLanguageIDs) + " - "
                + FormatLanguageList(languageMediaItem.HostLanguageIDs) + " - "
                + languageMediaItem.KeyString);
            List<MediaDescription> mediaDescriptions = languageMediaItem.MediaDescriptions;
            if (mediaDescriptions != null)
            {
                foreach (MediaDescription mediaDescription in mediaDescriptions)
                    WriteMediaDescription(writer, mediaDescription);
            }
            writer.WriteLine(CommentPrefix + " End LanguageMediaItem");
        }

        protected void WriteMediaDescription(StreamWriter writer, MediaDescription mediaDescription)
        {
            if (mediaDescription == null)
                return;
            writer.WriteLine(CommentPrefix + " MediaDescription - " + mediaDescription.MimeType
                + " - " + mediaDescription.FileName);
        }

        protected void WriteSpeakerName(StreamWriter writer, MultiLanguageString speakerName)
        {
            if (speakerName == null)
                return;
            writer.WriteLine(CommentPrefix + " SpeakerName - " + speakerName.KeyString + " - "
                + FormatMultiLanguageString(speakerName));
        }

        protected void WriteOptions(StreamWriter writer, List<IBaseObjectKeyed> options,
            List<OptionDescriptor> optionDescriptors)
        {
            if (options == null)
                return;

            foreach (IBaseObjectKeyed option in options)
            {
                BaseString optionString = option as BaseString;

                if (optionDescriptors != null)
                {
                    OptionDescriptor optionDescriptor = optionDescriptors.FirstOrDefault(x => x.KeyString == option.KeyString);

                    if (optionDescriptor != null)
                    {
                        if (optionDescriptor.Value == optionString.Text)
                            continue;
                    }
                }

                WriteOption(writer, option as BaseString);
            }
        }

        protected void WriteOption(StreamWriter writer, BaseString option)
        {
            if (option == null)
                return;
            writer.WriteLine(CommentPrefix + " Option - " + option.KeyString + " - " + option.Text);
        }

        protected void WriteMappings(StreamWriter writer, IBaseObject obj)
        {
            List<MultiLanguageItem> studyItems = null;
            List<MultiLanguageString> speakerNames = null;
            ContentStudyList studyList = null;
            ContentMediaItem mediaItem = null;
            if (obj is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                studyList = content.ContentStorageStudyList;
                mediaItem = content.ContentStorageMediaItem;
            }
            else if (obj is ContentStudyList)
                studyList = GetComponent<ContentStudyList>();
            else if (obj is ContentStudyList)
                mediaItem = GetComponent<ContentMediaItem>();
            if (studyList != null)
            {
                studyItems = studyList.StudyItems;
                speakerNames = studyList.SpeakerNames;
            }
            if (mediaItem != null)
            {
                studyItems = mediaItem.LocalStudyItems;
                speakerNames = mediaItem.LocalSpeakerNames;
            }
            WriteItemsMappings(writer, studyItems, speakerNames);
        }

        protected void WriteItemsMappings(StreamWriter writer, List<MultiLanguageItem> studyItems,
            List<MultiLanguageString> speakerNames)
        {
            if (studyItems != null)
            {
                int index = 0;
                int speakerNameIndex = -1;
                MultiLanguageString speakerName = null;

                foreach (MultiLanguageItem studyItem in studyItems)
                {
                    if (studyItem.HasSpeakerNameKey && (speakerNames != null))
                    {
                        speakerName = speakerNames.FirstOrDefault(x => x.MatchKey(studyItem.SpeakerNameKey));
                        if (speakerName != null)
                            speakerNameIndex = speakerNames.IndexOf(speakerName);
                        else
                            speakerNameIndex = -1;
                    }
                    else
                    {
                        speakerNameIndex = -1;
                        speakerName = null;
                    }
                    WriteItemMappings(writer, index, studyItem, speakerNameIndex, speakerName);
                    index++;
                }
            }
        }

        protected void WriteItemMappings(StreamWriter writer, int index, MultiLanguageItem studyItem,
            int speakerNameIndex, MultiLanguageString speakerName)
        {
            if (!StudyItemHasMappings(studyItem))
                return;
            string mappings = FormatItemMappings(studyItem);
            string speakerNameField = String.Empty;
            if (studyItem.HasSpeakerNameKey)
                speakerNameField = ":" + studyItem.SpeakerNameKey;
            writer.WriteLine(CommentPrefix + " Mapping - " + index.ToString() + speakerNameField + " - " + mappings);
        }

        protected bool StudyItemHasMappings(MultiLanguageItem studyItem)
        {
            if (studyItem == null)
                return false;
            foreach (LanguageID languageID in UniqueLanguageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);
                if (languageItem == null)
                    continue;
                if ((languageItem.SentenceRunCount() == 0) && (languageItem.WordRunCount() == 0))
                    continue;
                if (TextRunsHaveMappings(languageItem, languageItem.SentenceRuns))
                    return true;
                if (TextRunsHaveMappings(languageItem, languageItem.WordRuns))
                    return true;
            }
            return false;
        }

        protected bool TextRunsHaveMappings(LanguageItem languageItem, List<TextRun> textRuns)
        {
            if (textRuns == null)
                return false;
            foreach (TextRun textRun in textRuns)
            {
                if (textRun.MediaRunCount() != 0)
                    return true;
                if ((textRun.Start == 0) && ((textRun.Length == 0) || (textRun.Length == languageItem.TextLength)))
                    return false;
                return true;
            }
            return false;
        }

        protected void WriteLanguage(StreamWriter writer, string languageDescriptorName, int index)
        {
            List<LanguageID> languageIDs = null;
            LanguageID languageID = null;
            string languageName = languageDescriptorName;

            switch (languageDescriptorName)
            {
                case "Target":
                    languageIDs = TargetLanguageIDs;
                    break;
                case "Host":
                    languageIDs = HostLanguageIDs;
                    break;
                case "":
                case null:
                    return;
                default:
                    writer.Write(languageDescriptorName);
                    return;
            }

            if (languageIDs == null)
            {
                writer.Write(languageName);
                return;
            }

            if ((index >= 0) && (index < languageIDs.Count))
                languageID = languageIDs[index];

            if (languageID != null)
                languageName = languageID.LanguageName(UserProfile.UILanguageID);

            writer.Write(languageName);
        }

        protected void WriteLanguageCode(StreamWriter writer, string languageDescriptorName, int index)
        {
            List<LanguageID> languageIDs = null;
            LanguageID languageID = null;
            string languageName = languageDescriptorName;

            switch (languageDescriptorName)
            {
                case "Target":
                    languageIDs = TargetLanguageIDs;
                    break;
                case "Host":
                    languageIDs = HostLanguageIDs;
                    break;
                case "":
                case null:
                    return;
                default:
                    writer.Write(languageDescriptorName);
                    return;
            }

            if (languageIDs == null)
            {
                writer.Write(languageName);
                return;
            }

            if ((index >= 0) && (index < languageIDs.Count))
                languageID = languageIDs[index];

            if (languageID != null)
                languageName = languageID.LanguageCultureExtensionCode;

            writer.Write(languageName);
        }

        private static char[] HeadingSeps = { '-' };
        private static char[] TypeSeps = { ' ' };
        private static char[] LanguageSeps = { '|' };

        protected DirectiveCodes ParseDirective(string line, ref string componentType, ref string contentType,
            ref string contentSubType, ref MultiLanguageString title, ref MultiLanguageString description,
            ref List<Annotation> annotations, ref NodeMasterReference masterReference,
            ref MarkupTemplate markupTemplate, ref MarkupTemplateReference markupReference,
            ref List<IBaseObjectKeyed> options, ref LanguageMediaItem languageMediaItem)
        {
            if (line.StartsWith("End"))
                return DirectiveCodes.End;

            string[] parts = line.Split(HeadingSeps, StringSplitOptions.None);
            string typesString = String.Empty;
            string titleString = String.Empty;
            string descriptionString = String.Empty;
            string keyString = String.Empty;

            if (parts.Count() >= 1)
            {
                typesString = parts[0].Trim();
                string[] typesParts = typesString.Split(TypeSeps, StringSplitOptions.RemoveEmptyEntries);

                if (typesParts.Count() >= 1)
                    contentType = typesParts[0].Trim();
                else
                    contentType = "Words";

                if (typesParts.Count() >= 2)
                    contentSubType = typesParts[1].Trim();
                else
                    contentSubType = BaseObjectContent.GetContentSubTypes(contentType).FirstOrDefault();

                titleString = typesString;
            }
            else
            {
                contentType = "Words";
                contentSubType = "Vocabulary";
                typesString = titleString = "Words Vocabulary";
            }

            if (parts.Count() >= 2)
                titleString = parts[1].Trim();

            if (parts.Count() >= 3)
                descriptionString = parts[2].Trim();

            if (parts.Count() >= 4)
                keyString = parts[3].Trim();

            switch (contentType)
            {
                case "Course":
                case "Plan":
                    componentType = "BaseObjectNodeTree";
                    title = ParseMultiLanguageString("Title", titleString);
                    description = ParseMultiLanguageString("Description", descriptionString);
                    annotations = null;
                    masterReference = null;
                    markupTemplate = null;
                    markupReference = null;
                    options = null;
                    break;
                case "Group":
                case "Lesson":
                    componentType = "BaseObjectNode";
                    title = ParseMultiLanguageString("Title", titleString);
                    description = ParseMultiLanguageString("Description", descriptionString);
                    annotations = null;
                    masterReference = null;
                    markupTemplate = null;
                    markupReference = null;
                    options = null;
                    break;
                case "Document":
                case "Audio":
                case "Video":
                case "Automated":
                case "Image":
                case "TextFile":
                case "PDF":
                case "Embedded":
                case "Media":
                case "Transcript":
                case "Text":
                case "Sentences":
                case "Words":
                case "Characters":
                case "Expansion":
                case "Exercises":
                case "Notes":
                case "Comments":
                    componentType = "BaseObjectContent";
                    annotations = null;
                    masterReference = null;
                    markupTemplate = null;
                    markupReference = null;
                    options = null;
                    if (String.IsNullOrEmpty(titleString))
                    {
                        if ((typesString != contentSubType) && !typesString.Contains(contentSubType))
                            titleString = typesString + " " + contentSubType;
                        else
                            titleString = typesString;
                    }
                    title = ParseMultiLanguageString("Title", titleString);
                    description = ParseMultiLanguageString("Description", descriptionString);
                    break;
                case "Master":
                    {
                        NodeMaster master = Repositories.ResolveNamedReference(
                            "NodeMasters", null, UserRecord.UserName, titleString) as NodeMaster;
                        if (master != null)
                            masterReference = new NodeMasterReference(master);
                        else
                            Error = "Unknown master reference: " + title;
                    }
                    return DirectiveCodes.Master;
                case "MarkupTemplate":
                    return DirectiveCodes.MarkupTemplate;
                case "MarkupReference":
                    {
                        MarkupTemplate markup = Repositories.ResolveNamedReference(
                            "MarkupTemplates", null, UserRecord.UserName, titleString) as MarkupTemplate;
                        if (markup != null)
                            markupReference = new MarkupTemplateReference(markup);
                        else
                            Error = "Unknown markup template reference: " + title;
                    }
                    return DirectiveCodes.MarkupReference;
                case "Option":
                    {
                        BaseString option = new BaseString(titleString, descriptionString);
                        if (options == null)
                            options = new List<IBaseObjectKeyed> { option };
                        else
                            options.Add(option);
                    }
                    return DirectiveCodes.Option;
                case "LanguageMediaItem":
                    {
                        List<LanguageID> targetLanguageIDs = ParseLanguageList(titleString);
                        List<LanguageID> hostLanguageIDs = ParseLanguageList(descriptionString);
                        languageMediaItem = new LanguageMediaItem(
                            keyString,
                            targetLanguageIDs,
                            hostLanguageIDs,
                            UserRecord.UserName);
                    }
                    return DirectiveCodes.LanguageMediaItem;
                case "SpeakerName":
                    {
                        MultiLanguageString speakerName = ParseMultiLanguageString(titleString, descriptionString);
                        SetSpeakerName(speakerName);
                    }
                    return DirectiveCodes.SpeakerName;
                case "Mapping":
                    ParseItemMapping(line);
                    return DirectiveCodes.Mapping;
                case "Heading":
                case "Label":
                case "Prefix":
                case "Suffix":
                case "Note":
                case "QuestionNote":
                case "AnswerNote":
                case "FootNote":
                case "Category":
                case "Conjugation":
                case "Ordinal":
                    {
                        MultiLanguageString text = ParseMultiLanguageString(contentType, titleString);
                        Annotation annotation = new Annotation(contentType, null, text);
                        if (annotations == null)
                            annotations = new List<Annotation>();
                        annotations.Add(annotation);
                    }
                    return DirectiveCodes.Annotation;
                case "Break":
                    {
                        Annotation annotation = new Annotation(contentType, null, null);
                        if (annotations == null)
                            annotations = new List<Annotation>();
                        annotations.Add(annotation);
                    }
                    return DirectiveCodes.Annotation;
                case "Style":
                case "Tag":
                    {
                        Annotation annotation = new Annotation(contentType, titleString, null);
                        if (annotations == null)
                            annotations = new List<Annotation>();
                        annotations.Add(annotation);
                    }
                    return DirectiveCodes.Annotation;
                case "TextAnnotation":
                    {
                        MultiLanguageString text = ParseMultiLanguageString(contentType, titleString);
                        Annotation annotation = new Annotation(contentType, descriptionString, text);
                        if (annotations == null)
                            annotations = new List<Annotation>();
                        annotations.Add(annotation);
                    }
                    return DirectiveCodes.Annotation;
                default:
                    componentType = String.Empty;
                    break;
            }

            return DirectiveCodes.Start;
        }

        protected void SetSpeakerName(MultiLanguageString speakerName)
        {
            List<MultiLanguageString> speakerNames = null;
            if (Component is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                switch (content.ContentClass)
                {
                    case ContentClassType.StudyList:
                        ContentStudyList studyList = content.ContentStorageStudyList;
                        if (studyList != null)
                        {
                            speakerNames = studyList.SpeakerNames;
                            if (speakerNames == null)
                            {
                                speakerNames = new List<MultiLanguageString>();
                                studyList.SpeakerNames = speakerNames;
                            }
                        }
                        break;
                    case ContentClassType.DocumentItem:
                        break;
                    case ContentClassType.MediaList:
                        break;
                    case ContentClassType.MediaItem:
                        ContentMediaItem mediaItem = content.ContentStorageMediaItem;
                        if (mediaItem != null)
                        {
                            speakerNames = mediaItem.LocalSpeakerNames;
                            if (speakerNames == null)
                            {
                                speakerNames = new List<MultiLanguageString>();
                                mediaItem.LocalSpeakerNames = speakerNames;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (Component is ContentStudyList)
            {
                ContentStudyList studyList = GetComponent<ContentStudyList>();
                speakerNames = studyList.SpeakerNames;
                if (speakerNames == null)
                {
                    speakerNames = new List<MultiLanguageString>();
                    studyList.SpeakerNames = speakerNames;
                }
            }
            else if (Component is ContentMediaItem)
            {
                ContentMediaItem mediaItem = GetComponent<ContentMediaItem>();
                speakerNames = mediaItem.LocalSpeakerNames;
                if (speakerNames == null)
                {
                    speakerNames = new List<MultiLanguageString>();
                    mediaItem.LocalSpeakerNames = speakerNames;
                }
            }
            MultiLanguageString oldSpeakerName = speakerNames.FirstOrDefault(x => x.MatchKey(speakerName.Key));
            if (oldSpeakerName != null)
                oldSpeakerName.CopyDeep(speakerName);
            else
                speakerNames.Add(speakerName);
        }

        protected Dictionary<string, string> Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected virtual void ScanLine(string pattern, string line, ref List<Annotation> annotations)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            if (RowCount > 1)
                line = line.Replace("\r", "");

            List<MultiLanguageItem> multiLanguageItems = new List<MultiLanguageItem>();

            ScanString(pattern, line, multiLanguageItems, Tags);

            foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
            {
                if ((annotations != null) && (annotations.Count() != 0))
                {
                    multiLanguageItem.AddAnnotations(annotations);
                    annotations = null;
                }

                try
                {
                    NodeUtilities.AutoResetWordRuns(multiLanguageItem, UniqueLanguageIDs);
                    multiLanguageItem.AutoResetSentenceRuns(UniqueLanguageIDs);
                    AddEntry(multiLanguageItem, null, Tags);
                }
                catch (Exception exc)
                {
                    string message = exc.Message;

                    if (exc.InnerException != null)
                        message += ": " + exc.InnerException.Message;

                    Exception newException = new Exception(FormatErrorPrefix() + message);
                    throw newException;
                }
            }
        }

        protected void RescanLine(string pattern, string line, ref List<Annotation> annotations)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return;

            if (RowCount > 1)
                line = line.Replace("\r", "");

            List<MultiLanguageItem> multiLanguageItems = new List<MultiLanguageItem>();

            ScanString(pattern, line, multiLanguageItems, Tags);

            foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
            {
                if ((annotations != null) && (annotations.Count() != 0))
                {
                    multiLanguageItem.AddAnnotations(annotations);
                    annotations = null;
                }

                try
                {
                    MergeEntry(multiLanguageItem, null, Tags);
                }
                catch (Exception exc)
                {
                    string message = exc.Message;

                    if (exc.InnerException != null)
                        message += ": " + exc.InnerException.Message;

                    Exception newException = new Exception(FormatErrorPrefix() + message);
                    throw newException;
                }
            }
        }

        protected string FormatLine(string pattern, List<object> arguments = null)
        {
            MultiLanguageItem multiLanguageItem = GetEntry();
            string returnValue = null;

            if (multiLanguageItem != null)
            {
                bool useIt = true;

                if (ItemKeyFlags != null)
                {
                    if (!ItemKeyFlags.TryGetValue(multiLanguageItem.CompoundStudyItemKey, out useIt))
                        useIt = true;
                }

                if (useIt)
                {
                    returnValue = FormatString(pattern, multiLanguageItem, arguments);

                    if ((RowCount > 1) && (returnValue != null))
                    {
                        returnValue = returnValue.Replace("\r\n", "\n");
                        returnValue = returnValue.Replace("\n", "\r\n");
                    }

                    if (UseComments)
                    {
                        if (multiLanguageItem.HasAnnotations())
                        {
                            string annotationLines = FormatAnnotations(multiLanguageItem.Annotations);

                            if (!String.IsNullOrEmpty(annotationLines))
                                returnValue = annotationLines + "\r\n" + returnValue;
                        }
                    }
                }
                else
                    returnValue = String.Empty;
            }

            return returnValue;
        }

        protected virtual string FormatString(string pattern, MultiLanguageItem multiLanguageItem, List<object> arguments = null)
        {
            string returnValue = null;

            if (String.IsNullOrEmpty(pattern) || multiLanguageItem == null)
                return returnValue;

            StringBuilder sb = new StringBuilder();
            int patternCount = pattern.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;
            int argIndex = 0;
            string argumentString;

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(pattern, patternIndex, patternCount, out nextPatternIndex);
                char nextPatternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                if ((patternChr == '%') && (nextPatternChr == '{'))
                {
                    int i;

                    for (i = tmpIndex; i < patternCount; i++)
                    {
                        if (pattern[i] == '}')
                            break;
                    }

                    string control = pattern.Substring(tmpIndex, i - tmpIndex);
                    int controlIndex = ObjectUtilities.GetIntegerFromStringEnd(control, 0);
                    control = ObjectUtilities.RemoveNumberFromStringEnd(control);
                    int controlLength = control.Length;
                    nextPatternIndex = i + 1;
                    patternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                    for (i = 0; i < controlLength; i++)
                    {
                        if (!Char.IsDigit(control[i]))
                            break;
                    }

                    if (i != 0)
                        controlLength = Convert.ToInt32(control.Substring(0, i));
                    else
                        controlLength = 0;

                    string descriptorName = null;
                    int descriptorIndex = 0;
                    string speakerNameDescriptorName = null;
                    int speakerNameDescriptorIndex = 0;
                    string mediaDescriptorName = null;
                    int mediaDescriptorIndex = 0;
                    string mediaDescriptorKey = null;

                    switch (control)
                    {
                        case "t":
                        case "ta":
                            descriptorName = "Target";
                            descriptorIndex = controlIndex;
                            break;
                        case "h":
                        case "ha":
                            descriptorName = "Host";
                            descriptorIndex = controlIndex;
                            break;
                        case "n":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = controlIndex;
                            break;
                        case "nt":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = controlIndex;
                            break;
                        case "nh":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = controlIndex;
                            break;
                        case "m":
                        case "mt":
                        case "mta":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Audio";
                            break;
                        case "mh":
                        case "mha":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Audio";
                            break;
                        case "mp":
                        case "mtp":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Picture";
                            break;
                        case "mhp":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Picture";
                            break;
                        case "d":
                            if ((arguments != null) && (argIndex < arguments.Count()))
                                argumentString = String.Format("{0:d" + controlLength.ToString() + "}", arguments[argIndex]);
                            else
                                argumentString = "(no value)";
                            sb.Append(argumentString);
                            argIndex++;
                            break;
                        case "s":
                            if ((arguments != null) && (argIndex < arguments.Count()))
                            {
                                if (controlLength == 0)
                                    argumentString = arguments[argIndex].ToString();
                                else
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", arguments[argIndex]);
                            }
                            else
                                argumentString = "(no value)";
                            sb.Append(argumentString);
                            argIndex++;
                            break;
                        case "o":
                            argumentString = String.Format("{0:d" + controlLength.ToString() + "}", Ordinal);
                            Ordinal = Ordinal + 1;
                            sb.Append(argumentString);
                            break;
                        case "title":
                            if (Component is BaseObjectTitled)
                            {
                                BaseObjectTitled content = GetComponent<BaseObjectTitled>();
                                if (content.Title != null)
                                {
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", content.GetTitleString(HostLanguageID));
                                    sb.Append(argumentString);
                                }
                            }
                            break;
                        case "description":
                            if (Component is BaseObjectTitled)
                            {
                                BaseObjectTitled content = GetComponent<BaseObjectTitled>();
                                if (content.Title != null)
                                {
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", content.Description.Text(HostLanguageID));
                                    sb.Append(argumentString);
                                }
                            }
                            break;
                        case "label":
                            if (Component is BaseObjectTitled)
                            {
                                BaseObjectTitled content = GetComponent<BaseObjectTitled>();
                                if (content.Label != null)
                                {
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", content.Label);
                                    sb.Append(argumentString);
                                }
                            }
                            break;
                        case "contentType":
                            if (Component is BaseObjectContent)
                            {
                                BaseObjectContent content = GetComponent<BaseObjectContent>();
                                argumentString = String.Format("{0:" + controlLength.ToString() + "}", content.ContentType);
                                sb.Append(argumentString);
                            }
                            break;
                        case "contentSubType":
                            if (Component is BaseObjectContent)
                            {
                                BaseObjectContent content = GetComponent<BaseObjectContent>();
                                argumentString = String.Format("{0:" + controlLength.ToString() + "}", content.ContentSubType);
                                sb.Append(argumentString);
                            }
                            break;
                        case "node":
                        case "studyList":
                        case "tag":
                            if (Component is BaseObjectContent)
                            {
                                BaseObjectContent content = GetComponent<BaseObjectContent>();
                                BaseObjectNode node = content.Node;
                                if ((node != null) && (node.Title != null))
                                {
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", node.GetTitleString(HostLanguageID));
                                    sb.Append(argumentString);
                                }
                            }
                            else if (Component is BaseObjectTitled)
                            {
                                BaseObjectTitled content = GetComponent<BaseObjectTitled>();
                                if (content.Title != null)
                                {
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", content.GetTitleString(HostLanguageID));
                                    sb.Append(argumentString);
                                }
                            }
                            break;
                        default:
                            if (String.IsNullOrEmpty(Error))
                                Error = FormatErrorPrefix();
                            Error = Error + "\nUnknown %() control: " + control.ToString();
                            break;
                    }

                    if (descriptorName != null)
                    {
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && (ld.Name == descriptorName))
                                {
                                    if (index == descriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == descriptorName) && x.Used && (x.LanguageID != null));
                            if (languageDescriptor == null)
                                languageDescriptor = UserProfile.UILanguageDescriptor;
                        }

                        if (languageDescriptor != null)
                        {
                            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageDescriptor.LanguageID);

                            if (languageItem != null)
                                sb.Append(languageItem.Text.Replace("\r", "").Replace("\n", ""));
                        }
                    }

                    if (speakerNameDescriptorName != null)
                    {
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && (ld.Name == speakerNameDescriptorName))
                                {
                                    if (index == speakerNameDescriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == speakerNameDescriptorName) && x.Used && (x.LanguageID != null));
                        }

                        if (languageDescriptor != null)
                        {
                            string speakerNameKey = multiLanguageItem.SpeakerNameKey;
                            MultiLanguageString speakerNameMLS = null;
                            string speakerName = String.Empty;

                            if (!String.IsNullOrEmpty(speakerNameKey))
                                speakerNameMLS = multiLanguageItem.SpeakerName;

                            if (speakerNameMLS != null)
                                speakerName = speakerNameMLS.Text(languageDescriptor.LanguageID);

                            if (!String.IsNullOrEmpty(speakerName))
                            {
                                if (controlLength == 0)
                                    argumentString = speakerName;
                                else
                                    argumentString = String.Format("{0:" + controlLength.ToString() + "}", speakerName);

                                sb.Append(argumentString);
                            }
                        }
                    }

                    if (mediaDescriptorName != null)
                    {
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && (ld.Name == mediaDescriptorName))
                                {
                                    if (index == mediaDescriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == mediaDescriptorName) && x.Used && (x.LanguageID != null));
                        }

                        if (languageDescriptor != null)
                        {
                            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageDescriptor.LanguageID);
                            string mediaFiles = String.Empty;
                            if ((languageItem != null) && (languageItem.SentenceRunCount() != 0))
                            {
                                foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                                {
                                    MediaRun mediaRun = sentenceRun.GetMediaRun(mediaDescriptorKey);
                                    if (mediaRun != null)
                                    {
                                        if (!mediaRun.IsReference)
                                        {
                                            if (!String.IsNullOrEmpty(mediaFiles))
                                                mediaFiles += "+";

                                            mediaFiles += mediaRun.FileName;
                                        }
                                    }
                                }

                                sb.Append(mediaFiles);
                            }
                        }
                    }
                }
                else
                    sb.Append(patternChr);
            }

            return sb.ToString();
        }

        public string FormatAnnotations(List<Annotation> annotations)
        {
            if (annotations == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (Annotation annotation in annotations)
            {
                string annotationLine = FormatAnnotation(annotation);

                if (!String.IsNullOrEmpty(annotationLine))
                {
                    if (sb.Length != 0)
                        sb.Append("\r\n");

                    sb.Append(annotationLine);
                }
            }

            return sb.ToString();
        }

        public string FormatAnnotation(Annotation annotation)
        {
            if (annotation == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            sb.Append(CommentPrefix);
            sb.Append(annotation.Type);

            if (annotation.IsValueType())
            {
                sb.Append(" - ");
                sb.Append(annotation.Value);
            }
            else if (annotation.IsTextType())
            {
                sb.Append(" - ");
                sb.Append(FormatMultiLanguageString(annotation.Text));
            }

            return sb.ToString();
        }

        public string FormatMultiLanguageString(MultiLanguageString mls)
        {
            string returnValue = String.Empty;
            if (mls == null)
                return returnValue;
            List<LanguageID> languageIDs = new List<LanguageID>(UniqueLanguageIDs);
            int index = 0;
            if (mls.HasText(languageIDs))
            {
                foreach (LanguageID languageID in languageIDs)
                {
                    if (index != 0)
                        returnValue += "|";
                    returnValue += mls.Text(languageID);
                    index++;
                }
            }
            return returnValue;
        }

        public MultiLanguageString ParseMultiLanguageString(string key, string str)
        {
            MultiLanguageString mls = new MultiLanguageString(key);
            ObjectUtilities.PrepareMultiLanguageString(mls, String.Empty, UniqueLanguageIDs);
            if (String.IsNullOrEmpty(str))
                return mls;
            string[] parts = str.Split(LanguageSeps, StringSplitOptions.None);
            int index = 0;
            foreach (string part in parts)
            {
                if (index >= UniqueLanguageIDs.Count())
                    break;
                LanguageID languageID = UniqueLanguageIDs[index++];
                mls.SetText(languageID, part);
            }
            return mls;
        }

        public string FormatLanguageList(List<LanguageID> languageIDs)
        {
            string returnValue = String.Empty;
            if (languageIDs == null)
                return returnValue;
            if (languageIDs.Count == 0)
                return returnValue;
            int index = 0;
            foreach (LanguageID languageID in languageIDs)
            {
                if (index != 0)
                    returnValue += "|";
                returnValue += languageID.LanguageCultureExtensionCode;
                index++;
            }
            return returnValue;
        }

        public List<LanguageID> ParseLanguageList(string str)
        {
            List<LanguageID> languageIDs = new List<LanguageID>();
            if (String.IsNullOrEmpty(str))
                return languageIDs;
            string[] parts = str.Split(LanguageSeps, StringSplitOptions.None);
            foreach (string part in parts)
            {
                LanguageID languageID = new LanguageID(part);
                languageIDs.Add(languageID);
            }
            return languageIDs;
        }

        protected string FormatItemMappings(MultiLanguageItem studyItem)
        {
            StringBuilder sb = new StringBuilder();
            if (studyItem == null)
                return String.Empty;
            int index = 0;
            foreach (LanguageID languageID in UniqueLanguageIDs)
            {
                LanguageItem languageItem = studyItem.LanguageItem(languageID);
                if (index != 0)
                    sb.Append("|");
                if (languageItem == null)
                {
                    index++;
                    continue;
                }
                sb.Append(FormatLanguageItemMappings(languageItem));
                index++;
            }
            return sb.ToString();
        }

        protected string FormatLanguageItemMappings(LanguageItem languageItem)
        {
            StringBuilder sb = new StringBuilder();
            if (languageItem == null)
                return String.Empty;
            sb.Append(FormatTextRunsMappings("Sentence", languageItem.SentenceRuns));
            if (languageItem.WordRunCount() != 0)
            {
                if (languageItem.SentenceRunCount() != 0)
                    sb.Append(",");
                sb.Append(FormatTextRunsMappings("Word", languageItem.WordRuns));
            }
            return sb.ToString();
        }

        protected string FormatTextRunsMappings(string prefix, List<TextRun> textRuns)
        {
            StringBuilder sb = new StringBuilder();
            if (textRuns == null)
                return String.Empty;
            int index = 0;
            foreach (TextRun textRun in textRuns)
            {
                if (index != 0)
                    sb.Append(",");
                sb.Append(FormatTextRun(prefix, textRun));
                index++;
            }
            return sb.ToString();
        }

        protected string FormatTextRun(string prefix, TextRun textRun)
        {
            StringBuilder sb = new StringBuilder();
            if (textRun == null)
                return String.Empty;
            sb.Append(prefix + "Run(");
            sb.Append(textRun.Start);
            sb.Append(",");
            sb.Append(textRun.Stop);
            if (textRun.MediaRunCount() != 0)
            {
                int index = 0;
                sb.Append(",");
                foreach (MediaRun mediaRun in textRun.MediaRuns)
                {
                    if (index != 0)
                        sb.Append(",");
                    sb.Append(FormatMediaRun(mediaRun));
                    index++;
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        protected string FormatMediaRun(MediaRun mediaRun)
        {
            StringBuilder sb = new StringBuilder();
            if (mediaRun == null)
                return String.Empty;
            sb.Append("MediaRun(");
            sb.Append(FormatQuotedString(mediaRun.KeyString));
            sb.Append(",");
            sb.Append(FormatQuotedString(mediaRun.FileName));
            sb.Append(",");
            sb.Append(mediaRun.Start.ToString());
            sb.Append(",");
            sb.Append(mediaRun.Stop.ToString());
            sb.Append(",");
            sb.Append(mediaRun.StorageState.ToString());
            sb.Append(",");
            sb.Append(FormatQuotedString(mediaRun.MediaItemKey));
            sb.Append(",");
            sb.Append(FormatQuotedString(mediaRun.LanguageMediaItemKey));
            sb.Append(")");
            return sb.ToString();
        }

        protected string FormatQuotedString(string str)
        {
            if (!String.IsNullOrEmpty(str))
                return "\"" + str + "\"";
            else
                return "\"\"";
        }

        protected void ParseItemMapping(string line)
        {
            int dash1 = line.IndexOf('-');
            if (dash1 < 0)
                return;
            line = line.Substring(dash1 + 1);
            int dash2 = line.IndexOf('-');
            if (dash2 < 0)
                return;
            string indexString = line.Substring(0, dash2).Trim();
            int studyItemIndex = 0;
            string speakerNameKey = null;
            ParseInteger(ref indexString, out studyItemIndex);
            if (indexString.StartsWith(":"))
            {
                indexString = indexString.Substring(1);
                speakerNameKey = indexString.Trim();
            }
            MultiLanguageItem studyItem = GetStudyItemIndexed(studyItemIndex);
            if (studyItem == null)
                return;
            studyItem.SpeakerNameKey = speakerNameKey;
            line = line.Substring(dash2 + 1).Trim();
            string[] parts = line.Split(LanguageSeps, StringSplitOptions.None);
            int index = 0;
            foreach (string part in parts)
            {
                if (index >= UniqueLanguageIDs.Count)
                    break;
                LanguageID languageID = UniqueLanguageIDs[index];
                index++;
                LanguageItem languageItem = studyItem.LanguageItem(languageID);
                if (languageItem == null)
                    continue;
                ParseLanguageItemMapping(part, languageItem);
            }
        }

        protected void ParseLanguageItemMapping(string str, LanguageItem languageItem)
        {
            languageItem.SentenceRuns = ParseTextRuns(ref str, "sentence");
            languageItem.WordRuns = ParseTextRuns(ref str, "word");
        }

        protected List<TextRun> ParseTextRuns(ref string str, string prefix)
        {
            List<TextRun> textRuns = null;

            SkipWhiteSpace(ref str);

            while (!String.IsNullOrEmpty(str))
            {
                str = str.Trim();
                string strLower = str.ToLower();

                if (strLower.StartsWith(prefix + "run("))
                {
                    TextRun textRun = ParseTextRun(ref str, prefix);

                    if (textRun == null)
                        return textRuns;

                    if (textRuns == null)
                        textRuns = new List<TextRun>();

                    textRuns.Add(textRun);
                }
                else
                    return textRuns;
            }

            return textRuns;
        }

        protected TextRun ParseTextRun(ref string str, string prefix)
        {
            int start = 0;
            int stop = 0;
            List<MediaRun> mediaRuns = null;
            TextRun textRun = null;

            SkipWhiteSpace(ref str);

            if (!str.ToLower().StartsWith(prefix + "run("))
                return null;

            str = str.Substring(prefix.Length + 4);

            if (!ParseInteger(ref str, out start))
                return null;

            if (!ParseToken(ref str, ","))
                return null;

            if (!ParseInteger(ref str, out stop))
                return null;

            if (!ParseMediaRuns(ref str, out mediaRuns))
                return null;

            if (!ParseToken(ref str, ")"))
                return null;

            textRun = new TextRun(start, stop, mediaRuns);

            return textRun;
        }

        protected bool ParseInteger(ref string str, out int value)
        {
            int index = 0;

            SkipWhiteSpace(ref str);
            int count = str.Length;

            while ((index < count) && char.IsDigit(str[index]))
                index++;

            if (index == 0)
            {
                value = 0;
                return false;
            }

            string integerString = str.Substring(0, index);
            value = ObjectUtilities.GetIntegerFromString(integerString, 0);
            str = str.Substring(index);

            return true;
        }

        protected bool ParseQuotedString(ref string str, out string value)
        {
            int index = 0;

            value = String.Empty;

            SkipWhiteSpace(ref str);

            if (!ParseToken(ref str, "\""))
                return false;

            while ((index < str.Length) && (str[index] != '"'))
                index++;

            value = str.Substring(0, index);

            str = str.Substring(index);

            if (!ParseToken(ref str, "\""))
                return false;

            return true;
        }

        protected bool ParseDelimitedString(ref string str, char[] delimiters, out string value)
        {
            int index = 0;

            value = String.Empty;

            SkipWhiteSpace(ref str);

            while ((index < str.Length) && !delimiters.Contains(str[index]))
                index++;

            value = str.Substring(0, index);

            str = str.Substring(index);

            return true;
        }

        protected bool ParseTimeSpan(ref string str, out TimeSpan value)
        {
            int index = 0;

            SkipWhiteSpace(ref str);

            while ((index < str.Length) && (char.IsDigit(str[index]) || (str[index] == ':') || (str[index] == '.')))
                index++;

            if (index == 0)
            {
                value = TimeSpan.Zero;
                return false;
            }

            string timeSpanString = str.Substring(0, index);
            value = ObjectUtilities.GetTimeSpanFromString(timeSpanString, TimeSpan.Zero);
            str = str.Substring(index);

            return true;
        }

        protected bool ParseToken(ref string str, string token)
        {
            SkipWhiteSpace(ref str);

            string testString = str.Substring(0, token.Length);

            if (testString == token)
            {
                str = str.Substring(token.Length);
                return true;
            }

            return false;
        }

        private static char[] _CommaOrRParen = {',', ')'};

        protected bool ParseMediaRuns(ref string str, out List<MediaRun> mediaRuns)
        {
            mediaRuns = null;

            SkipWhiteSpace(ref str);

            while (str.StartsWith(","))
            {
                str = str.Substring(1);

                if (!str.ToLower().StartsWith("mediarun("))
                    return false;

                str = str.Substring(9);
                string key, file, storageStateString, mediaItemKey, languageMediaItemKey;
                TimeSpan start, stop;
                if (!ParseQuotedString(ref str, out key))
                    return false;
                if (!ParseToken(ref str, ","))
                    return false;
                if (!ParseQuotedString(ref str, out file))
                    return false;
                if (!ParseToken(ref str, ","))
                    return false;
                if (!ParseTimeSpan(ref str, out start))
                    return false;
                if (!ParseToken(ref str, ","))
                    return false;
                if (!ParseTimeSpan(ref str, out stop))
                    return false;
                if (!ParseToken(ref str, ","))
                    return false;
                if (!ParseDelimitedString(ref str, _CommaOrRParen, out storageStateString))
                    return false;
                if (!ParseToken(ref str, ","))
                    return false;
                if (!ParseQuotedString(ref str, out mediaItemKey))
                    return false;
                if (!ParseToken(ref str, ","))
                    return false;
                if (!ParseQuotedString(ref str, out languageMediaItemKey))
                    return false;
                if (!ParseToken(ref str, ")"))
                    return false;
                MediaRun mediaRun = new MediaRun(key, file, mediaItemKey, languageMediaItemKey,
                    UserRecord.UserName, start, stop - start);
                mediaRun.StorageState = ApplicationData.GetStorageStateFromString(storageStateString);

                if (mediaRuns == null)
                    mediaRuns = new List<MediaRun>();

                mediaRuns.Add(mediaRun);
                SkipWhiteSpace(ref str);

                if ((str.Length == 0) || (str[0] != ','))
                    return true;

                str = str.Substring(1);
                SkipWhiteSpace(ref str);
            }

            return true;
        }

        protected void SkipWhiteSpace(ref string str)
        {
            while (str.Length != 0)
            {
                char chr = str[0];

                if (!char.IsWhiteSpace(chr))
                    return;

                str = str.Substring(1);
            }
        }

        public void ScanString(string pattern, string str, List<MultiLanguageItem> multiLanguageItems,
            Dictionary<string, string> tags)
        {
            str = ScanSubstitutionCheck(str);

            int patternCount = pattern.Length;
            int strCount = str.Length;
            int patternIndex;
            int nextPatternIndex;
            int tmpIndex;
            int strIndex = 0;
            MultiLanguageItem multiLanguageItem = null;
            string useLabel = null;
            string label;
            string value;
            int doubleQuoteCount = 0;

            if (tags != null)
                tags.Clear();

            for (patternIndex = 0; patternIndex < patternCount; patternIndex = nextPatternIndex)
            {
                char patternChr = EscapedChar(pattern, patternIndex, patternCount, out nextPatternIndex);
                char nextPatternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                if ((patternChr == '%') && (nextPatternChr == '{'))
                {
                    int i;

                    for (i = tmpIndex; i < patternCount; i++)
                    {
                        if (pattern[i] == '}')
                            break;
                    }

                    string control = pattern.Substring(tmpIndex, i - tmpIndex);
                    int controlIndex = ObjectUtilities.GetIntegerFromStringEnd(control, 0);
                    control = ObjectUtilities.RemoveNumberFromStringEnd(control);
                    int controlLength = control.Length;
                    nextPatternIndex = i + 1;
                    patternChr = EscapedChar(pattern, nextPatternIndex, patternCount, out tmpIndex);

                    for (i = 0; i < controlLength; i++)
                    {
                        if (!Char.IsDigit(control[i]))
                            break;
                    }

                    if (i != 0)
                        controlLength = Convert.ToInt32(control.Substring(0, i));
                    else
                        controlLength = 0;

                    string descriptorName = null;
                    string mediaDescriptorName = null;
                    int descriptorIndex = 0;
                    int mediaDescriptorIndex = 0;
                    string mediaDescriptorKey = null;
                    string speakerNameDescriptorName = null;
                    int speakerNameDescriptorIndex = 0;

                    switch (control)
                    {
                        case "t":
                        case "ta":
                            descriptorName = "Target";
                            descriptorIndex = controlIndex;
                            break;
                        case "h":
                        case "ha":
                            descriptorName = "Host";
                            descriptorIndex = controlIndex;
                            break;
                        case "m":
                        case "mt":
                        case "mta":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Audio";
                            break;
                        case "mh":
                        case "mha":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Audio";
                            break;
                        case "mtp":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Picture";
                            break;
                        case "mhp":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "Picture";
                            break;
                        case "mtt":
                            mediaDescriptorName = "Target";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "TimeTag";
                            break;
                        case "mht":
                            mediaDescriptorName = "Host";
                            mediaDescriptorIndex = controlIndex;
                            mediaDescriptorKey = "TimeTag";
                            break;
                        case "n":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = controlIndex;
                            break;
                        case "nt":
                            speakerNameDescriptorName = "Target";
                            speakerNameDescriptorIndex = controlIndex;
                            break;
                        case "nh":
                            speakerNameDescriptorName = "Host";
                            speakerNameDescriptorIndex = controlIndex;
                            break;
                        case "d":
                        case "o":
                            for (i = strIndex; i < strCount; i++)
                            {
                                if (!Char.IsDigit(str[i]))
                                    break;
                            }
                            strIndex = i;
                            break;
                        case "s":
                        case "title":
                        case "description":
                        case "label":
                        case "contentType":
                        case "contentSubType":
                        case "node":
                        case "tag":
                            if (controlLength == 0)
                            {
                                for (i = strIndex; i < strCount; i++)
                                {
                                    if (str[i] == patternChr)
                                        break;
                                }
                                if (!tags.TryGetValue(control, out label))
                                    tags.Add(control, label = str.Substring(strIndex, i - strIndex));
                                strIndex = i;
                            }
                            else
                            {
                                if (strIndex + controlLength > strCount)
                                    controlLength = strCount - strIndex;
                                tags.Add(control, label = str.Substring(strIndex, i - strIndex));
                                strIndex += controlLength;
                            }
                            if (control == "label")
                            {
                                useLabel = label;
                                multiLanguageItem.Key = label;
                            }
                            break;
                        case "characters":
                        case "words":
                        case "sentences":
                        case "expansion":
                            useLabel = control;
                            useLabel = Char.ToUpper(useLabel[0]).ToString() + control.Substring(1);
                            break;
                        default:
                            if (String.IsNullOrEmpty(Error))
                                Error = FormatErrorPrefix();
                            Error = Error + "\nUnknown %() control: " + control.ToString();
                            break;
                    }

                    if ((descriptorName != null) || (mediaDescriptorName != null))
                    {
                        if (useLabel == null)
                            useLabel = Label;

                        multiLanguageItem = multiLanguageItems.FirstOrDefault(x => x.MatchKey(useLabel));

                        if (multiLanguageItem == null)
                            multiLanguageItems.Add(multiLanguageItem = new MultiLanguageItem(useLabel, LanguageDescriptors));

                        string name = (descriptorName != null ? descriptorName : mediaDescriptorName);
                        descriptorIndex = (descriptorName != null ? descriptorIndex : mediaDescriptorIndex);

                        for (i = strIndex; i < strCount;)
                        {
                            if (str[i] == patternChr)
                                goto skip;

                            switch (str[i])
                            {
                                case '(':
                                    SkipToChar(str, ')', patternChr, i, strCount, out i);
                                    break;
                                case '"':
                                    if ((doubleQuoteCount & 1) == 0)
                                        SkipToChar(str, '"', patternChr, i, strCount, out i);
                                    else
                                        goto skip;
                                    break;
                                case '<':
                                    SkipToChar(str, '>', patternChr, i, strCount, out i);
                                    break;
                                case '[':
                                    SkipToChar(str, ']', patternChr, i, strCount, out i);
                                    break;
                                default:
                                    i++;
                                    break;
                            }
                        }

                        skip:
                        value = str.Substring(strIndex, i - strIndex).Trim();
                        strIndex = i;
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && ((ld.Name == descriptorName) || (ld.Name == mediaDescriptorName)))
                                {
                                    if (index == descriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => ((x.Name == descriptorName) || (x.Name == mediaDescriptorName)) && x.Used && (x.LanguageID != null));
                        }

                        if (languageDescriptor != null)
                        {
                            LanguageItem languageItem = multiLanguageItem.LanguageItem(languageDescriptor.LanguageID);

                            if (languageItem == null)
                            {
                                languageItem = new LanguageItem(null, languageDescriptor.LanguageID, "");
                                multiLanguageItem.Add(languageItem);
                            }

                            if (descriptorName != null)
                            {
                                value = TextUtilities.StripHtml(value);
                                if (!languageItem.HasText())
                                    languageItem.Text = value;
                                if (languageItem.Text != value)
                                {
                                    if (!languageItem.Text.EndsWith(";" + value) &&
                                            !languageItem.Text.StartsWith(value + ";") &&
                                            !languageItem.Text.Contains(";" + value + ";"))
                                        languageItem.Text += ";" + value;
                                }
                                if (languageItem.HasSentenceRuns())
                                {
                                    TextRun sentenceRun = languageItem.SentenceRuns[0];
                                    sentenceRun.Start = 0;
                                    sentenceRun.Length = value.Length;
                                }
                            }
                            ScanForMedia(multiLanguageItem, languageItem, value, mediaDescriptorKey);
                        }
                    }

                    if (speakerNameDescriptorName != null)
                    {
                        LanguageDescriptor languageDescriptor = null;

                        if (LanguageDescriptors != null)
                        {
                            int index = 0;
                            foreach (LanguageDescriptor ld in LanguageDescriptors)
                            {
                                if (ld.Used && (ld.LanguageID != null) && (ld.Name == speakerNameDescriptorName))
                                {
                                    if (index == speakerNameDescriptorIndex)
                                        languageDescriptor = ld;

                                    index++;
                                }
                            }
                            if (languageDescriptor == null)
                                languageDescriptor = LanguageDescriptors.FirstOrDefault(x => (x.Name == speakerNameDescriptorName) && x.Used && (x.LanguageID != null));
                        }

                        if (languageDescriptor != null)
                        {
                            if (controlLength == 0)
                            {
                                for (i = strIndex; i < strCount; i++)
                                {
                                    if (str[i] == patternChr)
                                        break;
                                }
                                if (!tags.TryGetValue(control, out label))
                                {
                                    tags.Add("n", str.Substring(strIndex, i - strIndex));
                                    tags.Add("nl", control);
                                }
                                strIndex = i;
                            }
                            else
                            {
                                if (strIndex + controlLength > strCount)
                                    controlLength = strCount - strIndex;
                                tags.Add("n", str.Substring(strIndex, i - strIndex));
                                tags.Add("nl", control);
                                strIndex += controlLength;
                            }
                        }
                    }
                }
                else
                {
                    if (strIndex >= strCount)
                        break;

                    char strChr = str[strIndex];

                    if (strChr != patternChr)
                    {
                        if (String.IsNullOrEmpty(Error))
                            Error = FormatErrorPrefix();

                        Error = Error + "\nPattern mismatch, line " + ItemIndex.ToString() + ", at " + strIndex.ToString();

                        if (str.Length > 132)
                        {
                            if (strIndex > 120)
                            {
                                int start = strIndex - 40;
                                int end = strIndex + 40;

                                if (end > str.Length)
                                    end = str.Length;

                                str = "..." + str.Substring(start, end - start) + "...";

                                Error = Error += " (" + (strIndex - start).ToString() + " here)";
                            }
                            else
                                str = str.Substring(0, 132) + "...";
                        }

                        Error = Error + ": " + TextUtilities.HtmlEncode(str);
                        break;
                    }

                    strIndex++;

                    switch (strChr)
                    {
                        case '"':
                            doubleQuoteCount++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public string ScanSubstitutionCheck(string str)
        {
#if false
            if (String.IsNullOrEmpty(str))
                return str;

            if (str.Contains("$"))
            {
                LanguageID uiLanguageID = UserProfile.UILanguageID;
                int targetLanguageCount = (TargetLanguageIDs == null ? 0 : TargetLanguageIDs.Count());
                int hostLanguageCount = (HostLanguageIDs == null ? 0 : HostLanguageIDs.Count());
                string defaultLanguageName;
                string languageName;

                if (targetLanguageCount != 0)
                {
                    defaultLanguageName = TargetLanguageIDs.Last().LanguageName(uiLanguageID);
                    if (targetLanguageCount > 3)
                        languageName = TargetLanguageIDs[3].LanguageName(uiLanguageID);
                    else
                        languageName = defaultLanguageName;
                    str = str.Replace("$Target3", languageName);
                    if (targetLanguageCount > 2)
                        languageName = TargetLanguageIDs[2].LanguageName(uiLanguageID);
                    else
                        languageName = defaultLanguageName;
                    str = str.Replace("$Target2", languageName);
                    if (targetLanguageCount > 1)
                        languageName = TargetLanguageIDs[1].LanguageName(uiLanguageID);
                    else
                        languageName = defaultLanguageName;
                    str = str.Replace("$Target1", languageName);
                    languageName = TargetLanguageIDs[0].LanguageName(uiLanguageID);
                    str = str.Replace("$Target0", languageName);
                    str = str.Replace("$Target", languageName);
                }

                if (hostLanguageCount != 0)
                {
                    defaultLanguageName = HostLanguageIDs.Last().LanguageName(uiLanguageID);
                    if (hostLanguageCount > 3)
                        languageName = HostLanguageIDs[3].LanguageName(uiLanguageID);
                    else
                        languageName = defaultLanguageName;
                    str = str.Replace("$Host3", languageName);
                    if (hostLanguageCount > 2)
                        languageName = HostLanguageIDs[2].LanguageName(uiLanguageID);
                    else
                        languageName = defaultLanguageName;
                    str = str.Replace("$Host2", languageName);
                    if (hostLanguageCount > 1)
                        languageName = HostLanguageIDs[1].LanguageName(uiLanguageID);
                    else
                        languageName = defaultLanguageName;
                    str = str.Replace("$Host1", languageName);
                    languageName = HostLanguageIDs[0].LanguageName(uiLanguageID);
                    str = str.Replace("$Host0", languageName);
                    str = str.Replace("$Host", languageName);
                }

                str = str.Replace("$UI", uiLanguageID.LanguageName(uiLanguageID));
            }
#endif

            return str;
        }

        public static char[] ColonSep = { ':' };

        public void ScanForMedia(MultiLanguageItem multiLanguageItem, LanguageItem languageItem, string value, string key)
        {
            int c = value.Length;
            int i, e;
            MediaRun mediarun = null;
            TextRun sentenceRun = (languageItem.HasSentenceRuns() ? languageItem.SentenceRuns[0] : null);
            string mediaFilePath = "";
            int start = -1, end = -1;

            for (i = 0; i < c; i++)
            {
                switch (value[i])
                {
                    case '<':
                        start = i;
                        try
                        {
                            SkipToChar(value, '>', '\0', i , c, out e);
                            end = e;
                            string elementText = value.Substring(i);
                            i = e - 1;
                            XElement element = XElement.Parse(elementText, LoadOptions.None);
                            if (element.Name.LocalName.ToLower() == "img")
                            {
                                if (element.HasAttributes)
                                {
                                    XAttribute srcAttribute = element.Attribute("src");

                                    if (srcAttribute != null)
                                    {
                                        string src = srcAttribute.Value;
                                        mediaFilePath = GetMediaFilePath(src);
                                        mediarun = new MediaRun("Picture", mediaFilePath, Owner);
                                        if (sentenceRun == null)
                                        {
                                            sentenceRun = new TextRun(0, languageItem.Text.Length, new List<MediaRun>(1) { mediarun });
                                            if (languageItem.SentenceRuns == null)
                                                languageItem.SentenceRuns = new List<TextRun>(1) { sentenceRun };
                                        }
                                        else if (sentenceRun.MediaRuns == null)
                                            sentenceRun.MediaRuns = new List<MediaRun>(1) { mediarun };
                                        else
                                            sentenceRun.MediaRuns.Add(mediarun);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    case '[':
                        start = i;
                        try
                        {
                            SkipToChar(value, ']', '\0', i, c, out e);
                            end = e;
                            string inner = value.Substring(i + 1, (e - i) - 2);
                            i = e - 1;
                            string[] parts = inner.Split(ColonSep);
                            string tag = "";
                            if (parts.Count() >= 1)
                                tag = parts[0];
                            if (parts.Count() >= 2)
                                mediaFilePath = parts[1];
                            if (!String.IsNullOrEmpty(mediaFilePath))
                                mediaFilePath = GetMediaFilePath(mediaFilePath);
                            switch (tag)
                            {
                                case "sound":
                                    key = "Audio";
                                    break;
                                case "image":
                                    key = "Picture";
                                    break;
                                default:
                                    break;
                            }
                            mediarun = new MediaRun(key, mediaFilePath, Owner);
                            if (sentenceRun == null)
                            {
                                sentenceRun = new TextRun(0, languageItem.Text.Length, new List<MediaRun>(1) { mediarun });
                                if (languageItem.SentenceRuns == null)
                                    languageItem.SentenceRuns = new List<TextRun>(1) { sentenceRun };
                            }
                            else if (sentenceRun.MediaRuns == null)
                                sentenceRun.MediaRuns = new List<MediaRun>(1) { mediarun };
                            else
                                sentenceRun.MediaRuns.Add(mediarun);
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    default:
                        break;
                }
            }

            if ((start != -1) && (value == languageItem.Text))
            {
                string text = value.Remove(start, end - start);
                languageItem.Text = text;

                if (sentenceRun != null)
                    sentenceRun.Stop = text.Length;
            }

            if (!String.IsNullOrEmpty(key) && (start == -1))
            {
                switch (key)
                {
                    default:
                    case "Audio":
                    case "Video":
                    case "Picture":
                        if (languageItem.HasSentenceRuns())
                        {
                            sentenceRun = languageItem.GetSentenceRun(0);
                            MediaRun mediaRun = sentenceRun.GetMediaRunNonReference(key);
                            if (mediaRun == null)
                            {
                                mediaRun = new MediaRun(key, value, Owner);
                                sentenceRun.AddMediaRun(mediaRun);
                            }
                            else
                            {
                                mediaRun.FileName = value;
                                sentenceRun.AddMediaRun(mediaRun);
                            }
                        }
                        else
                        {
                            MediaRun mediaRun = new MediaRun(key, value, Owner);
                            languageItem.AddSentenceRun(new TextRun(languageItem, mediaRun));
                        }
                        break;
                    case "TimeTag":
                        {
                            TimeSpan startTime = TimeSpan.Zero;
                            TimeSpan length = TimeSpan.Zero;
                            if (!ScanTimeTag(value, ref startTime, ref length))
                                break;
                            if (String.IsNullOrEmpty(MediaItemKey) || String.IsNullOrEmpty(LanguageMediaItemKey))
                            {
                                if (!GetDefaultMediaReference(multiLanguageItem, languageItem))
                                    break;
                            }
                            if (languageItem.HasSentenceRuns())
                            {
                                sentenceRun = languageItem.GetSentenceRun(0);
                                MediaRun mediaRun = sentenceRun.GetMediaRunNonReference(MediaRunKey);
                                if (mediaRun == null)
                                {
                                    mediaRun = new MediaRun(
                                        MediaRunKey,
                                        null,
                                        MediaItemKey,
                                        LanguageMediaItemKey,
                                        Owner,
                                        startTime,
                                        length);
                                    sentenceRun.AddMediaRun(mediaRun);
                                }
                                else
                                {
                                    mediaRun.Start = startTime;
                                    mediaRun.Length = length;
                                    sentenceRun.AddMediaRun(mediaRun);
                                }
                            }
                            else
                            {
                                MediaRun mediaRun = new MediaRun(
                                    MediaRunKey,
                                    null,
                                    MediaItemKey,
                                    LanguageMediaItemKey,
                                    Owner,
                                    startTime,
                                    length);
                                languageItem.AddSentenceRun(new TextRun(languageItem, mediaRun));
                            }
                        }
                        break;
                }
            }

            if (sentenceRun != null)
            {
                SaveAlternateMedia(multiLanguageItem, languageItem, sentenceRun);

                if (HostLanguageIDs.Contains(languageItem.LanguageID))
                {
                    LanguageItem targetLanguageItem = multiLanguageItem.LanguageItem(TargetLanguageID);

                    if (targetLanguageItem != null)
                    {
                        TextRun targetTextRun = targetLanguageItem.GetSentenceRun(0);

                        if ((targetTextRun == null) || !targetTextRun.HasAudioVideo())
                        {
                            if (targetTextRun == null)
                            {
                                targetTextRun = new TextRun(0, targetLanguageItem.Text.Length, null);

                                if (targetLanguageItem.SentenceRuns == null)
                                    targetLanguageItem.SentenceRuns = new List<TextRun>(1) { targetTextRun };
                                else
                                    targetLanguageItem.SentenceRuns.Add(targetTextRun);
                            }

                            if (sentenceRun.MediaRuns != null)
                            {
                                if (targetTextRun.MediaRuns == null)
                                    targetTextRun.MediaRuns = new List<MediaRun>();

                                foreach (MediaRun hostMediaRun in sentenceRun.MediaRuns)
                                    targetTextRun.MediaRuns.Add(new MediaRun(hostMediaRun));

                                SaveAlternateMedia(multiLanguageItem, targetLanguageItem, targetTextRun);
                            }
                        }
                    }
                }
            }
        }

        protected bool ScanTimeTag(
            string value,
            ref TimeSpan startTime,
            ref TimeSpan length)
        {
            string tmp;
            bool returnValue = true;

            if (IsPatternMatch(value, "dd:dd:dd"))
            {
                startTime = ObjectUtilities.GetTimeSpanFromString(value, startTime);
                // Length done later.
            }
            else if (IsPatternMatch(value, "dd:dd"))
            {
                startTime = ObjectUtilities.GetTimeSpanFromString("00:" + value, startTime);
                // Length done later.
            }
            else if (IsPatternMatch(value, "dd:dd:dd dd:dd:dd"))
            {
                string[] parts = value.Split(LanguageLookup.Space, StringSplitOptions.None);

                startTime = ObjectUtilities.GetTimeSpanFromString(parts[0], startTime);
                length = ObjectUtilities.GetTimeSpanFromString(parts[1], length);
            }
            else if (IsPatternMatch(value, "dd:dd dd:dd"))
            {
                string[] parts = value.Split(LanguageLookup.Space, StringSplitOptions.None);

                startTime = ObjectUtilities.GetTimeSpanFromString("00:" + parts[0], startTime);
                length = ObjectUtilities.GetTimeSpanFromString("00:" + parts[1], length);
            }
            else if (value.EndsWith("s") && ObjectUtilities.IsNumberString(tmp = value.Substring(0, value.Length - 1)))
            {
                startTime = TimeSpan.FromSeconds(ObjectUtilities.GetIntegerFromString(tmp, 0));
                // Length done later.
            }
            else
            {
                PutError("Unexpected time tag format: " + value);
                returnValue = false;
            }

            return returnValue;
        }

        protected bool IsPatternMatch(string value, string pattern)
        {
            int index;
            int count = pattern.Length;

            if (value.Length != pattern.Length)
                return false;

            for (index = 0; index < count; index++)
            {
                switch (pattern[index])
                {
                    case 'd':
                        if (!char.IsDigit(value[index]))
                            return false;
                        break;
                    default:
                        if (value[index] != pattern[index])
                            return false;
                        break;
                }
            }

            return true;
        }

        protected bool GetDefaultMediaReference(
            MultiLanguageItem multiLanguageItem,
            LanguageItem languageItem)
        {
            BaseObjectNode node = null;

            if (Component is BaseObjectContent)
            {
                BaseObjectContent content = GetComponent<BaseObjectContent>();
                node = content.Node;
            }
            else if (Component is ContentStudyList)
                node = GetComponent<ContentStudyList>().Node;
            else if (Component is ContentMediaItem)
                node = GetComponent<ContentMediaItem>().Node;
            else if (Component is ToolStudyList)
            {
                ToolStudyItem toolStudyItem = GetComponent<ToolStudyList>().GetToolStudyItemIndexed(ItemIndex);

                if (toolStudyItem != null)
                    node = toolStudyItem.StudyItem.Node;
            }

            if (node == null)
                return false;

            BaseObjectContent mediaContent = null;

            if (String.IsNullOrEmpty(MediaItemKey))
            {
                List<BaseObjectContent> contentList = node.GetContentWithStorageClass(ContentClassType.MediaItem);

                if ((contentList == null) || (contentList.Count() == 0))
                    return false;

                mediaContent = contentList.First();

                MediaItemKey = mediaContent.KeyString;
            }

            if (String.IsNullOrEmpty(LanguageMediaItemKey))
                LanguageMediaItemKey = languageItem.LanguageID.MediaLanguageCode();

            if (String.IsNullOrEmpty(MediaRunKey))
            {
                switch (mediaContent.ContentType)
                {
                    case "Audio":
                        MediaRunKey = "Audio";
                        break;
                    case "Video":
                        MediaRunKey = "Video";
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        public virtual string GetMediaFilePath(string key)
        {
            return key;
        }

        public void SaveAlternateMedia(MultiLanguageItem mediaMultiLanguageItem, LanguageItem mediaLanguageItem, TextRun currentTextRun)
        {
            LanguageID targetLanguageID = mediaLanguageItem.LanguageID;
            List<LanguageID> alternateLanguageIDs = LanguageLookup.GetAlternateLanguageIDs(targetLanguageID);
            int textRunIndex = mediaLanguageItem.SentenceRuns.IndexOf(currentTextRun);
            List<string> names = new List<string>() { mediaLanguageItem.GetRunText(currentTextRun) };

            if ((alternateLanguageIDs != null) && (textRunIndex != -1))
            {
                foreach (LanguageID alternateLanguageID in alternateLanguageIDs)
                {
                    LanguageItem languageItem = mediaMultiLanguageItem.LanguageItem(alternateLanguageID);

                    if (languageItem == null)
                        continue;

                    TextRun textRun = null;

                    if (languageItem.HasSentenceRuns())
                        textRun = languageItem.SentenceRuns[0];

                    if (textRun == null)
                    {
                        textRun = new TextRun(0, languageItem.Text.Length, new List<MediaRun>(1));
                        if (languageItem.SentenceRuns == null)
                            languageItem.SentenceRuns = new List<TextRun>(1) { textRun };
                    }
                    else if (textRun.MediaRuns == null)
                        textRun.MediaRuns = new List<MediaRun>(1);

                    foreach (MediaRun mediaRun in currentTextRun.MediaRuns)
                    {
                        if (textRun.MediaRuns.FirstOrDefault(XAttribute => XAttribute.MatchKey(mediaRun.Key)) == null)
                        {
                            MediaRun newMediaRun = new MediaRun(mediaRun);
                            textRun.MediaRuns.Add(newMediaRun);
                        }
                    }
                }
            }
        }

        protected void FixupMediaRuns()
        {
            if (ReadStudyLists != null)
            {
                foreach (ContentStudyList studyList in ReadStudyLists)
                    FixupMediaRuns(studyList.StudyItems);
            }
            else if (Component != null)
            {
                ContentStudyList studyList = null;

                if (Component is BaseObjectContent)
                    studyList = GetComponent<BaseObjectContent>().ContentStorageStudyList;
                else if (Component is ContentStudyList)
                    studyList = GetComponent<ContentStudyList>();

                if (studyList != null)
                    FixupMediaRuns(studyList.StudyItems);
            }
        }

        protected void FixupMediaRuns(List<MultiLanguageItem> studyItems)
        {
            if (!String.IsNullOrEmpty(LanguageMediaItemKey) &&
                !String.IsNullOrEmpty(MediaRunKey) &&
                (studyItems != null))
            {
                int studyItemCount = studyItems.Count();
                LanguageID rootLanguageID = LanguageLookup.GetLanguageIDNoAdd(LanguageMediaItemKey);
                List<LanguageID> languageIDs = LanguageLookup.GetFamilyLanguageIDs(rootLanguageID);
                for (int index = 0; index < studyItemCount; index++)
                {
                    MultiLanguageItem studyItem = studyItems[index];
                    if (studyItem != null)
                    {
                        foreach (LanguageID languageID in languageIDs)
                        {
                            if (!studyItem.HasLanguageID(languageID))
                                continue;
                            MediaRun mediaRun = studyItem.GetMediaRun(languageID, 0, MediaRunKey);
                            bool needMediaRun = false;
                            if (mediaRun == null)
                            {
                                mediaRun = studyItem.GetMediaRun(rootLanguageID, 0, MediaRunKey);
                                if (mediaRun != null)
                                    needMediaRun = true;
                            }
                            if (mediaRun != null)
                            {
                                if (mediaRun.Length == TimeSpan.Zero)
                                {
                                    if (index < studyItemCount - 1)
                                    {
                                        MultiLanguageItem nextStudyItem = studyItems[index + 1];
                                        MediaRun nextMediaRun = nextStudyItem.GetMediaRun(languageID, 0, MediaRunKey);

                                        if (nextMediaRun != null)
                                        {
                                            TimeSpan nextStartTime = nextMediaRun.Start;
                                            TimeSpan length = nextStartTime - mediaRun.Start;
                                            mediaRun.Length = length;
                                        }
                                    }
                                    else
                                        mediaRun.Length = TimeSpan.FromSeconds(5);
                                }
                                if (needMediaRun)
                                {
                                    TextRun sentenceRun = studyItem.LanguageItem(languageID).GetSentenceRun(0);
                                    if (sentenceRun != null)
                                    {
                                        MediaRun newMediaRun = new Content.MediaRun(mediaRun);
                                        studyItem.LanguageItem(languageID).GetSentenceRun(0).AddMediaRun(newMediaRun);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static char EscapedChar(string str, int inIndex, int inLength, out int outIndex)
        {
            if (inIndex >= inLength)
            {
                outIndex = inIndex;
                return '\0';
            }

            char outChr = str[inIndex++];
            char nextChr = (inIndex < inLength ? str[inIndex] : '\0');

            if (outChr == '\\')
            {
                switch (nextChr)
                {
                    case 't':
                        outChr = '\t';
                        break;
                    case 'n':
                        outChr = '\n';
                        break;
                    case 'r':
                        outChr = '\r';
                        break;
                    case 'f':
                        outChr = '\f';
                        break;
                    case '\\':
                        outChr = '\\';
                        break;
                    default:
                        inIndex--;
                        break;
                }

                inIndex++;
            }

            outIndex = inIndex;

            return outChr;
        }

        public static string ParseAlphabeticString(string str, int inIndex, int inLength, out int outIndex)
        {
            int index = inIndex;

            if (index >= inLength)
            {
                outIndex = index;
                return String.Empty;
            }

            while (index < inLength)
            {
                char outChr = str[index];

                if (char.IsLetter(outChr))
                    index++;
                else
                    break;
            }

            outIndex = index;

            string returnValue = str.Substring(inIndex, outIndex - inIndex);

            return returnValue;
        }

        public static void SkipToChar(string str, char endChar, char patternChr, int inIndex, int inCount, out int outIndex)
        {
            outIndex = inIndex + 1;
            for (inIndex++; inIndex < inCount; )
            {
                if (str[inIndex] == endChar)
                {
                    inIndex++;
                    break;
                }
                else if (str[inIndex] == patternChr)
                    break;

                switch (str[inIndex++])
                {
                    case '(':
                        SkipToChar(str, ')', patternChr, inIndex, inCount, out inIndex);
                        break;
                    case '"':
                        SkipToChar(str, '"', patternChr, inIndex, inCount, out inIndex);
                        break;
                    case '<':
                        SkipToChar(str, '>', patternChr, inIndex, inCount, out inIndex);
                        break;
                    case '[':
                        SkipToChar(str, ']', patternChr, inIndex, inCount, out inIndex);
                        break;
                    default:
                        break;
                }
            }

            if (inIndex <= inCount)
                outIndex = inIndex;
        }

        // Progress is 0.0f to 1.0f, where 1.0f means complete.
        public override float GetProgress()
        {
            return 1.0f;
        }

        public override string GetProgressMessage()
        {
            string message;

            if (ItemCount == 0)
                message = "Import beginning...";
            else
                message = "Read completed.  " + ItemCount.ToString() + " items created.";

            if (Timer != null)
                message += "  Elapsed time is " + ElapsedTime.ToString();

            return message;
        }

        public static string StudyItemLinePatternHelp = "This is a coded string that will be used to parse a row of text from the input, or format output."
            + " It uses substitutions of the form \"%{c}\" where \"c\" is a control string indicating what is to be substituted.\n"
            + " The control strings supported are:\n\n"
            + "\tt: set target language item.\n"
            + "\tt1-t3: set additional target language items\n"
            + "\th: set host language item\n"
            + "\th1-h3: set additional host language items\n"
            + "\td: skip a number\n"
            + "\ts: skip a string until a match of the next character\n"
            + "\t(num)s: skip (num) characters\n"
            + "\to: output an ordinal number which is incremented for each item\n"
            + "\ttag: for use with courses, or plans to select or set up a node or study list using the tag as the title\n"
            + "\ttitle: output the node content title\n"
            + "\tdescription: output the node content description\n"
            + "\tlabel: input or output the node content label\n"
            + "\tnode: input or output the node title\n"
            + "\tcontentType: input or output the content type name\n"
            + "\tcontentSubType: input or output the content subtype name\n"
            + "\tsentences, words, characters, expansion, exercises: redirects the input to components with these labels\n\n"
            + " For example: Say your target language is Spanish, host language is English, and you have some alternate language,"
            + " and you have rows that look like this:  \"MyLabel: espanol,english (alt),ignore\""
            + " You could use a pattern like: \"MyLabel: %{t},%{h} (%{t1}),%{s}\"";

        public static string DictionaryEntryLinePatternHelp = "This is a coded string that will be used to parse a row of text from the input, or format output."
            + " It uses substitutions of the form \"%{c}\" where \"c\" is a control string indicating what is to be substituted.\n"
            + " The control strings supported are:\n\n"
            + "\tt: set target language item.\n"
            + "\tt1-t3: set additional target language items\n"
            + "\th: set host language item\n"
            + "\th1-h3: set additional host language items\n"
            + "\td: skip a number\n"
            + "\ts: skip a string until a match of the next character\n"
            + "\t(num)s: skip (num) characters\n"
            + "\to: output an ordinal number which is incremented for each item\n"
            + "\tc: set the lexical category, one of:\n"
            + "\t\t"
            + "Unknown, "
            + "Multiple, "
            + "Noun, "
            + "ProperNoun, "
            + "Pronoun, "
            + "Determiner, "
            + "Adjective, "
            + "\n\t\t"
            + "Verb, "
            + "Adverb, "
            + "Preposition, "
            + "Conjunction, "
            + "\n\t\t"
            + "Interjection, "
            + "Particle, "
            + "Article, "
            + "MeasureWord, "
            + "Number, "
            + "Prefix, "
            + "Suffix, "
            + "Abbreviation, "
            + "Acronymn, "
            + "Symbol, "
            + "Phrase, "
            + "PrepositionalPhrase, "
            + "Proverb, "
            + "Contraction, "
            + "Idiom"
            + "\n"
            + "\tcs: extra or language-specific lexical category strings\n"
            + " For example: Say your target language is Spanish, host language is English, and you have some alternate language,"
            + " and you have rows that look like this:  \"MyLabel: espanol,english (alt),ignore\""
            + " You could use a pattern like: \"MyLabel: %{t},%{h} (%{t1}),%{s}\"";

        public static string StudyItemBlockPatternHelp = "This is a coded string that will be used to parse the language blocks from the input, or format output."
            + " It uses substitutions of the form \"%c\" or \"%c{args}\" where \"c\" is a control string indicating what is to be substituted,"
            + " and \"arg\" is a possible argument giving further information.\n"
            + " The control strings supported are:\n \n"
            + "\tp: input or output a language block of text, where arg is a line pattern for each line in the block."
            + " It uses substitutions of the form \"%{c}\" where \"c\" is a control string indicating what is to be substituted.\n"
            + " The control strings supported are:\n \n"
                + "\t\tt: set target language item.\n"
                + "\t\tt1-t3: set additional target language items\n"
                + "\t\th: set host language item\n"
                + "\t\th1-h3: set additional host language items\n"
                + "\t\td: skip a number\n"
                + "\t\ts: skip a string until a match of the next character\n"
                + "\t\t(num)s: skip (num) characters\n"
                + "\t\to: output an ordinal number which is incremented for each item\n"
                + "\t\ttag: for use with courses, or plans to select or set up a node or study list using the tag as the title\n"
                + "\t\ttitle: output the node content title\n"
                + "\t\tdescription: output the node content description\n"
                + "\t\tlabel: input or output the node content label\n"
                + "\t\tnode: input or output the node title\n"
                + "\t\tcontentType: input or output the content type name\n"
                + "\t\tcontentSubType: input or output the content subtype name\n \n"
            + "\tlt: input or output target language name\n"
            + "\tlt1-lt3: input or output additional target language names\n"
            + "\tlh: input or output host language item\n"
            + "\tlh1-lh3: input or output additional host language names\n"
            ;

        public static string DictionaryEntryBlockPatternHelp = "This is a coded string that will be used to parse the language blocks from the input, or format output."
            + " It uses substitutions of the form \"%c\" or \"%c{args}\" where \"c\" is a control string indicating what is to be substituted,"
            + " and \"arg\" is a possible argument giving further information.\n"
            + " The control strings supported are:\n \n"
            + "\tp: input or output a language block of text, where arg is a line pattern for each line in the block."
            + " It uses substitutions of the form \"%{c}\" where \"c\" is a control string indicating what is to be substituted.\n"
            + " The control strings supported are:\n \n"
                + "\t\tt: set target language item.\n"
                + "\t\tt1-t3: set additional target language items\n"
                + "\t\th: set host language item\n"
                + "\t\th1-h3: set additional host language items\n"
                + "\t\td: skip a number\n"
                + "\t\ts: skip a string until a match of the next character\n"
                + "\t\t(num)s: skip (num) characters\n"
                + "\t\to: output an ordinal number which is incremented for each item\n"
                + "\tc: set the lexical category, one of:\n"
                + "\t\t\t"
                + "Unknown, "
                + "Multiple, "
                + "Noun, "
                + "ProperNoun, "
                + "Pronoun, "
                + "Determiner, "
                + "Adjective, "
                + "\n\t\t\t"
                + "Verb, "
                + "Adverb, "
                + "Preposition, "
                + "Conjunction, "
                + "\n\t\t\t"
                + "Interjection, "
                + "Particle, "
                + "Article, "
                + "MeasureWord, "
                + "Number, "
                + "Prefix, "
                + "Suffix, "
                + "Abbreviation, "
                + "Acronymn, "
                + "Symbol, "
                + "Phrase, "
                + "PrepositionalPhrase, "
                + "Proverb, "
                + "Contraction, "
                + "Idiom"
                + "\n"
                + "\t\tcs: extra or language-specific lexical category strings\n"
            + "\tlt: input or output target language name\n"
            + "\tlt1-lt3: input or output additional target language names\n"
            + "\tlh: input or output host language item\n"
            + "\tlh1-lh3: input or output additional host language names\n"
            ;

        public static string MediaItemKeyHelp = "Enter the media item key the text associates with.";
        public static string LanguageMediaItemKeyHelp = "Enter the language media item key the text associates with (i.e. \"ja\" for Japanese).";
        public static string MediaRunKeyHelp = "Select the media run key.";

        public string LinePatternHelp
        {
            get
            {
                string help;

                switch (TargetType)
                {
                    case "DictionaryEntry":
                        help = DictionaryEntryLinePatternHelp;
                        break;
                    default:
                        help = StudyItemLinePatternHelp;
                        break;
                }

                return help;
            }
        }

        public string BlockPatternHelp
        {
            get
            {
                string help;

                switch (TargetType)
                {
                    case "DictionaryEntry":
                        help = DictionaryEntryBlockPatternHelp;
                        break;
                    default:
                        help = StudyItemBlockPatternHelp;
                        break;
                }

                return help;
            }
        }

        public string PatternHelp
        {
            get
            {
                if (IsLineArrangement)
                    return LinePatternHelp;
                else
                    return BlockPatternHelp;
            }
        }

        public static string UseCommentsHelp = "Use or expect comment or directive lines beginning with the comment prefix.  These directives assist the importer in processing the input.";
        public static string CommentPrefixHelp = "This is the character sequence that will mark a line as a comment or directive.";
        public static string OrdinalHelp = "Via the \"%{o}\" pattern control, an ordinal can be output starting from this initial value, and incremented.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            DeleteBeforeImport = GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", DeleteBeforeImportHelp, null, null);

            Pattern = GetArgumentDefaulted("Pattern", "string", "rw", DefaultPattern, "Pattern", PatternHelp);
            UseComments = GetFlagArgumentDefaulted("UseComments", "flag", "rw", UseComments, "Use comments", UseCommentsHelp,
                null, null);
            CommentPrefix = GetArgumentDefaulted("CommentPrefix", "string", "rw", CommentPrefix, "Comment prefix",
                CommentPrefixHelp);
            IsExcludePrior = GetFlagArgumentDefaulted("IsExcludePrior", "flag", "r", IsExcludePrior, "Exclude prior items", IsExcludePriorHelp,
                null, null);
            IsDoMerge = GetFlagArgumentDefaulted("IsDoMerge", "flag", "r", IsDoMerge, "Merge", IsDoMergeHelp,
                null, null);
            IsTranslateMissingItems = GetFlagArgumentDefaulted("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);

            switch (TargetType)
            {
                case "BaseObjectNode":
                case "BaseObjectNodeTree":
                    SubDivide = GetFlagArgumentDefaulted("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount",
                        },
                        null);
                    SubDivideToStudyListsOnly = GetFlagArgumentDefaulted("SubDivideToStudyListsOnly", "flag", "r",
                        SubDivideToStudyListsOnly, "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        null);
                    TitlePrefix = GetArgumentDefaulted("TitlePrefix", "string", "r", "Default", "Title prefix", TitlePrefixHelp);
                    DefaultContentType = GetArgumentDefaulted("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp);
                    DefaultContentSubType = GetArgumentDefaulted("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp);
                    MasterName = GetMasterListArgumentDefaulted("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    StudyItemSubDivideCount = GetIntegerArgumentDefaulted("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Study items per leaf", StudyItemSubDivideCountHelp);
                    MinorSubDivideCount = GetIntegerArgumentDefaulted("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    MajorSubDivideCount = GetIntegerArgumentDefaulted("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                case "BaseObjectContent":
                    Ordinal = GetIntegerArgumentDefaulted("Ordinal", "integer", "w", Ordinal, "Ordinal", OrdinalHelp);
                    SubDivide = GetFlagArgumentDefaulted("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount",
                        },
                        null);
                    SubDivideToStudyListsOnly = GetFlagArgumentDefaulted("SubDivideToStudyListsOnly", "flag", "r",
                        SubDivideToStudyListsOnly, "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        new List<string>()
                        {
                            "TitlePrefix",
                            "DefaultContentType",
                            "DefaultContentSubType",
                            "Label"
                        });
                    TitlePrefix = GetArgumentDefaulted("TitlePrefix", "string", "r", "Default", "Title prefix", TitlePrefixHelp);
                    DefaultContentType = GetArgumentDefaulted("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp);
                    DefaultContentSubType = GetArgumentDefaulted("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp);
                    MasterName = GetMasterListArgumentDefaulted("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    StudyItemSubDivideCount = GetIntegerArgumentDefaulted("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Study items per leaf", StudyItemSubDivideCountHelp);
                    MinorSubDivideCount = GetIntegerArgumentDefaulted("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    MajorSubDivideCount = GetIntegerArgumentDefaulted("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                default:
                    break;
            }

            MakeTargetPublic = GetFlagArgumentDefaulted("MakeTargetPublic", "flag", "r",
                MakeTargetPublic, "Make public", MakeTargetPublicHelp, null, null);
            IsFilterDuplicates = GetFlagArgumentDefaulted("IsFilterDuplicates", "flag", "r",
                IsFilterDuplicates, "Filter out duplicates", IsFilterDuplicatesHelp,
                new List<string>(1) { "AnchorLanguages" }, null);
            List<string> anchorLanguageFlagNames = LanguageID.GetLanguageCultureExtensionCodes(UniqueLanguageIDs);
            AnchorLanguageFlags = GetFlagListArgumentDefaulted("AnchorLanguages", "languageflaglist", "r",
                AnchorLanguageFlags, anchorLanguageFlagNames, "Anchor languages", AnchorLanguagesHelp, null, null);

            MediaItemKey = GetArgumentDefaulted("MediaItemKey", "string", "r", MediaItemKey, "MediaItemKey",
                MediaItemKeyHelp);

            LanguageMediaItemKey = GetArgumentDefaulted("LanguageMediaItemKey", "string", "r",
                LanguageMediaItemKey, "LanguageMediaItemKey",
                LanguageMediaItemKeyHelp);

            MediaRunKey = GetStringListArgumentDefaulted("MediaRunKey", "string", "r",
                MediaRunKey, MediaRun.MediaRunKeys, "MediaRunKey",
                MediaRunKeyHelp);

            if (NodeUtilities != null)
                Label = NodeUtilities.GetLabelFromContentType(DefaultContentType, DefaultContentSubType);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            if (String.IsNullOrEmpty(Pattern))
                Pattern = DefaultPattern;

            SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport, "Delete before import",
                DeleteBeforeImportHelp, null, null);

            SetArgument("Pattern", "string", "rw", Pattern, "Pattern", PatternHelp, null, null);
            SetFlagArgument("UseComments", "flag", "rw", UseComments, "Use comments", UseCommentsHelp, null, null);
            SetArgument("CommentPrefix", "string", "rw", CommentPrefix, "Comment prefix", CommentPrefixHelp, null, null);
            SetFlagArgument("IsExcludePrior", "flag", "r", IsExcludePrior, "Exclude prior items", IsExcludePriorHelp,
                null, null);
            SetFlagArgument("IsDoMerge", "flag", "r", IsDoMerge, "Merge", IsDoMergeHelp, null, null);
            SetFlagArgument("IsTranslateMissingItems", "flag", "r", IsTranslateMissingItems, "Translate missing items",
                IsTranslateMissingItemsHelp, null, null);

            switch (TargetType)
            {
                case "BaseObjectNode":
                case "BaseObjectNodeTree":
                    SetFlagArgument("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount",
                        },
                        null);
                    SetFlagArgument("SubDivideToStudyListsOnly", "flag", "r", SubDivideToStudyListsOnly,
                        "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        null);
                    SetArgument("TitlePrefix", "string", "r", TitlePrefix, "Title prefix", TitlePrefixHelp, null, null);
                    SetArgument("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp, null, null);
                    SetArgument("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp, null, null);
                    SetMasterListArgument("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    SetIntegerArgument("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Items per leaf", StudyItemSubDivideCountHelp);
                    SetIntegerArgument("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    SetIntegerArgument("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                case "BaseObjectContent":
                    SetIntegerArgument("Ordinal", "integer", "w", Ordinal, "Ordinal", OrdinalHelp);
                    SetFlagArgument("SubDivide", "flag", "r", SubDivide,
                        "Subdivide items into smaller groups", SubDivideHelp,
                        new List<string>()
                        {
                            "SubDivideToStudyListsOnly",
                            "MasterName",
                            "StudyItemSubDivideCount",
                            "MinorSubDivideCount",
                            "MajorSubDivideCount"
                        },
                        null);
                    SetFlagArgument("SubDivideToStudyListsOnly", "flag", "r", SubDivideToStudyListsOnly,
                        "Subdivide to study lists only", SubDivideToStudyListsOnlyHelp,
                        null,
                        new List<string>()
                        {
                            "TitlePrefix",
                            "DefaultContentType",
                            "DefaultContentSubType"
                        });
                    SetArgument("TitlePrefix", "string", "r", TitlePrefix, "Title prefix", TitlePrefixHelp, null, null);
                    SetArgument("DefaultContentType", "string", "r", DefaultContentType, "Default content type", DefaultContentTypeHelp, null, null);
                    SetArgument("DefaultContentSubType", "string", "r", DefaultContentSubType, "Default content sub-type", DefaultContentSubTypeHelp, null, null);
                    SetMasterListArgument("MasterName", "stringlist", "r", MasterName, "Master name", MasterNameHelp);
                    SetIntegerArgument("StudyItemSubDivideCount", "integer", "r", StudyItemSubDivideCount, "Items per leaf", StudyItemSubDivideCountHelp);
                    SetIntegerArgument("MinorSubDivideCount", "integer", "r", MinorSubDivideCount, "Minor subdivide count", MinorSubDivideCountHelp);
                    SetIntegerArgument("MajorSubDivideCount", "integer", "r", MajorSubDivideCount, "Major subdivide count", MinorSubDivideCountHelp);
                    break;
                default:
                    break;
            }

            SetFlagArgument("MakeTargetPublic", "flag", "r", MakeTargetPublic, "Make targets public",
                MakeTargetPublicHelp, null, null);
            SetFlagArgument("IsFilterDuplicates", "flag", "r",
                IsFilterDuplicates, "Filter out duplicates", IsFilterDuplicatesHelp,
                new List<string>(1) { "AnchorLanguages" }, null);
            List<string> anchorLanguageFlagNames = LanguageID.GetLanguageCultureExtensionCodes(UniqueLanguageIDs);
            SetFlagListArgument("AnchorLanguages", "languageflaglist", "r", AnchorLanguageFlags, anchorLanguageFlagNames, "Anchor languages",
                AnchorLanguagesHelp, null, null);

            MediaItemKey = GetArgumentDefaulted("MediaItemKey", "string", "r", MediaItemKey, "MediaItemKey",
                MediaItemKeyHelp);

            LanguageMediaItemKey = GetArgumentDefaulted("LanguageMediaItemKey", "string", "r",
                LanguageMediaItemKey, "LanguageMediaItemKey",
                LanguageMediaItemKeyHelp);

            MediaRunKey = GetStringListArgumentDefaulted("MediaRunKey", "string", "r",
                MediaRunKey, MediaRun.MediaRunKeys, "MediaRunKey",
                MediaRunKeyHelp);
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            switch (contentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                case "ContentMediaList":
                case "ContentMediaItem":
                case "MultiLanguageItem":
                case "MultiLanguageString":
                case "DictionaryEntry":
                    if (capability == "Support")
                        return true;
                    else if ((importExport == "Export") && (capability == "UseFlags"))
                        return true;
                    else if (capability == "Text")
                        return true;
                    else if (capability == "RecursedStudyItems")
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

        public static new string TypeStringStatic { get { return "Patterned"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
