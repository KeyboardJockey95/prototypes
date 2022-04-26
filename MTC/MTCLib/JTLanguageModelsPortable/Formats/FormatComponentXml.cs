using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatComponentXml : FormatComponent
    {
        public bool ChildrenOnly { get; set; }
        private bool SavedChildrenOnly;
        private List<IBase> References;

        public FormatComponentXml(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, TypeStringStatic, userRecord, repositories, translator, languageUtilities)
        {
            SetupDefaults(userRecord);
        }

        public FormatComponentXml(TitledTree tree, string targetName, string targetOwner, List<IBaseXml> targetList,
                List<LanguageDescriptor> languageDescriptors, int itemIndex,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, targetName, targetOwner, targetList, languageDescriptors, itemIndex, TypeStringStatic, userRecord,
                repositories, translator, languageUtilities)
        {
            SetupDefaults(userRecord);
        }

        public FormatComponentXml(TitledTree tree, IBaseXml target, List<LanguageDescriptor> languageDescriptors, int itemIndex, string typeName,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, target, languageDescriptors, itemIndex, typeName, userRecord, repositories, translator, languageUtilities)
        {
            SetupDefaults(userRecord);
        }

        public FormatComponentXml(TitledTree tree, string targetName, string targetOwner, List<IBaseXml> targetList,
                List<LanguageDescriptor> languageDescriptors, int itemIndex, string typeName,
                UserRecord userRecord, IMainRepository repositories, ITranslator translator, LanguageUtilities languageUtilities)
            : base(tree, targetName, targetOwner, targetList, languageDescriptors, itemIndex, typeName, userRecord,
                repositories, translator, languageUtilities)
        {
            SetupDefaults(userRecord);
        }

        protected virtual void SetupDefaults(UserRecord userRecord)
        {
            switch (GetTargetName())
            {
                case "Text":
                case "Lesson":
                case "LessonGroup":
                case "Course":
                case "Plan":
                case "FlashList":
                    ChildrenOnly = false;
                    SavedChildrenOnly = ChildrenOnly;
                    break;
                default:
                    break;
            }
            DefaultFileExtension = ".xml";
        }

        public static string ChildrenOnlyHelp = "Check this if the XML file only has the child objects.";

        public override void LoadFromArguments()
        {
            if (Arguments == null)
                return;

            base.LoadFromArguments();

            switch (GetTargetName())
            {
                case "Text":
                case "Lesson":
                case "LessonGroup":
                case "Course":
                case "Plan":
                case "FlashList":
                    SavedChildrenOnly = ChildrenOnly = GetFlagArgumentDefaulted("ChildrenOnly", "flag", "r",
                        SavedChildrenOnly, "Children only", ChildrenOnlyHelp);
                    break;
                default:
                    break;
            }
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            switch (GetTargetName())
            {
                case "Text":
                case "Lesson":
                case "LessonGroup":
                case "Course":
                case "Plan":
                case "FlashList":
                    SetFlagArgument("ChildrenOnly", "flag", "r", ChildrenOnly, "Children only", ChildrenOnlyHelp);
                    break;
                default:
                    break;
            }
        }

        protected override void ReadObject(StreamReader reader, IBaseXml obj)
        {
            List<TitledNode> items = new List<TitledNode>();
            List<TitledNode> topItems = new List<TitledNode>();
            if ((obj != null) && (obj is TitledNode))
                topItems.Add(obj as TitledNode);
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
            else if ((obj is TitledBase) || (obj == null))
            {
                XElement element = XElement.Load(reader, LoadOptions.PreserveWhitespace);

                if (element == null)
                    throw new ModelException(Error = "Unexpected format.");

                string rootName = element.Name.LocalName;

                if (rootName == "JTLanguage")
                {
                    TitledNode item;
                    TitledBase baseItem;
                    string source;
                    TitledNode parentItem = obj as TitledNode;
                    object originalParentKey = (parentItem != null ? parentItem.Key : null);
                    bool addToParent = (Tree == null ? false : true);

                    List<XElement> elements = element.Elements().ToList();
                    XElement referencesElement = null;

                    if (elements.Count() == 0)
                        return;

                    int parentIndex = 0;
                    XElement parentElement = elements.ElementAt(0);

                    Repositories.StartFixups();

                    if ((parentElement != null) && (parentElement.Name.LocalName == "References"))
                    {
                        referencesElement = parentElement;
                        LoadReferences(parentElement);

                        if (elements.Count() == 1)
                        {
                            BaseXml.InImport = true;
                            Repositories.EndFixups();
                            BaseXml.InImport = false;
                            return;
                        }

                        parentElement = elements.ElementAt(1);
                        parentIndex = 1;
                    }

                    if ((parentItem != null) && (parentElement.Name.LocalName == parentItem.GetType().Name) && !ChildrenOnly)
                    {
                        elements.RemoveAt(parentIndex);

                        parentItem.OnElement(parentElement);
                        parentItem.Modified = true;
                        addToParent = false;
                    }

                    if ((parentItem == null) && (obj != null))
                    {
                        object oldKey = null;
                        string oldOwner = null;

                        if (obj is IBase)
                            oldKey = (obj as IBase).Key;

                        if (obj is TitledBase)
                            oldOwner = (obj as TitledBase).Owner;

                        try
                        {
                            BaseXml.InImport = true;
                            obj.OnElement(parentElement);
                        }
                        catch (Exception exception)
                        {
                            BaseXml.InImport = true;
                            Repositories.EndFixups();
                            BaseXml.InImport = false;
                            throw exception;
                        }

                        BaseXml.InImport = false;

                        if (obj is IBase)
                        {
                            (obj as IBase).Key = oldKey;
                            (obj as IBase).Modified = true;
                        }

                        if (obj is TitledBase)
                            (obj as TitledBase).Owner = oldOwner;
                    }
                    else
                    {
                        object parentKey = (parentItem != null ? parentItem.Key : null);
                        string parentSource = (parentItem != null ? LessonUtilities.ObjectSourceFromObject(parentItem) : "");

                        foreach (XElement childElement in elements)
                        {
                            item = null;
                            baseItem = null;

                            BaseXml.InImport = true;

                            switch (childElement.Name.LocalName)
                            {
                                case "Course":
                                    item = new Course(childElement);
                                    source = "Courses";
                                    break;
                                case "LessonGroup":
                                    item = new LessonGroup(childElement);
                                    source = "LessonGroups";
                                    break;
                                case "Lesson":
                                    item = new Lesson(childElement);
                                    source = "Lessons";
                                    break;
                                case "Plan":
                                    item = new Plan(childElement);
                                    source = "Plans";
                                    if (References != null)
                                    {
                                        PlanConfigurations planConfigurations = References.FirstOrDefault(x => x.MatchKey(item.Key)) as PlanConfigurations;
                                        if (planConfigurations != null)
                                        {
                                            planConfigurations.Key = item.Key;
                                            if (!Repositories.SaveReference("PlanConfigurations", null, planConfigurations))
                                            {
                                                if (!Repositories.UpdateReference("PlanConfigurations", null, planConfigurations))
                                                {
                                                    throw new ModelException("Error plan configurations: " +
                                                        (planConfigurations.Title != null ? planConfigurations.Title.Text(HostLanguageID) : planConfigurations.KeyString));
                                                }
                                            }
                                            References.Remove(planConfigurations);
                                        }
                                    }
                                    break;
                                case "PlanConfigurations":
                                    baseItem = new PlanConfigurations(childElement);
                                    source = "PlanConfigurations";
                                    break;
                                case "FlashList":
                                    item = new FlashList(childElement);
                                    source = "FlashLists";
                                    break;
                                case "LessonMaster":
                                    baseItem = new LessonMaster(childElement);
                                    source = "LessonMasters";
                                    break;
                                case "MarkupTemplate":
                                    baseItem = new MarkupTemplate(childElement);
                                    source = "MarkupTemplates";
                                    break;
                                case "References":
                                    if (referencesElement == null)
                                        LoadReferences(childElement);
                                    continue;
                                default:
                                    BaseXml.InImport = true;
                                    Repositories.EndFixups();
                                    BaseXml.InImport = false;
                                    throw new ModelException("Unexpected child element: " + childElement.Name.LocalName);
                            }

                            BaseXml.InImport = false;

                            if (item != null)
                            {
                                items.Add(item);
                                baseItem = item;
                            }

                            baseItem.Owner = Owner;

                            if (!Repositories.SaveReference(source, null, baseItem))
                            {
                                if (!Repositories.UpdateReference(source, null, baseItem))
                                {
                                    BaseXml.InImport = true;
                                    Repositories.EndFixups();
                                    BaseXml.InImport = false;
                                    throw new ModelException("Error adding child item: " +
                                        (baseItem.Title != null ? baseItem.Title.Text(HostLanguageID) : baseItem.KeyString));
                                }
                            }

                            if (!LessonUtilities.AddHeader(baseItem, source))
                            {
                                BaseXml.InImport = true;
                                Repositories.EndFixups();
                                BaseXml.InImport = false;
                                throw new ModelException("Error adding tree header: " +
                                    (baseItem.Title != null ? baseItem.Title.Text(HostLanguageID) : baseItem.KeyString));
                            }
                        }

                        if (parentItem != null)
                        {
                            BaseXml.InImport = true;
                            Repositories.FixupObject(parentItem);
                            Repositories.EndFixups();
                            BaseXml.InImport = false;

                            if (!parentItem.MatchKey(originalParentKey))
                            {
                                parentItem.Key = originalParentKey;

                                string itemSource = LessonUtilities.ObjectSourceFromObject(parentItem);

                                if (parentItem.HasChildren())
                                {
                                    foreach (TitledReference childReference in parentItem.Children)
                                    {
                                        TitledNode childItem = childReference.TypedItemAs<TitledNode>();

                                        if (childItem == null)
                                            childItem = items.FirstOrDefault(x => x.MatchKey(childReference.Key));

                                        if (childItem != null)
                                        {
                                            if (childReference.Item == null)
                                                childReference.Item = childItem;

                                            if (!(parentItem is TitledTree))
                                                childItem.Parent = new TitledReference(itemSource, parentItem.Key, parentItem);
                                        }
                                    }
                                }
                            }

                            foreach (TitledNode itemNode in items)
                            {
                                string itemSource = LessonUtilities.ObjectSourceFromObject(itemNode);

                                if (itemNode.HasChildren())
                                {
                                    foreach (TitledReference childReference in itemNode.Children)
                                    {
                                        TitledNode childItem = childReference.TypedItemAs<TitledNode>();

                                        if (childItem == null)
                                            childItem = items.FirstOrDefault(x => x.MatchKey(childReference.Key));

                                        if (childItem != null)
                                        {
                                            if (childReference.Item == null)
                                                childReference.Item = childItem;

                                            if (!childItem.HasParent())
                                                childItem.Parent = new TitledReference(itemSource, itemNode.Key, itemNode);
                                        }
                                    }
                                }

                                if (!itemNode.HasParent() && !(parentItem is TitledTree))
                                {
                                    itemNode.Parent = new TitledReference(parentSource, originalParentKey, parentItem);

                                    if (addToParent)
                                        parentItem.AddChild(new TitledReference(itemSource, itemNode.Key, itemNode));
                                }

                                if ((Tree != null) && (Tree.GetNodeWithSource(itemNode.Key, itemSource) == null))
                                {
                                    TitledNodeReference parentNode = null;

                                    if (itemNode.HasParent())
                                        parentNode = Tree.GetNodeWithSource(itemNode.ParentKey, itemNode.ParentSource);

                                    LessonUtilities.CreateTreeNode(Tree, parentNode, itemNode, itemSource);
                                }

                                if (itemNode.Modified)
                                {
                                    if (!Repositories.UpdateReference(itemSource, null, itemNode))
                                        throw new ModelException("Error updating child item: " + itemNode.GetTitleString(HostLanguageID));
                                }
                            }

                            //if (!(parentItem is Course))
                            //    LessonUtilities.ExtractNodeHierarchyFromItemList(Tree, items);
                        }
                        else
                        {
                            BaseXml.InImport = true;
                            Repositories.EndFixups();
                            BaseXml.InImport = false;
                        }
                    }
                }
                else if (obj != null)
                {
                    object oldKey = null;
                    string oldOwner = null;

                    if (obj is IBase)
                        oldKey = (obj as IBase).Key;

                    if (obj is TitledBase)
                        oldOwner = (obj as TitledBase).Owner;

                    try
                    {
                        BaseXml.InImport = true;
                        obj.OnElement(element);
                    }
                    catch (Exception exception)
                    {
                        BaseXml.InImport = true;
                        Repositories.EndFixups();
                        BaseXml.InImport = false;
                        throw exception;
                    }

                    BaseXml.InImport = false;

                    if (obj is IBase)
                    {
                        (obj as IBase).Key = oldKey;
                        (obj as IBase).Modified = true;
                    }

                    if (obj is TitledBase)
                        (obj as TitledBase).Owner = oldOwner;
                }
                else
                {
                    TitledBase item;
                    string source;

                    Repositories.StartFixups();

                    BaseXml.InImport = true;

                    switch (element.Name.LocalName)
                    {
                        case "Course":
                            item = new Course(element);
                            source = "Courses";
                            break;
                        case "Plan":
                            item = new Plan(element);
                            source = "Plans";
                            if (References != null)
                            {
                                PlanConfigurations planConfigurations = References.FirstOrDefault(x => x.MatchKey(item.Key)) as PlanConfigurations;
                                if (planConfigurations != null)
                                {
                                    planConfigurations.Key = item.Key;
                                    if (!Repositories.SaveReference("PlanConfigurations", null, planConfigurations))
                                    {
                                        if (!Repositories.UpdateReference("PlanConfigurations", null, planConfigurations))
                                        {
                                            throw new ModelException("Error plan configurations: " +
                                                (planConfigurations.Title != null ? planConfigurations.Title.Text(HostLanguageID) : planConfigurations.KeyString));
                                        }
                                    }
                                    References.Remove(planConfigurations);
                                }
                            }
                            break;
                        case "LessonMaster":
                            item = new LessonMaster(element);
                            source = "LessonMasters";
                            break;
                        case "MarkupTemplate":
                            item = new MarkupTemplate(element);
                            source = "MarkupTemplates";
                            break;
                        default:
                            BaseXml.InImport = true;
                            Repositories.EndFixups();
                            BaseXml.InImport = false;
                            throw new ModelException("Unexpected element: " + element.Name.LocalName);
                    }

                    BaseXml.InImport = false;

                    item.Owner = Owner;

                    if (!Repositories.SaveReference(source, null, item))
                    {
                        if (!Repositories.UpdateReference(source, null, item))
                        {
                            BaseXml.InImport = true;
                            Repositories.EndFixups();
                            BaseXml.InImport = false;
                            throw new ModelException("Error adding child item: " +
                                (item.Title != null ? item.Title.Text(HostLanguageID) : item.KeyString));
                        }
                    }

                    BaseXml.InImport = true;
                    Repositories.EndFixups();
                    BaseXml.InImport = false;
                }
            }
            if ((Target == null) && ((TargetList == null) || (TargetList.Count() == 0)))
            {
                int itemCount = items.Count();
                int itemIndex;

                for (itemIndex = itemCount - 1; itemIndex >= 0; itemIndex--)
                {
                    TitledNode item = items[itemIndex];
                    object key = item.Key;
                    string source = LessonUtilities.ObjectSourceFromObject(item);
                    int innerIndex;
                    int innerCount = itemIndex - 1;
                    bool notFound = true;

                    for (innerIndex = itemIndex - 1; innerIndex >= 0; innerIndex--)
                    {
                        TitledNode innerItem = items[innerIndex];

                        if (innerItem.GetChildWithSource(key, source) != null)
                        {
                            notFound = false;
                            break;
                        }
                    }

                    if (notFound)
                        topItems.Add(item);
                }

                if (topItems.Count() != 0)
                {
                    if (topItems.Count() == 1)
                        Target = topItems.First();
                    else
                        TargetList = new List<IBaseXml>(topItems);
                }
            }
        }

        protected override void ReadComponent(StreamReader reader)
        {
            ItemCount = 0;

            XElement element = XElement.Load(reader, LoadOptions.PreserveWhitespace);

            if (element == null)
                throw new ModelException(Error = "Unexpected format.");

            Repositories.StartFixups();

            XElement itemElement = element;

            if (element.Name.LocalName == "JTLanguage")
            {
                List<XElement> elements = element.Elements().ToList();

                if (elements.Count() == 0)
                    return;

                itemElement = elements.ElementAt(0);

                if ((itemElement != null) && (itemElement.Name.LocalName == "References"))
                {
                    LoadReferences(itemElement);

                    if (elements.Count() == 1)
                    {
                        BaseXml.InImport = true;
                        Repositories.EndFixups();
                        BaseXml.InImport = false;
                        ItemCount = 0;
                        return;
                    }

                    itemElement = elements.ElementAt(1);
                }
            }

            if (itemElement != null)
            {
                if (itemElement.Name.LocalName != Component.GetType().Name)
                {
                    BaseXml.InImport = true;
                    Repositories.EndFixups();
                    BaseXml.InImport = false;
                    throw new ModelException("Can't import a " + Component.GetType().Name + " from a " + itemElement.Name.LocalName + ".");
                }

                object oldKey = null;
                string oldOwner = null;

                if (Component is IBase)
                    oldKey = (Component as IBase).Key;

                if (Component is TitledBase)
                    oldOwner = (Component as TitledBase).Owner;

                try
                {
                    BaseXml.InImport = true;
                    Component.OnElement(itemElement);
                }
                catch (Exception exception)
                {
                    BaseXml.InImport = true;
                    Repositories.EndFixups();
                    BaseXml.InImport = false;
                    throw exception;
                }

                BaseXml.InImport = false;

                if (Component is IBase)
                {
                    (Component as IBase).Key = oldKey;
                    (Component as IBase).Modified = true;
                }

                if (Component is TitledBase)
                    (Component as TitledBase).Owner = oldOwner;
            }

            BaseXml.InImport = true;
            Repositories.EndFixups();
            BaseXml.InImport = false;

            ItemCount = GetEntryCount();
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
                    XElement element = new XElement("JTLanguage");
                    foreach (IBaseXml target in TargetList)
                        CollectReferences(target);
                    SaveReferences(element);
                    foreach (IBaseXml target in TargetList)
                    {
                        if (target is TitledNode)
                        {
                            TitledNode item = target as TitledNode;
                            ProcessNode(element, item);
                        }
                        else
                            element.Add(target.Xml);
                    }
                    element.Save(writer);
                }
                else
                    WriteObject(writer, Target);

                writer.Flush();

#if !PORTABLE
                writer.Close();
#endif
            }
        }

        protected override void WriteObject(StreamWriter writer, IBaseXml obj)
        {
            if ((obj is LessonComponent) || (obj is FlashList))
            {
                Component = obj;
                if (IsSupportedVirtual("Export", GetComponentName(), "Support"))
                {
                    WriteComponent(writer);
                    ComponentIndex = ComponentIndex + 1;
                }
                Component = null;
            }
            else if (obj is TitledNode)
            {
                TitledNode item = obj as TitledNode;
                XElement element = new XElement("JTLanguage");
                CollectReferences(item);
                SaveReferences(element);
                ProcessNode(element, item);
                element.Save(writer);
            }
            else if (obj is IBaseXml)
            {
                XElement element = new XElement("JTLanguage");
                CollectReferences(obj);
                SaveReferences(element);
                XElement childElement = obj.Xml;
                element.Add(childElement);
                element.Save(writer);
            }
        }

        protected virtual void ProcessNode(XElement element, TitledNode item)
        {
            if (!item.IsPublic && !UserRecord.IsAdministrator() && (item.Owner != UserRecord.TeamOrUserName))
                return;

            if ((item != Target) && (NodeFlags != null))
            {
                string source = LessonUtilities.ObjectSourceFromObject(item);
                Boolean exportIt = false;

                NodeFlags.TryGetValue(LessonUtilities.KeyAndSourceString(item.KeyString, source), out exportIt);

                if (!exportIt)
                    return;
            }

            if ((Target != null) && (Target is Plan))
            {
                if (!(item is Lesson) && !(item is LessonGroup) && !(item is Course))
                    element.Add(item.Xml);
            }
            else
                element.Add(item.Xml);

            if (item.HasChildren())
            {
                foreach (TitledReference child in item.Children)
                {
                    TitledNode childItem = child.TypedItemAs<TitledNode>();

                    if (childItem != null)
                        ProcessNode(element, childItem);
                }
            }
        }

        protected override void WriteComponent(StreamWriter writer)
        {
            ItemCount = GetEntryCount();
            XElement element = new XElement("JTLanguage");
            CollectReferences(Component);
            SaveReferences(element);
            XElement componentElement = Component.Xml;
            element.Add(componentElement);
            element.Save(writer);
        }

        protected void CollectReferences(IBaseXml item)
        {
            if (item == null)
                return;

            if ((Target != null) && (Target is Plan))
            {
                if ((item is Lesson) || (item is LessonGroup) || (item is Course))
                    return;
            }

            if (item is Lesson)
            {
                Lesson lesson = item as Lesson;
                LessonMaster lessonMaster = lesson.LessonMaster;

                if (lessonMaster != null)
                {
                    if (References == null)
                        References = new List<IBase>();

                    if (References.FirstOrDefault(x => (x is LessonMaster) && x.MatchKey(lessonMaster.Key)) == null)
                    {
                        CollectReferences(lessonMaster);
                        References.Add(lessonMaster);
                    }
                }

                if (lesson.Count() != 0)
                {
                    foreach (LessonComponent lessonComponent in lesson.LessonComponents)
                    {
                        if (lessonComponent is DocumentComponent)
                        {
                            DocumentComponent documentComponent = lessonComponent as DocumentComponent;
                            MarkupReference markupReference = documentComponent.MarkupReference;

                            if (markupReference != null)
                            {
                                MarkupTemplate markupTemplate = markupReference.Item;

                                if (markupTemplate == null)
                                {
                                    markupReference.ResolveReference(Repositories, "MarkupTemplates");
                                    markupTemplate = markupReference.Item;
                                }

                                if (markupTemplate != null)
                                {
                                    if (References == null)
                                        References = new List<IBase>();

                                    if (References.FirstOrDefault(x => (x is MarkupTemplate) && x.MatchKey(markupTemplate.Key)) == null)
                                        References.Add(markupTemplate);
                                }
                            }
                        }
                    }
                }
            }
            else if (item is DocumentComponent)
            {
                DocumentComponent documentComponent = item as DocumentComponent;
                MarkupReference markupReference = documentComponent.MarkupReference;

                if (markupReference != null)
                {
                    MarkupTemplate markupTemplate = markupReference.Item;

                    if (markupTemplate == null)
                    {
                        markupReference.ResolveReference(Repositories, "MarkupTemplates");
                        markupTemplate = markupReference.Item;
                    }

                    if (markupTemplate != null)
                    {
                        if (References == null)
                            References = new List<IBase>();

                        if (References.FirstOrDefault(x => (x is MarkupTemplate) && x.MatchKey(markupTemplate.Key)) == null)
                            References.Add(markupTemplate);
                    }
                }
            }
            else if (item is LessonMaster)
            {
                LessonMaster lessonMaster = item as LessonMaster;

                if (lessonMaster.ComponentItems != null)
                {
                    foreach (MasterComponentItem componentItem in lessonMaster.ComponentItems)
                    {
                        MarkupTemplate markupTemplate = null;
                        string key = null;

                        if (!String.IsNullOrEmpty(componentItem.MarkupTemplateKey) && !componentItem.IsLocalMarkupTemplate)
                        {
                            if ((References == null) ||
                                (References.FirstOrDefault(x =>
                                    (x is MarkupTemplate) && ((x as MarkupTemplate).KeyString == componentItem.MarkupTemplateKey)) == null))
                               key = componentItem.MarkupTemplateKey;
                        }
                        else if (!String.IsNullOrEmpty(componentItem.CopyMarkupTemplateKey))
                        {
                            if ((References == null) ||
                                (References.FirstOrDefault(x =>
                                    (x is MarkupTemplate) && ((x as MarkupTemplate).KeyString == componentItem.MarkupTemplateKey)) == null))
                                key = componentItem.CopyMarkupTemplateKey;
                        }

                        if (!String.IsNullOrEmpty(key))
                            markupTemplate = Repositories.MarkupTemplates.Get(
                                LessonUtilities.KeyObject(key, "MarkupTemplates"));

                        if (markupTemplate != null)
                        {
                            if (References == null)
                                References = new List<IBase>();

                            References.Add(markupTemplate);
                        }
                    }
                }
            }
            else if (item is TitledNode)
            {
                TitledNode titledNode = item as TitledNode;

                if (item is Plan)
                {
                    Plan plan = item as Plan;
                    PlanConfigurations planConfigurations = plan.PlanConfigurations;

                    if (planConfigurations == null)
                        planConfigurations = Repositories.PlanConfigurations.Get(plan.Key);

                    if (planConfigurations != null)
                    {
                        if (References == null)
                            References = new List<IBase>();

                        References.Add(planConfigurations);
                    }
                }

                if (titledNode.HasChildren())
                {
                    foreach (TitledReference child in titledNode.Children)
                    {
                        TitledNode childItem = child.TypedItemAs<TitledNode>();

                        if (childItem != null)
                            CollectReferences(childItem);
                    }
                }
            }
        }

        protected void SaveReferences(XElement element)
        {
            if ((References != null) && (References.Count() != 0))
            {
                XElement referencesElement = new XElement("References");

                foreach (IBase item in References)
                {
                    XElement referenceElement = item.Xml;
                    referencesElement.Add(referenceElement);
                }

                element.Add(referencesElement);
            }
        }

        protected void LoadReferences(XElement element)
        {
            List<XElement> elements = element.Elements().ToList();
            List<IBase> items = new List<IBase>(elements.Count());

            if (elements.Count() == 0)
                return;

            foreach (XElement childElement in elements)
            {
                IBase item = null;
                string source = "";

                BaseXml.InImport = true;

                switch (childElement.Name.LocalName)
                {
                    case "LessonMaster":
                        item = new LessonMaster(childElement);
                        source = "LessonMasters";
                        break;
                    case "MarkupTemplate":
                        item = new MarkupTemplate(childElement);
                        source = "MarkupTemplates";
                        break;
                    case "PlanConfigurations":
                        item = new PlanConfigurations(childElement);
                        source = "PlanConfigurations";
                        if (References == null)
                            References = new List<IBase>();
                        References.Add(item);
                        item = null;
                        break;
                    default:
                        throw new ModelException("Unexpected reference element: " + childElement.Name.LocalName);
                }

                BaseXml.InImport = false;

                if (item != null)
                {
                    items.Add(item);

                    if (!Repositories.SaveNamedReference(source, null, item, false))
                    {
                        throw new ModelException("Error loading reference: " + item.Name);
                    }
                }
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
                case "LessonMaster":
                case "MarkupTemplate":
                    if (capability == "Support")
                        return true;
                    return false;
                case "Lesson":
                case "LessonGroup":
                case "Course":
                case "Plan":
                    if (capability == "Support")
                        return true;
                    else if (capability == "NodeFlags")
                    {
                        if (importExport == "Export")
                            return true;
                    }
                    return false;
                case "Courses":
                case "Plans":
                case "PlanConfigurations":
                    if (capability == "Support")
                        return true;
                    return false;
                case "Dictionary":
                default:
                    return false;
            }
        }

        public override bool IsSupportedVirtual(string importExport, string componentName, string capability)
        {
            return IsSupportedStatic(importExport, componentName, capability);
        }

        public static new string TypeStringStatic { get { return "JTLanguage XML"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
