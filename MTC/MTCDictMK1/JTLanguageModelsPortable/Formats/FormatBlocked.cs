using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatBlocked : FormatComponent
    {
        protected LanguageID _LanguageID;
        protected int _LanguageIndex;
        protected string _SpeakerNameField;
        protected int _SpeakerNameFieldWidth;
        protected List<SpeakerText> _Items;
        public bool UseLanguageLabels { get; set; }
        public bool UseSpeakerNames { get; set; }

        public FormatBlocked(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, TypeStringStatic, userRecord, repositories, translator, languageUtilities)
        {
            SetupDefaults();
        }

        public FormatBlocked(TitledTree tree, string targetName, string targetOwner, List<IBaseXml> targetList,
                List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, targetName, targetOwner, targetList, languageDescriptors, itemIndex, TypeStringStatic, userRecord,
                repositories, translator, languageUtilities)
        {
            SetupDefaults();
        }

        protected virtual void SetupDefaults()
        {
            MimeType = "text/plain";
            UseLanguageLabels = false;
            UseSpeakerNames = true;
        }

        public static string UseLanguageLabelsHelp = "Check this to use language code labels (i.e. \"(en)\" before the English block.";

        public static string UseSpeakerNamesHelp = "Check this to use speaker names.";

        public override void LoadFromArguments()
        {
            if (Arguments == null)
                return;

            base.LoadFromArguments();

            UseLanguageLabels = (GetArgumentDefaulted("UseLanguageLabels", "flag", "rw", "on", "Use language code labels", UseLanguageLabelsHelp) == "on" ? true : false);

            switch (GetTargetName())
            {
                case "Text":
                case "Transcript":
                case "Lesson":
                case "LessonGroup":
                case "Course":
                case "Plan":
                    UseSpeakerNames = (GetArgumentDefaulted("UseSpeakerNames", "flag", "rw", "on", "Use speaker names", UseSpeakerNamesHelp) == "on" ? true : false);
                    break;
                default:
                    break;
            }
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetArgument("UseLanguageLabels", "flag", "rw", (UseLanguageLabels ? "on" : ""), "Use language code labels", UseLanguageLabelsHelp);

            switch (GetTargetName())
            {
                case "Text":
                case "Transcript":
                case "Lesson":
                case "LessonGroup":
                case "Course":
                case "Plan":
                    SetArgument("UseSpeakerNames", "flag", "rw", (UseSpeakerNames ? "on" : ""), "Use speaker names", UseSpeakerNamesHelp);
                    break;
                default:
                    break;
            }
        }

        protected override void ReadObject(StreamReader reader, IBaseXml obj)
        {
            ReadHeaderConfirm(reader, obj);

            if ((obj is LessonComponent) || (obj is FlashList))
            {
                Component = obj;
                if (IsSupportedStatic("Import", GetComponentName(), "Support"))
                {
                    ReadComponent(reader);
                    ComponentIndex = ComponentIndex + 1;
                }
                Component = null;
            }
            else if (obj is Lesson)
            {
                while (ReadSubObject(reader, obj))
                    ;
            }
            else if (obj is TitledNode)
            {
                TitledNode node = obj as TitledNode;
                TitledNodeReference nodeReference = null;
                TitledNode childNode;
                TitledNodeReference childNodeReference;
                string nodeSource = LessonUtilities.ObjectSourceFromObject(node);

                if (Tree != null)
                    nodeReference = Tree.GetNodeWithSource(node.Key, nodeSource);

                while (ReadHeaderCreate(reader, node, nodeReference, out childNode, out childNodeReference))
                    ReadObject(reader, childNode);
            }
            else
            {
                Error = "Read not supported for this object type: " + obj.GetType().Name;
                throw new ModelException(Error);
            }

            ReadFooterConfirm(reader, obj);
        }

        protected virtual void ReadHeaderConfirm(StreamReader reader, IBaseXml obj)
        {
            for (;;)
            {
                string line = ReadLine(reader);
                if (line == null)
                    return;
                if (String.IsNullOrEmpty(line))
                    continue;
                TitleCount = TitleCount + 1;
                string[] parts = line.Split(LanguageLookup.ColonCharacters);
                string componentName = (parts.Count() >= 1 ? parts[0] : "");
                if (componentName != GetNameFromComponent(obj))
                {
                    Error = "Read object header doesn't match.";
                    throw new ModelException(Error);
                }
                if (obj is TitledBase)
                {
                    TitledBase titledBase = obj as TitledBase;
                    string title = (parts.Count() >= 2 ? parts[1].Trim() : "");
                    string description = (parts.Count() >= 3 ? parts[2].Trim() : "");
                    string source = LessonUtilities.ObjectSourceFromTypeName(componentName);
                    string languageCodes = (parts.Count() >= 4 ? parts[3].Trim() : "");
                    if (!String.IsNullOrEmpty(languageCodes))
                    {
                        List<LanguageID> languageIDs = LanguageID.ParseLanguageIDList(languageCodes);
                        titledBase.LanguageIDs = languageIDs;
                        if (languageIDs.Count() != 0)
                        {
                            LanguageID languageID = languageIDs[0];
                            titledBase.LanguageID = languageID;
                        }
                    }
                    if (titledBase.Title != null)
                        titledBase.Title.Combine(new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, title)));
                    else
                        titledBase.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, title));
                    if (titledBase.Description != null)
                        titledBase.Description.Combine(new MultiLanguageString("Description", new LanguageString("Description", UserRecord.UILanguage, title)));
                    else
                        titledBase.Description = new MultiLanguageString("Description", new LanguageString("Description", UserRecord.UILanguage, title));
                }
                break;
            }
        }

        protected virtual bool ReadHeaderCreate(StreamReader reader, TitledNode parentNode, TitledNodeReference parentNodeReference,
            out TitledNode childNode, out TitledNodeReference childNodeReference)
        {
            childNode = null;
            childNodeReference = null;
            for (;;)
            {
                string line = ReadLine(reader);
                if (line == null)
                    return false;
                if (String.IsNullOrEmpty(line))
                    continue;
                string[] parts = line.Split(LanguageLookup.ColonCharacters);
                string componentName = (parts.Count() >= 1 ? parts[0].Trim() : "");
                string title = (parts.Count() >= 2 ? parts[1].Trim() : "");
                string description = (parts.Count() >= 3 ? parts[2].Trim() : "");
                string source = LessonUtilities.ObjectSourceFromTypeName(componentName);
                string languageCodes = (parts.Count() >= 4 ? parts[3].Trim() : "");
                LanguageID languageID = parentNode.LanguageID;
                List<LanguageID> languageIDs = parentNode.LanguageIDs;
                if (!String.IsNullOrEmpty(languageCodes))
                {
                    languageIDs = LanguageID.ParseLanguageIDList(languageCodes);
                    if (languageIDs.Count() != 0)
                        languageID = languageIDs[0];
                }
                PushLine(line);     // Push for ReadHeaderConfirm of ReadObject.
                TitledReference existingChildReference = null;
                if (parentNode.ChildCount() != 0)
                    existingChildReference = parentNode.Children.FirstOrDefault(x => (x.Title != null) && (x.Title.Text(HostLanguageID) == title));
                if (existingChildReference != null)
                    childNode = existingChildReference.TypedItem<TitledNode>();
                else
                {
                    switch (componentName)
                    {
                        case "LessonGroup":
                            childNode = new LessonGroup();
                            break;
                        case "Lesson":
                            childNode = new Lesson();
                            break;
                        case "FlashList":
                            childNode = new FlashList();
                            break;
                        case "End":
                            if (title != parentNode.GetType().Name)
                                throw new ModelException(Error = "Unexpected end type: " + title);
                            return false;
                        default:
                            throw new ModelException(Error = "Unexpected child object type: " + componentName);
                    }
                }
                if (childNode.Title != null)
                    childNode.Title.Combine(new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, title)));
                else
                    childNode.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, title));
                if (childNode.Description != null)
                    childNode.Description.Combine(new MultiLanguageString("Description", new LanguageString("Description", UserRecord.UILanguage, title)));
                else
                    childNode.Description = new MultiLanguageString("Description", new LanguageString("Description", UserRecord.UILanguage, title));
                childNode.LanguageID = languageID;
                childNode.LanguageIDs = languageIDs;
                if (existingChildReference == null)
                {
                    childNode.Index = parentNode.ChildCount();
                    childNode.Owner = parentNode.Owner;
                    childNode.IsPublic = parentNode.IsPublic;
                    if (parentNodeReference != null)
                        childNode.Parent = new TitledReference(parentNodeReference.Source, parentNode);
                    if (!Repositories.SaveReference(source, null, childNode))
                        throw new ModelException(Error = "Error adding " + componentName + ".");
                    childNodeReference = new TitledNodeReference(source, childNode);
                    if (Tree != null)
                    {
                        Tree.AddNode(childNodeReference);
                        if (parentNode == Tree)
                            Tree.AddChild(new TitledReference(source, childNode));
                    }
                    if ((parentNode != null) && (parentNode != Tree))
                        parentNode.AddChild(new TitledReference(source, childNode));
                    if (parentNodeReference != null)
                        parentNodeReference.AddChild(new TitledReference(source, childNode));
                }
                break;
            }
            return true;
        }

        protected override void ReadComponent(StreamReader reader)
        {
            string line;

            _Items = new List<SpeakerText>();
            Error = "";
            ItemCount = 0;

            if (Timer != null)
                Timer.Start();

            ReadLine(reader);

            if (LanguageDescriptors != null)
            {
                _LanguageIndex = 0;

                if (UseLanguageLabels)
                {
                    ItemIndex = 0;

                    while ((line = ReadLine(reader)) != null)
                    {
                        if (ItemIndex == 0)
                        {
                            if (line.StartsWith("(") && line.EndsWith(")"))
                            {
                                string languageCode = line.Substring(1, line.Length - 2);
                                _LanguageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
                                continue;
                            }
                        }

                        if (!ScanLine(line))
                        {
                            ItemIndex = 0;
                            _LanguageIndex++;
                        }
                    }
                }
                else
                {
                    foreach (LanguageDescriptor languageDescriptor in LanguageDescriptors)
                    {
                        if (!languageDescriptor.Used || (languageDescriptor.LanguageID == null) || !languageDescriptor.Show)
                            continue;

                        ItemIndex = 0;
                        _LanguageID = languageDescriptor.LanguageID;

                        while ((line = ReadLine(reader)) != null)
                        {
                            if (!ScanLine(line))
                                break;
                        }

                        _LanguageIndex++;
                    }
                }
            }

            ItemIndex = BaseIndex;
            AddSpeakerTextList(_Items);

            if (Timer != null)
            {
                Timer.Stop();
                OperationTime = Timer.GetTimeInSeconds();
            }

            if (!String.IsNullOrEmpty(Error))
                throw new ModelException(Error);
        }

        protected virtual bool ReadSubObject(StreamReader reader, IBaseXml parent)
        {
            Lesson lesson = parent as Lesson;

            if (lesson == null)
                return false;

            for (;;)
            {
                string line = ReadLine(reader);

                if (line == null)
                    return false;

                if (String.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(LanguageLookup.ColonCharacters);

                if (parts.Count() == 0)
                    continue;

                string componentName = parts[0].Trim();
                string title = "";
                string label = "Sentences";

                if (parts.Count() >= 2)
                {
                    if (componentName == "Vocabulary")
                    {
                        label = parts[1].Trim();
                        title = "Vocabulary - " + label;
                    }
                    else
                        title = parts[1].Trim();
                }

                // Push line.
                PushLine(line);

                if (componentName == "End")
                    return false;

                LessonComponent lessonComponent = lesson.GetLabeledFirst(componentName, label);

                if (lessonComponent == null)
                {
                    string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(Tree, lesson, null, null);
                    lessonComponent = LessonUtilities.CreateComponent(null, lesson, componentName, "(new)", label, mediaDirectory);

                    bool useIt = true;

                    if ((ComponentFlags == null) || !IsSupportedVirtual("Import", "Lesson", "ComponentFlags") || (ComponentFlags.TryGetValue(componentName, out useIt) && useIt))
                        lesson.Add(lessonComponent);
                }

                ReadObject(reader, lessonComponent);

                break;
            }

            return true;
        }

        protected virtual void ReadFooterConfirm(StreamReader reader, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList))
                return;
            string line = ReadLine(reader);
            if (line == null)
                return;
            if (String.IsNullOrEmpty(line))
                line = ReadLine(reader);
            TitleCount = TitleCount + 1;
            string[] parts = line.Split(LanguageLookup.ColonCharacters);
            string componentName = (parts.Count() >= 1 ? parts[0].Trim() : "");
            string typeName = (parts.Count() >= 2 ? parts[1].Trim() : "");
            if (componentName != "End")
            {
                Error = "Missing \"End\" line.";
                throw new ModelException(Error);
            }
            string expectedName = GetNameFromComponent(obj);
            if (typeName != expectedName)
            {
                Error = "End type \"" + typeName + "\" doesn't match expected \"" + expectedName + "\".";
                throw new ModelException(Error);
            }
        }

        protected override void WriteObjectHeader(StreamWriter writer, IBaseXml obj)
        {
            if (TitleCount != 0)
                writer.WriteLine("");
            TitleCount = TitleCount + 1;
            if (obj is TitledBase)
            {
                TitledBase titledBase = obj as TitledBase;
                string componentName = GetNameFromComponent(obj);
                string componentTitle;
                if (componentName == "Vocabulary")
                    componentTitle = " " + titledBase.Label;
                else if (titledBase.Title != null)
                {
                    componentTitle = titledBase.Title.Text(HostLanguageID);
                    if (!String.IsNullOrEmpty(componentTitle))
                        componentTitle = " " + componentTitle;
                }
                else
                    componentTitle = "";
                string languageCodes = "";
                string description = "";
                if (titledBase.Description != null)
                    description = " : " + titledBase.Description.Text(HostLanguageID);
                if (titledBase.LanguageIDs != null)
                    languageCodes = " : " + LanguageID.ConvertLanguageIDListToString(LanguageDescriptor.LanguageIDsFromLanguageDescriptors(LanguageDescriptors));
                string title = componentName + ":" + componentTitle + description + languageCodes;
                writer.WriteLine(title);
            }
        }

        protected override void WriteComponent(StreamWriter writer)
        {
            writer.WriteLine("");

            ItemCount = GetEntryCount() - BaseIndex;

            if (LanguageDescriptors != null)
            {
                _LanguageIndex = 0;

                foreach (LanguageDescriptor languageDescriptor in LanguageDescriptors)
                {
                    if (!languageDescriptor.Used || (languageDescriptor.LanguageID == null) || !languageDescriptor.Show)
                        continue;

                    ItemIndex = BaseIndex;
                    _LanguageID = languageDescriptor.LanguageID;

                    if (_LanguageIndex != 0)
                        writer.WriteLine(String.Empty);

                    if (UseLanguageLabels)
                        writer.WriteLine("(" + _LanguageID.LanguageCultureExtensionCode + ")");

                    while (ItemIndex < (ItemCount + BaseIndex))
                    {
                        if ((UseFlags != null) && !UseFlags[ItemIndex])
                        {
                            ItemIndex = ItemIndex + 1;
                            continue;
                        }

                        string line = FormatLine();

                        if (line != null)
                            writer.WriteLine(line);
                        else
                            break;
                    }

                    _LanguageIndex++;
                }
            }
        }

        protected override void WriteObjectFooter(StreamWriter writer, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList))
                return;
            writer.WriteLine("");
            writer.WriteLine("End: " + obj.GetType().Name);
        }

        protected bool ScanLine(string line)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line) && (ItemIndex >= ItemCount))
                return false;

            SpeakerText speakerText;

            if (ItemIndex == ItemCount)
            {
                speakerText = new SpeakerText(null, LanguageDescriptors, null);
                _Items.Add(speakerText);
                ItemCount = ItemCount + 1;
            }
            else
                speakerText = _Items[ItemIndex];

            LanguageItem languageItem = speakerText.LanguageItem(_LanguageID);

            if (languageItem == null)
            {
                languageItem = new LanguageItem(null, _LanguageID, null);
                speakerText.Add(languageItem);
            }

            if (UseSpeakerNames && (GetComponentName() != "Vocabulary"))
            {
                if (line.Contains(":"))
                {
                    string[] parts = line.Split(LanguageLookup.ColonCharacters);

                    if (parts.Count() != 0)
                    {
                        string speakerName = parts[0];
                        AddSpeakerNameCheck(speakerText, speakerName, _LanguageID);
                        if (parts.Count() > 1)
                            line = line.Substring(speakerName.Length + 1);
                        else
                            line = "";
                    }
                }
                line = line.Trim();
            }

            languageItem.Text = line;

            ItemIndex = ItemIndex + 1;

            return true;
        }

        protected string FormatLine()
        {
            InitializeSpeakerNameField();

            MultiLanguageItem multiLanguageItem = GetEntry();

            if (multiLanguageItem == null)
                return null;

            LanguageItem languageItem = multiLanguageItem.LanguageItem(_LanguageID);

            if (languageItem == null)
                return null;

            string text = languageItem.Text;

            if (text == null)
                text = "";

            text = text.Replace("\r", "").Replace("\n", "");

            return _SpeakerNameField + text;
        }

        protected void InitializeSpeakerNameField()
        {
            LanguageID hostLanguageID = HostLanguageID;

            _SpeakerNameField = String.Empty;
            _SpeakerNameFieldWidth = 0;

            if (!UseSpeakerNames || (GetComponentName() == "Vocabulary"))
                return;

            switch (GetComponentName())
            {
                case "Text":
                case "Transcript":
                    {
                        SpeakerTextsComponent component = GetComponent<SpeakerTextsComponent>();
                        List<MultiLanguageString> speakers = component.SpeakerNames;
                        if ((speakers == null) || (speakers.Count() == 0))
                            break;
                        foreach (MultiLanguageString multiLanguageString in speakers)
                        {
                            string name = multiLanguageString.Text(_LanguageID);
                            if (String.IsNullOrEmpty(name))
                            {
                                name = multiLanguageString.Text(hostLanguageID);
                                if (String.IsNullOrEmpty(name))
                                    name = multiLanguageString.KeyString;
                            }
                            int len = 0;
                            if (!String.IsNullOrEmpty(name))
                                len = name.Length + 3;
                            if (len > _SpeakerNameFieldWidth)
                                _SpeakerNameFieldWidth = len;
                        }
                        string speakerName = String.Empty;
                        SpeakerText speakerText = component.GetIndexed(ItemIndex);
                        if (speakerText != null)
                        {
                            string speakerNameKey = speakerText.SpeakerKey;
                            if (!String.IsNullOrEmpty(speakerNameKey))
                            {
                                speakerName = component.GetSpeakerNameText(speakerNameKey, _LanguageID);
                                if (String.IsNullOrEmpty(speakerName))
                                {
                                    speakerName = component.GetSpeakerNameText(speakerNameKey, hostLanguageID);
                                    if (String.IsNullOrEmpty(speakerName))
                                        speakerName = speakerNameKey;
                                }
                            }
                        }
                        string spaces = "                                                 ";
                        if (!String.IsNullOrEmpty(speakerName))
                        {
                            _SpeakerNameField = speakerName + ":  ";
                            if (_SpeakerNameField.Length < _SpeakerNameFieldWidth)
                                _SpeakerNameField += spaces.Substring(0, _SpeakerNameFieldWidth - _SpeakerNameField.Length);

                        }
                        else
                            _SpeakerNameField = spaces.Substring(0, _SpeakerNameFieldWidth);
                    }
                    break;
                case "Vocabulary":
                case "StudyList":
                default:
                    break;
            }
        }

        protected void AddSpeakerNameCheck(SpeakerText speakerText, string speakerName, LanguageID languageID)
        {
            switch (GetComponentName())
            {
                case "Text":
                case "Transcript":
                    {
                        SpeakerTextsComponent component = GetComponent<SpeakerTextsComponent>();
                        List<MultiLanguageString> speakers = component.SpeakerNames;
                        if (speakers == null)
                        {
                            speakers = new List<MultiLanguageString>();
                            component.SpeakerNames = speakers;
                        }
                        foreach (MultiLanguageString multiLanguageString in speakers)
                        {
                            LanguageString languageString = multiLanguageString.LanguageString(languageID);
                            if (languageString == null)
                            {
                                languageString = new LanguageString(multiLanguageString.Key, languageID, speakerName);
                                multiLanguageString.Add(languageString);
                                if (languageID == HostLanguageID)
                                {
                                    multiLanguageString.SetKeys(speakerName);
                                    speakerText.SpeakerKey = speakerName;
                                }
                                else if (String.IsNullOrEmpty(speakerText.SpeakerKey))
                                    speakerText.SpeakerKey = multiLanguageString.KeyString;
                                return;
                            }
                            else if (languageString.Text == speakerName)
                            {
                                if (languageID == HostLanguageID)
                                    speakerText.SpeakerKey = speakerName;
                                else if (String.IsNullOrEmpty(speakerText.SpeakerKey))
                                    speakerText.SpeakerKey = multiLanguageString.KeyString;
                                return;
                            }
                        }
                        MultiLanguageString newName = new MultiLanguageString(null, new LanguageString(speakerName, languageID, speakerName));
                        speakers.Add(newName);
                        if (languageID == HostLanguageID)
                            speakerText.SpeakerKey = speakerName;
                        else if (String.IsNullOrEmpty(speakerText.SpeakerKey))
                            speakerText.SpeakerKey = newName.KeyString;
                    }
                    break;
                default:
                    break;
            }
        }

        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            switch (componentName)
            {
                case "Text":
                case "TargetTextComponent":
                case "Transcript":
                case "TranscriptComponent":
                case "Vocabulary":
                case "Vocabulary Characters":
                case "Vocabulary Words":
                case "Vocabulary Sentences":
                case "StudyListComponent":
                case "Notes":
                case "NotesComponent":
                case "Comments":
                case "CommentsComponent":
                case "FlashList":
                    if (capability == "Support")
                        return true;
                    else if ((importExport == "Export") && (capability == "UseFlags"))
                        return true;
                    return false;
                case "Lesson":
                    if (capability == "Support")
                        return true;
                    else if (capability == "ComponentFlags")
                        return true;
                    return false;
                case "LessonGroup":
                case "Course":
                case "Plan":
                    if (capability == "Support")
                        return true;
                    else if (capability == "ComponentFlags")
                        return true;
                    else if (capability == "NodeFlags")
                    {
                        if (importExport == "Export")
                            return true;
                    }
                    return false;
                case "Dictionary":
                case "LessonMaster":
                case "MarkupTemplate":
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string componentName, string capability)
        {
            return IsSupportedStatic(importExport, componentName, capability);
        }

        public static new string TypeStringStatic { get { return "Blocked"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
