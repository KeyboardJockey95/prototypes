using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public partial class FormatComponent : Format
    {
        public TitledTree Tree { get; set; }
        public IBaseXml Target { get; set; }
        public List<IBaseXml> TargetList { get; set; }
        public IBaseXml Component { get; set; }
        public string TitlePrefix { get; set; }
        public string DefaultComponent { get; set; }
        public string Label { get; set; }
        public string MasterName { get; set; }
        public LessonMasterReference MasterReference { get; set; }
        public bool SubDivide { get; set; }
        public int ItemSubDivideCount { get; set; }
        public int LessonSubDivideCount { get; set; }
        private int ItemCounter { get; set; }
        private int LessonCounter { get; set; }
        private Dictionary<string, int> ItemCounters { get; set; }
        private Dictionary<string, int> LessonCounters { get; set; }
        public bool Sort { get; set; }
        public string MediaPath { get; set; }
        public List<LanguageDescriptor> LanguageDescriptors { get; set; }
        public int TitleCount { get; set; }
        public int ComponentIndex { get; set; }
        public int BaseIndex { get; set; }
        public int ItemIndex { get; set; }
        public List<FormatArgument> Arguments { get; set; }
        public Dictionary<string, bool> NodeFlags { get; set; }
        public Dictionary<string, bool> ComponentFlags { get; set; }
        public Dictionary<string, bool> MediaFlags { get; set; }
        public List<bool> UseFlags { get; set; }
        public bool IsCollectAlternateMedia { get; set; }
        public IMainRepository Repositories { get; set; }
        public UserRecord UserRecord { get; set; }
        public ITranslator Translator { get; set; }
        public LanguageUtilities LanguageUtilities { get; set; }
        public LessonUtilities LessonUtilities { get; set; }
        private string _SavedLine = null;

        public FormatComponent(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex, string type,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(GetNameFromTarget(target), type, GetOwnerFromObject(target))
        {
            Tree = tree;
            Target = target;
            TargetList = null;
            Component = null;
            TitlePrefix = "Default";
            DefaultComponent = "Vocabulary";
            Label = "Words";
            MasterName = "";
            MasterReference = null;
            SubDivide = false;
            ItemSubDivideCount = 20;
            LessonSubDivideCount = 5;
            ItemCounter = 0;
            LessonCounter = 0;
            ItemCounters = null;
            LessonCounters = null;
            Sort = false;
            MediaPath = "";
            LanguageDescriptors = LanguageDescriptor.CopyLanguageDescriptors(languageDescriptors);
            BaseIndex = itemIndex;
            ItemIndex = itemIndex;
            Arguments = null;
            NodeFlags = null;
            ComponentFlags = null;
            MediaFlags = null;
            UseFlags = null;
            IsCollectAlternateMedia = true;
            Repositories = repositories;
            UserRecord = userRecord;
            Translator = translator;
            LanguageUtilities = languageUtilities;
            LessonUtilities = new LessonUtilities(repositories, userRecord, translator, languageUtilities);
        }

        public FormatComponent(TitledTree tree, string targetName, string targetOwner, List<IBaseXml> targetList,
                List<LanguageDescriptor> languageDescriptors, int itemIndex, string type,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(targetName, type, targetOwner)
        {
            Tree = tree;
            Target = null;
            TargetList = targetList;
            Component = null;
            TitlePrefix = "Default";
            DefaultComponent = "Vocabulary";
            Label = "Words";
            MasterName = "";
            MasterReference = null;
            SubDivide = false;
            ItemSubDivideCount = 20;
            LessonSubDivideCount = 5;
            ItemCounter = 0;
            LessonCounter = 0;
            ItemCounters = null;
            LessonCounters = null;
            Sort = false;
            LanguageDescriptors = LanguageDescriptor.CopyLanguageDescriptors(languageDescriptors);
            BaseIndex = itemIndex;
            ItemIndex = itemIndex;
            Arguments = null;
            NodeFlags = null;
            ComponentFlags = null;
            MediaFlags = null;
            UseFlags = null;
            Repositories = repositories;
            UserRecord = userRecord;
            Translator = translator;
            LanguageUtilities = languageUtilities;
            LessonUtilities = new LessonUtilities(repositories, userRecord, translator, languageUtilities);
        }

        public override void Read(Stream stream)
        {
            if (!string.IsNullOrEmpty(MasterName) && (MasterReference == null))
            {
                LessonMaster master = Repositories.ResolveNamedReference("LessonMasters", null, UserRecord.UserName, MasterName) as LessonMaster;

                if (master == null)
                    throw new ModelException("Can't find lesson master: " + MasterName);

                MasterReference = new LessonMasterReference(master.Key, master);
            }

            using (StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding(true)))
            {
                TitleCount = 0;
                ComponentIndex = 0;

                if (Timer != null)
                    Timer.Start();

                ReadFormatHeader(reader);

                if (TargetList != null)
                {
                    foreach (IBaseXml target in TargetList)
                        ReadObject(reader, target);
                }
                else
                    ReadObject(reader, Target);

                if (Sort && (Tree != null) && (Target != null) && (Target is TitledNode))
                    LessonUtilities.SortNodeChildren(Tree, Target as TitledNode, true);

                if (Timer != null)
                {
                    Timer.Stop();
                    OperationTime = Timer.GetTimeInSeconds();
                }

                if (!String.IsNullOrEmpty(Error))
                    throw new ModelException(Error);
            }
        }

        protected virtual void ReadFormatHeader(StreamReader reader)
        {
        }

        protected virtual void ReadObject(StreamReader reader, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList))
            {
                Component = obj;
                if (IsSupportedVirtual("Import", GetComponentName(), "Support"))
                {
                    ReadComponent(reader);
                    ComponentIndex = ComponentIndex + 1;
                }
                Component = null;
            }
            else
            {
                Error = "Read not supported for this format.";
                throw new ModelException(Error);
            }
        }

        protected virtual void ReadComponent(StreamReader reader)
        {
        }

        protected virtual string ReadLine(StreamReader reader)
        {
            string line;

            if (_SavedLine != null)
            {
                line = _SavedLine;
                _SavedLine = null;
            }
            else
                line = reader.ReadLine();

            return line;
        }

        protected virtual void PushLine(string line)
        {
            if (_SavedLine != null)
                throw new ModelException("Format line already pushed.");

            _SavedLine = line;
        }

        public override void Write(Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream, new System.Text.UTF8Encoding(true));
            {
                TitleCount = 0;
                ComponentIndex = 0;

                WriteFormatHeader(writer);

                if (TargetList != null)
                {
                    foreach (IBaseXml target in TargetList)
                        WriteObject(writer, target);
                }
                else
                    WriteObject(writer, Target);

                writer.Flush();

#if !PORTABLE
                writer.Close();
#endif
            }
        }

        protected virtual void WriteFormatHeader(StreamWriter writer)
        {
        }

        protected virtual void WriteObject(StreamWriter writer, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList))
            {
                Boolean exportIt = true;
                TitledBase titledBase = obj as TitledBase;

                Component = obj;

                if (obj == Target)
                    exportIt = true;
                else
                {
                    if ((NodeFlags != null) && (obj is FlashList))
                        NodeFlags.TryGetValue(LessonUtilities.KeyAndSourceString(titledBase.KeyString, "FlashLists"), out exportIt);

                    if ((ComponentFlags != null) && (obj is FlashList))
                        ComponentFlags.TryGetValue("FlashList", out exportIt);
                }

                if (exportIt && IsSupportedVirtual("Export", GetComponentName(), "Support"))
                {
                    WriteObjectHeader(writer, obj);
                    WriteComponent(writer);
                    WriteObjectFooter(writer, obj);
                    ComponentIndex = ComponentIndex + 1;
                }

                Component = null;
            }
            else if (obj is TitledNode)
            {
                string source = LessonUtilities.ObjectSourceFromObject(obj);
                TitledBase titledBase = obj as TitledBase;
                Boolean exportIt = false;

                if (obj == Target)
                    exportIt = true;
                else if ((titledBase != null) && (NodeFlags != null))
                    NodeFlags.TryGetValue(LessonUtilities.KeyAndSourceString(titledBase.KeyString, source), out exportIt);

                if (exportIt)
                    WriteObjectHeader(writer, obj);

                if ((obj is Lesson) && exportIt)
                {
                    Lesson lesson = obj as Lesson;

                    if (lesson.LessonComponents != null)
                    {
                        foreach (LessonComponent lessonComponent in lesson.LessonComponents)
                        {
                            bool exportComponent = false;

                            if (ComponentFlags != null)
                            {
                                if (ComponentFlags.TryGetValue(lessonComponent.KeyString, out exportComponent))
                                {
                                    if (!exportComponent)
                                        continue;
                                }
                                else if (ComponentFlags.TryGetValue(lessonComponent.GetComponentName(), out exportComponent))
                                {
                                    if (!exportComponent)
                                        continue;
                                }
                                else if (ComponentFlags.TryGetValue(lessonComponent.GetComponentName() + " " + lessonComponent.Label, out exportComponent))
                                {
                                    if (!exportComponent)
                                        continue;
                                }
                            }

                            if (IsSupportedVirtual("Export", GetNameFromComponent(lessonComponent), "Support"))
                            {
                                ItemIndex = 0;
                                WriteObject(writer, lessonComponent);
                            }
                        }
                    }
                }

                TitledNode titledNode = obj as TitledNode;

                if (titledNode != Tree)
                    titledNode = Tree.GetNodeWithSource(titledNode.Key, source);

                if (titledNode.Children != null)
                {
                    foreach (TitledReference child in titledNode.Children)
                    {
                        titledBase = child.Item;

                        if (titledBase == null)
                        {
                            child.ResolveReferences(Repositories, false, true);
                            titledBase = child.Item;
                        }

                        if (titledBase != null)
                        {
                            if (!titledBase.IsPublic && !UserRecord.IsAdministrator()
                                    && (titledBase.Owner != UserRecord.TeamOrUserName))
                                continue;

                            ItemIndex = 0;
                            WriteObject(writer, child.Item);
                        }
                    }
                }

                if (exportIt)
                    WriteObjectFooter(writer, obj);
            }
        }

        protected virtual void WriteObjectHeader(StreamWriter writer, IBaseXml obj)
        {
            TitleCount = TitleCount + 1;
        }

        protected virtual void WriteComponent(StreamWriter writer)
        {
        }

        protected virtual void WriteObjectFooter(StreamWriter writer, IBaseXml obj)
        {
        }

        protected virtual void CollectMediaFiles(List<string> mediaFiles, IBaseXml obj)
        {
            MediaCollector collector = new MediaCollector(NodeFlags, ComponentFlags, MediaFlags, Repositories, UserRecord,
                IsSupportedVirtual, IsCollectAlternateMedia);
            collector.CollectMediaFiles(mediaFiles, obj, Tree);
        }

        protected virtual void LazyConvertMediaFiles()
        {
            List<string> mediaFiles = new List<string>();

            if (TargetList != null)
            {
                foreach (IBaseXml target in TargetList)
                    CollectMediaFiles(mediaFiles, target);
            }
            else if (Target != null)
                CollectMediaFiles(mediaFiles, Target);

            foreach (string mediaFilePath in mediaFiles)
            {
                string mimeType = MediaDescription.GetMimeTypeFromFileName(mediaFilePath);

                try
                {
                    if (!FileSingleton.Exists(mediaFilePath))
                    {
                        continue;
                    }

                    string message;
                    MediaConvertSingleton.LazyConvert(mediaFilePath, mimeType, false, out message);
                }
                catch (Exception)
                {
                }
            }
        }

        public T GetTarget<T>() where T : TitledBase
        {
            return Target as T;
        }

        public T GetComponent<T>() where T : TitledBase
        {
            return Component as T;
        }

        public static string GetNameFromTarget(IBaseXml target)
        {
            if (target == null)
                return String.Empty;

            if (target is FlashList)
                return "FlashList";
            else if (target is TargetTextComponent)
                return "Text";
            else if (target is TranscriptComponent)
                return "Transcript";
            else if (target is StudyListComponent)
                return "Vocabulary";
            else if (target is NotesComponent)
                return "Notes";
            else if (target is CommentsComponent)
                return "Comments";
            else
                return target.GetType().Name;
        }

        public static string GetNameFromComponent(IBaseXml component)
        {
            if (component == null)
                return String.Empty;

            if (component is FlashList)
                return "FlashList";
            else if (component is TargetTextComponent)
                return "Text";
            else if (component is TranscriptComponent)
                return "Transcript";
            else if (component is StudyListComponent)
                return "Vocabulary";
            else if (component is NotesComponent)
                return "Notes";
            else if (component is CommentsComponent)
                return "Comments";
            else
                return component.GetType().Name;
        }

        public string GetTargetName()
        {
            return GetNameFromTarget(Target);
        }

        public string GetComponentName()
        {
            return GetNameFromComponent(Component);
        }

        public static string GetOwnerFromObject(IBaseXml obj)
        {
            if (obj is TitledBase)
                return (obj as TitledBase).Owner;
            else 
                return null;
        }

        public TitledNode TargetTitledNode
        {
            get
            {
                return GetTarget<TitledNode>();
            }
            set
            {
                Target = value;
            }
        }

        public TitledNode ComponentTitledNode
        {
            get
            {
                return GetComponent<TitledNode>();
            }
            set
            {
                Component = value;
            }
        }

        public TitledBase ComponentTitledBase
        {
            get
            {
                return GetComponent<TitledBase>();
            }
            set
            {
                Component = value;
            }
        }

        public string GetComponentShortTitle()
        {
            if (ComponentTitledBase != null)
                return ComponentTitledBase.GetTitleString(HostLanguageID);
            return String.Empty;
        }

        public string GetComponentParentTitle()
        {
            if (Component == null)
                return String.Empty;
            if (ComponentTitledNode != null)
            {
                if ((Tree != null) && (Target != null))
                {
                    TitledNodeReference node = Tree.GetNodeWithSource(ComponentTitledNode.Key, LessonUtilities.ObjectSourceFromObject(Target));
                    if ((node != null) && (node.Parent != null))
                    {
                        TitledNode parentNode = Tree.GetNodeWithSource(node.ParentKey, node.ParentSource);
                        if (parentNode != null)
                            return parentNode.GetTitleString(HostLanguageID);
                    }
                }
                if (ComponentTitledNode.Parent != null)
                    return ComponentTitledNode.Parent.GetTitleString(HostLanguageID);
            }
            else if (Component is LessonComponent)
                return GetComponent<LessonComponent>().Lesson.GetTitleString(HostLanguageID);
            return String.Empty;
        }

        public string GetComponentLongTitle()
        {
            if (Component != null)
                return GetComponentParentTitle() + " - " + GetComponentShortTitle();
            return String.Empty;
        }

        public virtual void LoadFromArguments()
        {
            /*
            if (Arguments == null)
                return;

            LanguageDescriptor languageDescriptor;

            foreach (FormatArgument argument in Arguments)
            {
                switch (argument.Name)
                {
                    case "Target":
                    case "TargetAlternate1":
                    case "TargetAlternate2":
                    case "TargetAlternate3":
                    case "Host":
                        if (LanguageDescriptors != null)
                        {
                            languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == argument.Name);
                            if (languageDescriptor != null)
                                languageDescriptor.Show = (argument.Value == "on" ? true : false);
                        }
                        break;
                    default:
                        break;
                }
            }
            */
        }

        public virtual void SaveToArguments()
        {
            /*
            if (Arguments == null)
                Arguments = new List<FormatArgument>();

            if (LanguageDescriptors != null)
            {
                foreach (LanguageDescriptor languageDescriptor in LanguageDescriptors)
                {
                    if ((!languageDescriptor.Used) || (languageDescriptor.LanguageID == null))
                        continue;

                    SetArgument(languageDescriptor.Name, "flag", (languageDescriptor.Show ? "on" : ""), languageDescriptor.LanguageID.LanguageCultureExtension);
                }
            }
            */
        }

        public string GetArgumentKeyName(string name)
        {
            string keyName = GetType().Name + "." + GetTargetName() + "." + name;
            return keyName;
        }

        public string GetUserOptionString(string name, string defaultValue)
        {
            string keyName = GetArgumentKeyName(name);
            if (UserRecord != null)
                return UserRecord.GetUserOptionString(keyName, defaultValue);
            return defaultValue;
        }

        public int GetUserOptionInteger(string name, int defaultValue)
        {
            string keyName = GetArgumentKeyName(name);
            if (UserRecord != null)
                return UserRecord.GetUserOptionInteger(keyName, defaultValue);
            return defaultValue;
        }

        public bool GetUserOptionFlag(string name, bool defaultValue)
        {
            string keyName = GetArgumentKeyName(name);
            if (UserRecord != null)
                return (UserRecord.GetUserOptionString(keyName, (defaultValue ? "on" : "off")) == "on");
            return defaultValue;
        }

        public FormatArgument FindArgument(string name)
        {
            if (Arguments == null)
                return null;
            FormatArgument argument = Arguments.FirstOrDefault(x => x.Name == name);
            return argument;
        }

        public FormatArgument FindOrCreateArgument(string name, string type, string direction, string value, string label, string help)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help);
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            return argument;
        }

        public string GetArgument(string name)
        {
            FormatArgument argument = FindArgument(name);

            if (argument != null)
                return argument.Value;

            return "";
        }

        public string GetArgumentDefaulted(string name, string type, string direction, string defaultValue, string label, string help)
        {
            if (UserRecord != null)
                defaultValue = UserRecord.GetUserOptionString(GetArgumentKeyName(name), defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue, label, help);

            if (argument != null)
                return argument.Value;

            return defaultValue;
        }

        public int GetIntegerArgumentDefaulted(string name, string type, string direction, int defaultValue, string label, string help)
        {
            if (UserRecord != null)
                defaultValue = UserRecord.GetUserOptionInteger(GetArgumentKeyName(name), defaultValue);

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue.ToString(), label, help);

            int value = defaultValue;

            if ((argument != null) && !String.IsNullOrEmpty(argument.Value))
            {
                try
                {
                    value = Convert.ToInt32(argument.Value);
                }
                catch (Exception)
                {
                }
            }

            return value;
        }

        public bool GetFlagArgumentDefaulted(string name, string type, string direction, bool defaultValue, string label, string help)
        {
            if (UserRecord != null)
                defaultValue = (UserRecord.GetUserOptionString(GetArgumentKeyName(name), (defaultValue ? "on" : "off")) == "on");

            FormatArgument argument = FindOrCreateArgument(name, type, direction, defaultValue ? "on" : "off", label, help);

            bool value = defaultValue;

            if ((argument != null) && (argument.Value != null))
            {
                if (argument.Value == "on")
                    value = true;
                else
                    value = false;
            }

            return value;
        }

        public FormatArgument SetArgument(string name, string type, string direction, string value, string label, string help)
        {
            FormatArgument argument = FindArgument(name);
            if (argument == null)
            {
                argument = new FormatArgument(name, type, direction, value, label, help);
                if (Arguments == null)
                    Arguments = new List<FormatArgument>(1) { argument };
                else
                    Arguments.Add(argument);
            }
            else
                argument.Value = value;

            return argument;
        }

        public FormatArgument SetIntegerArgument(string name, string type, string direction, int value, string label, string help)
        {
            return SetArgument(name, type, direction, value.ToString(), label, help);
        }

        public FormatArgument SetFlagArgument(string name, string type, string direction, bool value, string label, string help)
        {
            return SetArgument(name, type, direction, (value ? "on" : "off"), label, help);
        }

        public void SaveUserOptions()
        {
            if (UserRecord == null)
                return;

            if (Arguments == null)
                return;

            foreach (FormatArgument argument in Arguments)
                UserRecord.SetUserOptionString(GetArgumentKeyName(argument.Name), argument.Value);
        }

        public override void DeleteFirst()
        {
            ItemCount = 0;

            if (Component == null)
                return;

            if (Component is SpeakerTextsComponent)
                GetComponent<SpeakerTextsComponent>().DeleteAll();
            else if (Component is FlashList)
                GetComponent<FlashList>().DeleteAll();
            else
                Component.Clear();
        }

        protected int GetItemCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                return ItemCounter;
            else
            {
                int itemCounter = 0;

                if (ItemCounters == null)
                    ItemCounters = new Dictionary<string, int>();

                ItemCounters.TryGetValue(tag, out itemCounter);

                return itemCounter;
            }
        }

        protected void IncrementItemCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                ItemCounter = ItemCounter + 1;
            else
            {
                if (ItemCounters == null)
                    ItemCounters = new Dictionary<string, int>();

                int itemCounter = 0;

                if (ItemCounters.TryGetValue(tag, out itemCounter))
                    ItemCounters[tag] = itemCounter + 1;
                else
                    ItemCounters.Add(tag, 1);
            }
        }

        protected int GetLessonCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                return LessonCounter;
            else
            {
                int lessonCounter = 0;

                if (LessonCounters == null)
                    LessonCounters = new Dictionary<string, int>();

                LessonCounters.TryGetValue(tag, out lessonCounter);

                return lessonCounter;
            }
        }

        protected void IncrementLessonCounter(string tag)
        {
            if (String.IsNullOrEmpty(tag))
                LessonCounter = LessonCounter + 1;
            else
            {
                if (LessonCounters == null)
                    LessonCounters = new Dictionary<string, int>();

                int lessonCounter = 0;

                if (LessonCounters.TryGetValue(tag, out lessonCounter))
                    LessonCounters[tag] = lessonCounter + 1;
                else
                    LessonCounters.Add(tag, 1);
            }
        }

        protected string FormatNumber(int number)
        {
            return String.Format("{0,4:d4}", number);
        }

        protected string GetGroupTitle(string tag)
        {
            int startGroupNumber = (GetItemCounter(tag) / (ItemSubDivideCount * LessonSubDivideCount)) * (ItemSubDivideCount * LessonSubDivideCount);
            int endGroupNumber = startGroupNumber + (ItemSubDivideCount * LessonSubDivideCount) - 1;
            string groupTitle = tag + (SubDivide ? (!String.IsNullOrEmpty(tag) ? " - " : "") + "(" + FormatNumber(startGroupNumber) + "-" + FormatNumber(endGroupNumber) + ")" : "");
            return groupTitle;
        }

        protected string GetLessonTitle(string tag)
        {
            int startLessonNumber = (GetItemCounter(tag) / ItemSubDivideCount) * ItemSubDivideCount;
            int endLessonNumber = startLessonNumber + ItemSubDivideCount - 1;
            string lessonTitle = tag + (SubDivide ? (!String.IsNullOrEmpty(tag) ? " - " : "") + "(" + FormatNumber(startLessonNumber) + "-" + FormatNumber(endLessonNumber) + ")" : "");
            return lessonTitle;
        }

        protected void AddEntry(MultiLanguageItem multiLanguageItem, string speakerKey = null, Dictionary<string, string> tags = null)
        {
            string tag = null;
            string componentName = DefaultComponent;
            string componentKey = DefaultComponent;
            string label = multiLanguageItem.KeyString;

            if (tags != null)
            {
                if (!tags.TryGetValue("tag", out tag))
                    tags.TryGetValue("lesson", out tag);

                if (!tags.TryGetValue("componentName", out componentName))
                    componentName = DefaultComponent;

                if (!tags.TryGetValue("label", out label))
                    label = multiLanguageItem.KeyString;
            }

            if (componentKey == "Vocabulary")
                componentKey = label;

            if (Component == null)
                return;

            bool allow = true;
            string flagsName = (componentName == "Vocabulary" ? componentName + " " + label : componentName);

            if ((ComponentFlags != null) && ComponentFlags.TryGetValue(flagsName, out allow) && !allow)
                return;

            if (Component is SpeakerTextsComponent)
            {
                GetComponent<SpeakerTextsComponent>().InsertMultiLanguageItem(ItemIndex, multiLanguageItem, speakerKey);
                ItemIndex = ItemIndex + 1;
            }
            else if (Component is FlashList)
            {
                if (GetComponent<FlashList>().LoadEntryIndexed(ItemIndex, multiLanguageItem))
                    ItemIndex = ItemIndex + 1;
            }
            else if (Component is Lesson)
            {
                Lesson lesson = Component as Lesson;
                LessonComponent lessonComponent = lesson.Get(componentKey);
                if (lessonComponent == null)
                    lessonComponent = lesson.GetLabeledFirst(componentName, label);
                SpeakerTextsComponent speakerTextsComponent = lessonComponent as SpeakerTextsComponent;
                if (speakerTextsComponent == null)
                {
                    if ((MasterReference != null) && (MasterReference.Item != null))
                    {
                        MasterComponentItem componentItem = MasterReference.Item.GetComponentItem(componentKey);
                        if (componentItem == null)
                            componentItem = MasterReference.Item.GetComponentItem(label);
                        if (componentItem == null)
                            componentItem = MasterReference.Item.GetComponentItem(componentName);
                        if (componentItem != null)
                        {
                            LessonUtilities.ConfigureComponentFromComponentItem(lesson, componentItem, Tree.KeyString, null);
                            speakerTextsComponent = lesson.GetTyped<SpeakerTextsComponent>(componentKey);
                        }
                    }
                    if (speakerTextsComponent == null)
                    {
                        speakerTextsComponent = LessonUtilities.CreateComponent(Tree.KeyString, lesson, componentName, componentKey, label, null) as SpeakerTextsComponent;
                        lesson.Add(speakerTextsComponent);
                    }
                }
                if (speakerTextsComponent == null)
                    return;
                speakerTextsComponent.AddMultiLanguageItem(multiLanguageItem, speakerKey);
            }
            else
            {
                if (TitlePrefix.Length != 0)
                {
                    if (String.IsNullOrEmpty(tag))
                        tag = TitlePrefix;
                    else
                        tag = TitlePrefix + " " + tag;
                }
                else
                {
                    if (String.IsNullOrEmpty(tag))
                        tag = "Default";
                }

                if (Component is Course)
                {
                    Course course = Component as Course;
                    LessonGroup group = null;
                    LessonGroup parentGroup = null;
                    Lesson lesson = null;
                    TitledNodeReference lessonNode = null;
                    TitledNodeReference groupNode = null;
                    TitledNodeReference parentGroupNode = null;
                    LanguageID languageID = course.LanguageID;
                    List<LanguageID> languageIDs = course.LanguageIDs;
                    string groupTitle = GetGroupTitle(tag);
                    string lessonTitle = GetLessonTitle(tag);

                    if (SubDivide && !String.IsNullOrEmpty(tag))
                    {
                        parentGroupNode = course.GetNodeWithSourceAndTitle("LessonGroups", tag, UserRecord.UILanguage);

                        if (parentGroupNode != null)
                        {
                            if (parentGroupNode.Item == null)
                                parentGroupNode.ResolveReferences(Repositories, false, false);

                            parentGroup = parentGroupNode.TypedItemAs<LessonGroup>();

                            if (parentGroup == null)
                                throw new ModelException("Can't file parent group: " + tag);
                        }
                        else
                        {
                            parentGroup = new LessonGroup();
                            parentGroup.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, tag));
                            parentGroup.LanguageID = course.LanguageID;
                            if (course.LanguageIDs != null)
                                parentGroup.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                            parentGroup.Index = course.ChildCount();
                            parentGroup.Owner = course.Owner;
                            parentGroup.IsPublic = course.IsPublic;
                            if (!Repositories.SaveReference("LessonGroups", null, parentGroup))
                                throw new ModelException("Error adding group: " + tag);
                            parentGroupNode = new TitledNodeReference("LessonGroups", parentGroup);
                            parentGroupNode.Index = course.ChildCount();
                            course.AddNode(parentGroupNode);
                            TitledReference courseChild = new TitledReference("LessonGroups", parentGroup);
                            courseChild.Index = parentGroup.Index;
                            course.AddChild(courseChild);
                        }
                    }

                    if (SubDivide)
                    {
                        if ((GetItemCounter(tag) % ItemSubDivideCount) == 0)
                        {
                            if (GetItemCounter(tag) != 0)
                                IncrementLessonCounter(tag);

                            groupTitle = GetGroupTitle(tag);
                            lessonTitle = GetLessonTitle(tag);

                            if ((GetLessonCounter(tag) % LessonSubDivideCount) == 0)
                            {
                                group = new LessonGroup();
                                group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                                group.LanguageID = course.LanguageID;
                                if (course.LanguageIDs != null)
                                    group.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                                group.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                                group.Owner = course.Owner;
                                group.IsPublic = course.IsPublic;
                                if (!Repositories.SaveReference("LessonGroups", null, group))
                                    throw new ModelException("Error adding group: " + groupTitle);
                                groupNode = new TitledNodeReference("LessonGroups", group);
                                groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                                course.AddNode(groupNode);
                                TitledReference courseChild = new TitledReference("LessonGroups", group);
                                courseChild.Index = group.Index;
                                if (parentGroup != null)
                                {
                                    parentGroup.AddChild(courseChild);
                                    parentGroupNode.AddChild(courseChild);
                                }
                                else
                                    course.AddChild(courseChild);
                            }
                            else
                            {
                                groupNode = Tree.GetNodeWithSourceAndTitle("LessonGroups", groupTitle, UserRecord.UILanguage);

                                if (groupNode == null)
                                {
                                    group = new LessonGroup();
                                    group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                                    group.LanguageID = course.LanguageID;
                                    if (course.LanguageIDs != null)
                                        group.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                                    group.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                                    group.Owner = course.Owner;
                                    group.IsPublic = course.IsPublic;
                                    if (!Repositories.SaveReference("LessonGroups", null, group))
                                        throw new ModelException("Error adding group: " + groupTitle);
                                    groupNode = new TitledNodeReference("LessonGroups", group);
                                    groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                                    course.AddNode(groupNode);
                                    TitledReference courseChild = new TitledReference("LessonGroups", group);
                                    courseChild.Index = group.Index;
                                    if (parentGroup != null)
                                    {
                                        parentGroup.AddChild(courseChild);
                                        parentGroupNode.AddChild(courseChild);
                                    }
                                    else
                                        course.AddChild(courseChild);
                                }
                                else
                                {
                                    if (groupNode.Item == null)
                                        groupNode.ResolveReferences(Repositories, false, false);

                                    group = groupNode.TypedItemAs<LessonGroup>();

                                    if (group == null)
                                        throw new ModelException("Can't find group: " + groupTitle);
                                }
                            }

                            lesson = new Lesson();
                            lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));
                            lesson.LanguageID = group.LanguageID;
                            if (group.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(group.LanguageIDs);
                            lesson.Index = group.ChildCount();
                            lesson.Owner = group.Owner;
                            lesson.IsPublic = group.IsPublic;
                            lesson.Parent = new TitledReference("LessonGroups", group);
                            if (MasterReference != null)
                            {
                                string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(course, lesson, null, null);
                                lesson.LessonMasterReference = new LessonMasterReference(MasterReference);
                                if (MasterReference.Item != null)
                                    LessonUtilities.ConfigureLessonFromLessonMaster(lesson, MasterReference.Item, course.KeyString, mediaDirectory);
                            }
                            if (!Repositories.SaveReference("Lessons", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            TitledReference groupChild = new TitledReference("Lessons", lesson);
                            groupChild.Index = group.ChildCount();
                            group.AddChild(groupChild);
                            lessonNode = new TitledNodeReference("Lessons", lesson);
                            groupNode.AddChild(groupChild);
                            course.AddNode(lessonNode);
                        }

                        if (groupNode == null)
                        {
                            groupNode = Tree.GetNodeWithSourceAndTitle("LessonGroups", groupTitle, UserRecord.UILanguage);

                            if (groupNode != null)
                            {
                                group = groupNode.TypedItemAs<LessonGroup>();

                                if (group == null)
                                    throw new ModelException("Can't find group: " + groupTitle);
                            }
                        }

                        if (groupNode == null)
                        {
                            group = new LessonGroup();
                            group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                            group.LanguageID = course.LanguageID;
                            if (course.LanguageIDs != null)
                                group.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                            group.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                            group.Owner = course.Owner;
                            group.IsPublic = course.IsPublic;
                            if (!Repositories.SaveReference("LessonGroups", null, group))
                                throw new ModelException("Error adding group: " + groupTitle);
                            groupNode = new TitledNodeReference("LessonGroups", group);
                            groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                            course.AddNode(groupNode);
                            TitledReference courseChild = new TitledReference("LessonGroups", group);
                            courseChild.Index = group.Index;
                            if (parentGroup != null)
                            {
                                parentGroup.AddChild(courseChild);
                                parentGroupNode.AddChild(courseChild);
                            }
                            else
                                course.AddChild(courseChild);
                        }
                    }

                    if (lessonNode == null)
                    {
                        lessonNode = course.GetNodeWithSourceAndTitle("Lessons", lessonTitle, UserRecord.UILanguage);

                        if (lessonNode != null)
                        {
                            lesson = lessonNode.TypedItemAs<Lesson>();

                            if (lesson == null)
                                throw new ModelException("Can't find lesson: " + lessonTitle);
                        }
                    }

                    if (lesson == null)
                    {
                        lesson = new Lesson();
                        lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));

                        if (MasterReference != null)
                        {
                            string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(course, lesson, null, null);
                            lesson.LessonMasterReference = new LessonMasterReference(MasterReference);
                            if (MasterReference.Item != null)
                                LessonUtilities.ConfigureLessonFromLessonMaster(lesson, MasterReference.Item, course.KeyString, mediaDirectory);
                        }

                        if (group == null)
                            group = parentGroup;

                        if (group != null)
                        {
                            lesson.LanguageID = group.LanguageID;
                            if (group.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(group.LanguageIDs);
                            lesson.Index = group.ChildCount();
                            lesson.Owner = group.Owner;
                            lesson.IsPublic = group.IsPublic;
                            lesson.Parent = new TitledReference("LessonGroups", group);
                            if (!Repositories.SaveReference("Lessons", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            TitledReference groupChild = new TitledReference("Lessons", lesson);
                            groupChild.Index = group.ChildCount();
                            group.AddChild(groupChild);
                            lessonNode = new TitledNodeReference("Lessons", lesson);
                            groupNode.AddChild(groupChild);
                        }
                        else
                        {
                            lesson.LanguageID = course.LanguageID;
                            if (course.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                            lesson.Index = course.ChildCount();
                            lesson.Owner = course.Owner;
                            lesson.IsPublic = course.IsPublic;
                            if (!Repositories.SaveReference("Lessons", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            TitledReference courseChild = new TitledReference("Lessons", lesson);
                            courseChild.Index = course.ChildCount();
                            course.AddChild(courseChild);
                            lessonNode = new TitledNodeReference("Lessons", lesson);
                        }
                        course.AddNode(lessonNode);
                    }

                    if (lessonNode == null)
                    {
                        lessonNode = new TitledNodeReference("Lessons", lesson);
                        TitledReference lessonReference = new TitledReference("Lessons", lesson);

                        if (group != null)
                        {
                            lessonNode.Owner = group.Owner;
                            lessonNode.IsPublic = group.IsPublic;
                            lessonNode.Index = group.ChildCount();
                            lessonReference.Index = group.ChildCount();
                            lessonReference.Owner = group.Owner;
                            lessonReference.IsPublic = group.IsPublic;
                            group.AddChild(lessonReference);
                            groupNode.AddChild(lessonReference);
                        }
                        else
                        {
                            lessonNode.Owner = course.Owner;
                            lessonNode.IsPublic = course.IsPublic;
                            lessonNode.Index = course.ChildCount();
                            lessonReference.Index = course.ChildCount();
                            lessonReference.Owner = course.Owner;
                            lessonReference.IsPublic = course.IsPublic;
                            course.AddChild(lessonReference);
                        }
                        course.AddNode(lessonNode);
                    }

                    SpeakerTextsComponent speakerTextsComponent = lesson.GetLabeledFirst(componentName, label) as SpeakerTextsComponent;

                    if (speakerTextsComponent == null)
                    {
                        if ((MasterReference != null) && (MasterReference.Item != null))
                        {
                            MasterComponentItem componentItem = MasterReference.Item.GetComponentItem(componentKey);
                            if (componentItem == null)
                                componentItem = MasterReference.Item.GetComponentItem(label);
                            if (componentItem == null)
                                componentItem = MasterReference.Item.GetComponentItem(componentName);
                            if (componentItem != null)
                            {
                                string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(Tree, lesson, null, null);
                                LessonUtilities.ConfigureComponentFromComponentItem(lesson, componentItem, Tree.KeyString, mediaDirectory);
                                speakerTextsComponent = lesson.GetTyped<SpeakerTextsComponent>(componentKey);
                            }
                        }
                        if (speakerTextsComponent == null)
                        {
                            string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(Tree, lesson, null, null);
                            speakerTextsComponent = LessonUtilities.CreateComponent(Tree.KeyString, lesson, componentName, componentKey, label, mediaDirectory) as SpeakerTextsComponent;
                            lesson.Add(speakerTextsComponent);
                        }
                    }

                    speakerTextsComponent.AddMultiLanguageItem(multiLanguageItem, speakerKey);
                    IncrementItemCounter(tag);
                }
                else if (Component is LessonGroup)
                {
                    Course course = Tree as Course;
                    LessonGroup parentGroup = Component as LessonGroup;
                    TitledNodeReference parentGroupNode = null;
                    LessonGroup group = null;
                    Lesson lesson = null;
                    TitledNodeReference lessonNode = null;
                    TitledNodeReference groupNode = null;
                    TitledReference groupChild;
                    LanguageID languageID = parentGroup.LanguageID;
                    List<LanguageID> languageIDs = parentGroup.LanguageIDs;
                    string groupTitle = GetGroupTitle(tag);
                    string lessonTitle = GetLessonTitle(tag);

                    lessonNode = course.GetNodeWithSourceAndTitle("Lessons", lessonTitle, UserRecord.UILanguage);
                    parentGroupNode = course.GetNodeWithSource(parentGroup.Key, "LessonGroups");

                    if (lessonNode == null)
                    {
                        lesson = new Lesson();
                        lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));
                        lesson.LanguageID = parentGroup.LanguageID;
                        if (parentGroup.LanguageIDs != null)
                            lesson.LanguageIDs = new List<LanguageID>(parentGroup.LanguageIDs);
                        lesson.Index = parentGroup.ChildCount();
                        lesson.Owner = parentGroup.Owner;
                        lesson.IsPublic = parentGroup.IsPublic;
                        if (MasterReference != null)
                        {
                            string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(course, lesson, null, null);
                            lesson.LessonMasterReference = new LessonMasterReference(MasterReference);
                            if (MasterReference.Item != null)
                                LessonUtilities.ConfigureLessonFromLessonMaster(lesson, MasterReference.Item, course.KeyString, mediaDirectory);
                        }
                        if (!Repositories.SaveReference("Lessons", null, lesson))
                            throw new ModelException("Error adding lesson: " + lessonTitle);
                    }
                    else
                    {
                        if (lessonNode.Item == null)
                            lessonNode.ResolveReferences(Repositories, false, false);

                        lesson = lessonNode.TypedItemAs<Lesson>();

                        if (lesson == null)
                            throw new ModelException("Can't find lesson: " + tag);
                    }

                    if (SubDivide)
                    {
                        if ((GetItemCounter(tag) % ItemSubDivideCount) == 0)
                        {
                            if (GetItemCounter(tag) != 0)
                                IncrementLessonCounter(tag);

                            groupTitle = GetGroupTitle(tag);
                            lessonTitle = GetLessonTitle(tag);

                            if ((GetLessonCounter(tag) % LessonSubDivideCount) == LessonSubDivideCount)
                            {
                                group = new LessonGroup();
                                group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                                group.LanguageID = parentGroup.LanguageID;
                                if (parentGroup.LanguageIDs != null)
                                    group.LanguageIDs = new List<LanguageID>(parentGroup.LanguageIDs);
                                group.Index = parentGroup.ChildCount();
                                group.Owner = parentGroup.Owner;
                                group.IsPublic = parentGroup.IsPublic;
                                if (!Repositories.SaveReference("LessonGroups", null, group))
                                    throw new ModelException("Error adding group: " + groupTitle);
                                groupNode = new TitledNodeReference("LessonGroups", group);
                                groupNode.Index = parentGroup.ChildCount();
                                course.AddNode(groupNode);
                                groupChild = new TitledReference("LessonGroups", group);
                                groupChild.Index = group.Index;
                                parentGroup.AddChild(groupChild);
                                parentGroupNode.AddChild(groupChild);
                            }
                            else
                            {
                                groupNode = Tree.GetNodeWithSourceAndTitle("LessonGroups", groupTitle, UserRecord.UILanguage);

                                if (groupNode == null)
                                {
                                    group = new LessonGroup();
                                    group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                                    group.LanguageID = course.LanguageID;
                                    if (course.LanguageIDs != null)
                                        group.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                                    group.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                                    group.Owner = course.Owner;
                                    group.IsPublic = course.IsPublic;
                                    if (!Repositories.SaveReference("LessonGroups", null, group))
                                        throw new ModelException("Error adding group: " + groupTitle);
                                    groupNode = new TitledNodeReference("LessonGroups", group);
                                    groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                                    course.AddNode(groupNode);
                                    groupChild = new TitledReference("LessonGroups", group);
                                    groupChild.Index = group.Index;
                                    parentGroup.AddChild(groupChild);
                                    parentGroupNode.AddChild(groupChild);
                                }
                                else
                                {
                                    if (groupNode.Item == null)
                                        groupNode.ResolveReferences(Repositories, false, false);

                                    group = groupNode.TypedItemAs<LessonGroup>();

                                    if (group == null)
                                        throw new ModelException("Can't find group: " + groupTitle);
                                }
                            }

                            lesson = new Lesson();
                            lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));
                            lesson.LanguageID = group.LanguageID;
                            if (group.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(group.LanguageIDs);
                            lesson.Index = group.ChildCount();
                            lesson.Owner = group.Owner;
                            lesson.IsPublic = group.IsPublic;
                            lesson.Parent = new TitledReference("LessonGroups", group);
                            if (MasterReference != null)
                            {
                                lesson.LessonMasterReference = new LessonMasterReference(MasterReference);
                                if (MasterReference.Item != null)
                                {
                                    string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(course, lesson, null, null);
                                    LessonUtilities.ConfigureLessonFromLessonMaster(lesson, MasterReference.Item, course.KeyString, mediaDirectory);
                                }
                            }
                            if (!Repositories.SaveReference("Lessons", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            groupChild = new TitledReference("Lessons", lesson);
                            groupChild.Index = group.ChildCount();
                            group.AddChild(groupChild);
                            groupNode.AddChild(groupChild);
                            lessonNode = new TitledNodeReference("Lessons", lesson);
                            lessonNode.Index = group.ChildCount();
                            course.AddNode(lessonNode);
                        }

                        if (groupNode == null)
                        {
                            groupNode = Tree.GetNodeWithSourceAndTitle("LessonGroups", groupTitle, UserRecord.UILanguage);

                            if (groupNode != null)
                            {
                                group = groupNode.TypedItemAs<LessonGroup>();

                                if (group == null)
                                    throw new ModelException("Can't find group: " + groupTitle);
                            }
                        }

                        if (groupNode == null)
                        {
                            group = new LessonGroup();
                            group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                            group.LanguageID = course.LanguageID;
                            if (course.LanguageIDs != null)
                                group.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                            group.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                            group.Owner = course.Owner;
                            group.IsPublic = course.IsPublic;
                            if (!Repositories.SaveReference("LessonGroups", null, group))
                                throw new ModelException("Error adding group: " + groupTitle);
                            groupNode = new TitledNodeReference("LessonGroups", group);
                            groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : course.ChildCount());
                            course.AddNode(groupNode);
                            TitledReference courseChild = new TitledReference("LessonGroups", group);
                            courseChild.Index = group.Index;
                            parentGroup.AddChild(courseChild);
                            parentGroupNode.AddChild(courseChild);
                        }
                    }

                    if (lessonNode == null)
                    {
                        lessonNode = course.GetNodeWithSourceAndTitle("Lessons", lessonTitle, UserRecord.UILanguage);

                        if (lessonNode != null)
                        {
                            lesson = lessonNode.TypedItemAs<Lesson>();

                            if (lesson == null)
                                throw new ModelException("Can't find lesson: " + lessonTitle);
                        }
                    }

                    if (lesson == null)
                    {
                        lesson = new Lesson();
                        lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));

                        if (MasterReference != null)
                        {
                            lesson.LessonMasterReference = new LessonMasterReference(MasterReference);
                            if (MasterReference.Item != null)
                            {
                                string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(course, lesson, null, null);
                                LessonUtilities.ConfigureLessonFromLessonMaster(lesson, MasterReference.Item, course.KeyString, mediaDirectory);
                            }
                        }

                        if (group == null)
                            group = parentGroup;

                        if (group != null)
                        {
                            lesson.LanguageID = group.LanguageID;
                            if (group.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(group.LanguageIDs);
                            lesson.Index = group.ChildCount();
                            lesson.Owner = group.Owner;
                            lesson.IsPublic = group.IsPublic;
                            lesson.Parent = new TitledReference("LessonGroups", group);
                            if (!Repositories.SaveReference("Lessons", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            groupChild = new TitledReference("Lessons", lesson);
                            groupChild.Index = group.ChildCount();
                            group.AddChild(groupChild);
                            lessonNode = new TitledNodeReference("Lessons", lesson);
                            groupNode.AddChild(groupChild);
                        }
                        else
                        {
                            lesson.LanguageID = course.LanguageID;
                            if (course.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(course.LanguageIDs);
                            lesson.Index = course.ChildCount();
                            lesson.Owner = course.Owner;
                            lesson.IsPublic = course.IsPublic;
                            if (!Repositories.SaveReference("Lessons", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            TitledReference courseChild = new TitledReference("Lessons", lesson);
                            courseChild.Index = course.ChildCount();
                            course.AddChild(courseChild);
                            lessonNode = new TitledNodeReference("Lessons", lesson);
                        }
                        course.AddNode(lessonNode);
                    }

                    if (lessonNode == null)
                    {
                        lessonNode = new TitledNodeReference("Lessons", lesson);
                        TitledReference lessonReference = new TitledReference("Lessons", lesson);

                        if (group != null)
                        {
                            lessonNode.Owner = group.Owner;
                            lessonNode.IsPublic = group.IsPublic;
                            lessonNode.Index = group.ChildCount();
                            lessonReference.Index = group.ChildCount();
                            lessonReference.Owner = group.Owner;
                            lessonReference.IsPublic = group.IsPublic;
                            group.AddChild(lessonReference);
                            group.AddChild(lessonReference);
                        }
                        else
                        {
                            lessonNode.Owner = parentGroup.Owner;
                            lessonNode.IsPublic = parentGroup.IsPublic;
                            lessonNode.Index = parentGroup.ChildCount();
                            lessonReference.Index = parentGroup.ChildCount();
                            lessonReference.Owner = parentGroup.Owner;
                            lessonReference.IsPublic = parentGroup.IsPublic;
                            parentGroup.AddChild(lessonReference);
                            parentGroupNode.AddChild(lessonReference);
                        }
                        course.AddNode(lessonNode);
                    }

                    SpeakerTextsComponent speakerTextsComponent = lesson.GetLabeledFirst(componentName, label) as SpeakerTextsComponent;

                    if (speakerTextsComponent == null)
                    {
                        if ((MasterReference != null) && (MasterReference.Item != null))
                        {
                            MasterComponentItem componentItem = MasterReference.Item.GetComponentItem(componentKey);
                            if (componentItem == null)
                                componentItem = MasterReference.Item.GetComponentItem(label);
                            if (componentItem == null)
                                componentItem = MasterReference.Item.GetComponentItem(componentName);
                            if (componentItem != null)
                            {
                                string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(Tree, lesson, null, null);
                                LessonUtilities.ConfigureComponentFromComponentItem(lesson, componentItem, Tree.KeyString, mediaDirectory);
                                speakerTextsComponent = lesson.GetTyped<SpeakerTextsComponent>(componentKey);
                            }
                        }
                        if (speakerTextsComponent == null)
                        {
                            string mediaDirectory = LessonUtilities.TreeNodeMediaTildeUrl(Tree, lesson, null, null);
                            speakerTextsComponent = LessonUtilities.CreateComponent(Tree.KeyString, lesson, componentName, componentKey, label, mediaDirectory) as SpeakerTextsComponent;
                            lesson.Add(speakerTextsComponent);
                        }
                    }

                    speakerTextsComponent.AddMultiLanguageItem(multiLanguageItem, speakerKey);
                    IncrementItemCounter(tag);
                }
                else if (Component is Plan)
                {
                    Plan plan = Component as Plan;
                    FlashList group = null;
                    FlashList parentGroup = null;
                    FlashList lesson = null;
                    TitledNodeReference lessonNode = null;
                    TitledNodeReference groupNode = null;
                    TitledNodeReference parentGroupNode = null;
                    LanguageID languageID = plan.LanguageID;
                    List<LanguageID> languageIDs = plan.LanguageIDs;
                    string groupTitle = GetGroupTitle(tag);
                    string lessonTitle = GetLessonTitle(tag);

                    if (SubDivide && !String.IsNullOrEmpty(tag))
                    {
                        parentGroupNode = plan.GetNodeWithSourceAndTitle("FlashLists", tag, UserRecord.UILanguage);

                        if (parentGroupNode != null)
                        {
                            if (parentGroupNode.Item == null)
                                parentGroupNode.ResolveReferences(Repositories, false, false);

                            parentGroup = parentGroupNode.TypedItemAs<FlashList>();

                            if (parentGroup == null)
                                throw new ModelException("Can't file parent flash list: " + tag);
                        }
                        else
                        {
                            parentGroup = new FlashList();
                            parentGroup.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, tag));
                            parentGroup.LanguageID = plan.LanguageID;
                            if (plan.LanguageIDs != null)
                                parentGroup.LanguageIDs = new List<LanguageID>(plan.LanguageIDs);
                            parentGroup.Index = plan.ChildCount();
                            parentGroup.Owner = plan.Owner;
                            parentGroup.IsPublic = plan.IsPublic;
                            if (!Repositories.SaveReference("FlashLists", null, parentGroup))
                                throw new ModelException("Error adding parent flash list: " + tag);
                            parentGroupNode = new TitledNodeReference("FlashLists", parentGroup);
                            parentGroupNode.Index = plan.ChildCount();
                            plan.AddNode(parentGroupNode);
                            TitledReference planChild = new TitledReference("FlashLists", parentGroup);
                            planChild.Index = parentGroup.Index;
                            plan.AddChild(planChild);
                        }
                    }

                    if (SubDivide)
                    {
                        if ((GetItemCounter(tag) % ItemSubDivideCount) == 0)
                        {
                            if (GetItemCounter(tag) != 0)
                                IncrementLessonCounter(tag);

                            groupTitle = GetGroupTitle(tag);
                            lessonTitle = GetLessonTitle(tag);

                            if ((GetLessonCounter(tag) % LessonSubDivideCount) == 0)
                            {
                                group = new FlashList();
                                group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                                group.LanguageID = plan.LanguageID;
                                if (plan.LanguageIDs != null)
                                    group.LanguageIDs = new List<LanguageID>(plan.LanguageIDs);
                                group.Index = (parentGroup != null ? parentGroup.ChildCount() : plan.ChildCount());
                                group.Owner = plan.Owner;
                                group.IsPublic = plan.IsPublic;
                                if (!Repositories.SaveReference("FlashLists", null, group))
                                    throw new ModelException("Error adding group flash list: " + groupTitle);
                                groupNode = new TitledNodeReference("FlashLists", group);
                                groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : plan.ChildCount());
                                plan.AddNode(groupNode);
                                TitledReference planChild = new TitledReference("FlashLists", group);
                                planChild.Index = group.Index;
                                if (parentGroup != null)
                                {
                                    parentGroup.AddChild(planChild);
                                    parentGroupNode.AddChild(planChild);
                                }
                                else
                                    plan.AddChild(planChild);
                            }
                            else
                            {
                                groupNode = Tree.GetNodeWithSourceAndTitle("FlashLists", groupTitle, UserRecord.UILanguage);

                                if (groupNode == null)
                                {
                                    group = new FlashList();
                                    group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                                    group.LanguageID = plan.LanguageID;
                                    if (plan.LanguageIDs != null)
                                        group.LanguageIDs = new List<LanguageID>(plan.LanguageIDs);
                                    group.Index = (parentGroup != null ? parentGroup.ChildCount() : plan.ChildCount());
                                    group.Owner = plan.Owner;
                                    group.IsPublic = plan.IsPublic;
                                    if (!Repositories.SaveReference("FlashLists", null, group))
                                        throw new ModelException("Error adding group flash list: " + groupTitle);
                                    groupNode = new TitledNodeReference("FlashLists", group);
                                    groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : plan.ChildCount());
                                    plan.AddNode(groupNode);
                                    TitledReference planChild = new TitledReference("FlashLists", group);
                                    planChild.Index = group.Index;
                                    if (parentGroup != null)
                                    {
                                        parentGroup.AddChild(planChild);
                                        parentGroupNode.AddChild(planChild);
                                    }
                                    else
                                        plan.AddChild(planChild);
                                }
                                else
                                {
                                    if (groupNode.Item == null)
                                        groupNode.ResolveReferences(Repositories, false, false);

                                    group = groupNode.TypedItemAs<FlashList>();

                                    if (group == null)
                                        throw new ModelException("Can't find group flash list: " + groupTitle);
                                }
                            }

                            lesson = new FlashList();
                            lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));
                            lesson.LanguageID = group.LanguageID;
                            if (group.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(group.LanguageIDs);
                            lesson.Index = group.ChildCount();
                            lesson.Owner = group.Owner;
                            lesson.IsPublic = group.IsPublic;
                            lesson.Parent = new TitledReference("FlashLists", group);
                            if (!Repositories.SaveReference("FlashLists", null, lesson))
                                throw new ModelException("Error adding flash list: " + tag);
                            TitledReference groupChild = new TitledReference("FlashLists", lesson);
                            groupChild.Index = group.ChildCount();
                            group.AddChild(groupChild);
                            lessonNode = new TitledNodeReference("FlashLists", lesson);
                            groupNode.AddChild(groupChild);
                            plan.AddNode(lessonNode);
                        }

                        if (groupNode == null)
                        {
                            groupNode = Tree.GetNodeWithSourceAndTitle("FlashLists", groupTitle, UserRecord.UILanguage);

                            if (groupNode != null)
                            {
                                group = groupNode.TypedItemAs<FlashList>();

                                if (group == null)
                                    throw new ModelException("Can't find group flash list: " + groupTitle);
                            }
                        }

                        if (groupNode == null)
                        {
                            group = new FlashList();
                            group.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, groupTitle));
                            group.LanguageID = plan.LanguageID;
                            if (plan.LanguageIDs != null)
                                group.LanguageIDs = new List<LanguageID>(plan.LanguageIDs);
                            group.Index = (parentGroup != null ? parentGroup.ChildCount() : plan.ChildCount());
                            group.Owner = plan.Owner;
                            group.IsPublic = plan.IsPublic;
                            if (!Repositories.SaveReference("FlashLists", null, group))
                                throw new ModelException("Error adding group: " + groupTitle);
                            groupNode = new TitledNodeReference("FlashLists", group);
                            groupNode.Index = (parentGroup != null ? parentGroup.ChildCount() : plan.ChildCount());
                            plan.AddNode(groupNode);
                            TitledReference planChild = new TitledReference("FlashLists", group);
                            planChild.Index = group.Index;
                            if (parentGroup != null)
                            {
                                parentGroup.AddChild(planChild);
                                parentGroupNode.AddChild(planChild);
                            }
                            else
                                plan.AddChild(planChild);
                        }
                    }

                    if (lessonNode == null)
                    {
                        lessonNode = plan.GetNodeWithSourceAndTitle("FlashLists", lessonTitle, UserRecord.UILanguage);

                        if (lessonNode != null)
                        {
                            lesson = lessonNode.TypedItemAs<FlashList>();

                            if (lesson == null)
                                throw new ModelException("Can't find flash list: " + lessonTitle);
                        }
                    }

                    if (lesson == null)
                    {
                        lesson = new FlashList();
                        lesson.Title = new MultiLanguageString("Title", new LanguageString("Title", UserRecord.UILanguage, lessonTitle));

                        if (group == null)
                            group = parentGroup;

                        if (group != null)
                        {
                            lesson.LanguageID = group.LanguageID;
                            if (group.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(group.LanguageIDs);
                            lesson.Index = group.ChildCount();
                            lesson.Owner = group.Owner;
                            lesson.IsPublic = group.IsPublic;
                            lesson.Parent = new TitledReference("FlashLists", group);
                            if (!Repositories.SaveReference("FlashLists", null, lesson))
                                throw new ModelException("Error adding flash list: " + tag);
                            TitledReference groupChild = new TitledReference("FlashLists", lesson);
                            groupChild.Index = group.ChildCount();
                            group.AddChild(groupChild);
                            lessonNode = new TitledNodeReference("FlashLists", lesson);
                            groupNode.AddChild(groupChild);
                        }
                        else
                        {
                            lesson.LanguageID = plan.LanguageID;
                            if (plan.LanguageIDs != null)
                                lesson.LanguageIDs = new List<LanguageID>(plan.LanguageIDs);
                            lesson.Index = plan.ChildCount();
                            lesson.Owner = plan.Owner;
                            lesson.IsPublic = plan.IsPublic;
                            if (!Repositories.SaveReference("FlashLists", null, lesson))
                                throw new ModelException("Error adding lesson: " + tag);
                            TitledReference planChild = new TitledReference("FlashLists", lesson);
                            planChild.Index = plan.ChildCount();
                            plan.AddChild(planChild);
                            lessonNode = new TitledNodeReference("FlashLists", lesson);
                        }
                        plan.AddNode(lessonNode);
                    }

                    if (lessonNode == null)
                    {
                        lessonNode = new TitledNodeReference("FlashLists", lesson);
                        TitledReference lessonReference = new TitledReference("FlashLists", lesson);

                        if (group != null)
                        {
                            lessonNode.Owner = group.Owner;
                            lessonNode.IsPublic = group.IsPublic;
                            lessonNode.Index = group.ChildCount();
                            lessonReference.Index = group.ChildCount();
                            lessonReference.Owner = group.Owner;
                            lessonReference.IsPublic = group.IsPublic;
                            group.AddChild(lessonReference);
                            groupNode.AddChild(lessonReference);
                        }
                        else
                        {
                            lessonNode.Owner = plan.Owner;
                            lessonNode.IsPublic = plan.IsPublic;
                            lessonNode.Index = plan.ChildCount();
                            lessonReference.Index = plan.ChildCount();
                            lessonReference.Owner = plan.Owner;
                            lessonReference.IsPublic = plan.IsPublic;
                            plan.AddChild(lessonReference);
                        }
                        plan.AddNode(lessonNode);
                    }

                    lesson.LoadEntry(multiLanguageItem);
                    IncrementItemCounter(tag);
                }
            }
        }

        protected void AddEntryList(List<MultiLanguageItem> multiLanguageItems)
        {
            if (Component == null)
                return;

            foreach (MultiLanguageItem multiLanguageItem in multiLanguageItems)
                AddEntry(multiLanguageItem);
        }

        protected void AddSpeakerTextList(List<SpeakerText> speakerTexts)
        {
            if (Component == null)
                return;

            foreach (SpeakerText speakerText in speakerTexts)
                AddEntry(speakerText, speakerText.SpeakerKey);
        }

        protected MultiLanguageItem GetEntry()
        {
            MultiLanguageItem multiLanguageItem = null;

            if (Component == null)
                return multiLanguageItem;

            if (Component is SpeakerTextsComponent)
                multiLanguageItem = GetComponent<SpeakerTextsComponent>().GetIndexed(ItemIndex);
            else if (Component is FlashList)
            {
                FlashEntry flashEntry = GetComponent<FlashList>().GetIndexed(ItemIndex);

                if (flashEntry != null)
                    multiLanguageItem = flashEntry.Item;
            }

            if (multiLanguageItem != null)
                ItemIndex = ItemIndex + 1;

            return multiLanguageItem;
        }

        protected SpeakerText GetSpeakerText()
        {
            MultiLanguageItem multiLanguageItem = null;
            SpeakerText speakerText = null;

            if (Component == null)
                return speakerText;

            if (Component is SpeakerTextsComponent)
                speakerText = GetComponent<SpeakerTextsComponent>().GetIndexed(ItemIndex);
            else if (Component is FlashList)
            {
                FlashEntry flashEntry = GetComponent<FlashList>().GetIndexed(ItemIndex);

                if (flashEntry != null)
                    multiLanguageItem = flashEntry.Item;

                speakerText = new SpeakerText(multiLanguageItem.Key, multiLanguageItem.LanguageItems);
            }

            if (speakerText != null)
                ItemIndex = ItemIndex + 1;

            return speakerText;
        }

        protected List<MultiLanguageItem> GetEntryList()
        {
            List<MultiLanguageItem> multiLanguageItems = new List<MultiLanguageItem>(GetEntryCount());
            MultiLanguageItem multiLanguageItem = null;

            if (Component == null)
                return multiLanguageItems;

            for (ItemIndex = 0; ItemIndex < ItemCount; ItemIndex = ItemIndex + 1)
            {
                multiLanguageItem = GetEntry();
                multiLanguageItems.Add(multiLanguageItem);
            }

            ItemIndex = 0;

            return multiLanguageItems;
        }

        protected List<SpeakerText> GetSpeakerTextList()
        {
            List<SpeakerText> speakerTexts = new List<SpeakerText>(GetEntryCount());
            SpeakerText speakerText = null;

            if (Component == null)
                return speakerTexts;

            for (ItemIndex = 0; ItemIndex < ItemCount; ItemIndex = ItemIndex + 1)
            {
                speakerText = GetSpeakerText();
                speakerTexts.Add(speakerText);
            }

            ItemIndex = 0;

            return speakerTexts;
        }

        protected int GetEntryCount()
        {
            int returnValue = 0;

            if (Component == null)
                return returnValue;

            if (Component is SpeakerTextsComponent)
                returnValue = GetComponent<SpeakerTextsComponent>().Count() - ItemIndex;
            else if (Component is FlashList)
                returnValue = GetComponent<FlashList>().Count() - ItemIndex;

            return returnValue;
        }

        public LanguageID HostLanguageID
        {
            get
            {
                if (LanguageDescriptors == null)
                    return null;
                LanguageDescriptor languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == "Host");
                if (languageDescriptor != null)
                    return languageDescriptor.LanguageID;
                return null;
            }
        }

        public LanguageID TargetLanguageID
        {
            get
            {
                if (LanguageDescriptors == null)
                    return null;
                LanguageDescriptor languageDescriptor = LanguageDescriptors.FirstOrDefault(x => x.Name == "Target");
                if (languageDescriptor != null)
                    return languageDescriptor.LanguageID;
                return null;
            }
        }

        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            return false;
        }

        public override bool IsSupportedVirtual(string importExport, string componentName, string capability)
        {
            return IsSupportedStatic(importExport, componentName, capability);
        }

        public static new string TypeStringStatic { get { return "Component"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
