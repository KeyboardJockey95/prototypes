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
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatXml : Format
    {
        protected XElement RootXml = null;
        protected int Version = 1;
        private static string FormatDescription = "Native JTLanguage object format based on XML.  Note: This format does not include media."
            + " Use JTLanguage Package or Chunky formats to also include media files.";

        public FormatXml()
            : base("JTLanguage XML", "FormatXml", FormatDescription, String.Empty, String.Empty,
                  "text/xml", ".xml", null, null, null, null, null)
        {
        }

        public FormatXml(FormatXml other)
            : base(other)
        {
        }

        // For derived classes.
        public FormatXml(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension, userRecord, userProfile, repositories, languageUtilities,
                  nodeUtilities)
        {
        }

        public override Format Clone()
        {
            return new FormatXml(this);
        }

        public override void Read(Stream stream)
        {
            ItemCount = 0;
            ResetProgress();

            //if (Timer != null)
            //    Timer.Start();

            DeleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);

            if (DeleteBeforeImport)
                DeleteFirst();

            StartFixups();

            try
            {
                using (StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding(true)))
                {
                    RootXml = XElement.Load(reader, LoadOptions.PreserveWhitespace);

                    if (RootXml.Name.LocalName != "JTLanguage")
                        throw new ObjectException("Invalid format.  XML root should be \"JTLanguage\".");

                    XAttribute versionAttribute = RootXml.Attribute("Version");

                    if (versionAttribute == null)
                        throw new ObjectException("No \"Version\" attribute in root XML element. Format is too old.");

                    if (Convert.ToInt32(versionAttribute.Value) != Version)
                        throw new ObjectException("Format is too new for this version of JTLanguage.");

                    List<XElement> elements = RootXml.Elements().ToList();
                    int elementCount = elements.Count();
                    int elementIndex = 0;

                    ContinueProgress(elementCount + 1 + ProgressCountBase);

                    if ((Targets != null) && (Targets.Count() != 0))
                    {
                        if (Targets.Count() == 1)
                        {
                            if (Targets[0] is BaseObjectNode)
                            {
                                BaseObjectNode node = Targets[0] as BaseObjectNode;

                                if (!node.IsTree())
                                    Tree = node.Tree;
                            }
                        }
                    }

                    if (elementCount >= 1)
                    {
                        if (elements.First().Name.LocalName == "References")
                        {
                            XElement referencesElement = elements.First();
                            elements.RemoveAt(0);
                            elementCount--;
                            UpdateProgressElapsed("Loading references...");
                            LoadReferences(referencesElement.Elements().ToList());
                        }
                    }

                    if (elementCount >= 1)
                    {
                        if (elements.First().Name.LocalName == "ExternalSavedChildren")
                        {
                            XElement externalSavedChildrenElement = elements.First();
                            elements.RemoveAt(0);
                            elementCount--;
                            UpdateProgressElapsed("Loading subordinate saved items...");
                            LoadExternalSavedChildren(externalSavedChildrenElement.Elements().ToList());
                        }
                    }

                    if (elementCount >= 1)
                    {
                        if (elements.First().Name.LocalName == "ExternalNonSavedChildren")
                        {
                            XElement externalNonSavedChildrenElement = elements.First();
                            elements.RemoveAt(0);
                            elementCount--;
                            UpdateProgressElapsed("Loading subordinate non-saved items...");
                            LoadExternalNonSavedChildren(externalNonSavedChildrenElement.Elements().ToList());
                        }
                    }

                    if ((Targets != null) && (Targets.Count() != 0))
                    {
                        if (Targets.Count() == 1)
                        {
                            if (elementCount == 0)
                                throw new ObjectException("No target elements in XML.");

                            XElement element = elements[elementIndex++];
                            IBaseObject target = Targets.First();

                            UpdateProgressElapsed("Loading target " + element.Name.LocalName + "...");
                            ReadTarget(target, element);

                            elementIndex++;

                            while (elementIndex < elementCount)
                            {
                                element = elements[elementIndex++];
                                UpdateProgressElapsed("Loading object " + element.Name.LocalName + "...");
                                ReadObject(element);
                            }
                        }
                        else
                        {
                            foreach (IBaseObject target in Targets)
                            {
                                if (elementIndex == elementCount)
                                    throw new ObjectException("Element count doesn't match targets.");

                                XElement element = elements[elementIndex++];
                                UpdateProgressElapsed("Loading target " + element.Name.LocalName + "...");
                                ReadTarget(target, element);

                                // Load children until another element of the same type is encountered.
                                while (elementIndex < elementCount)
                                {
                                    element = elements[elementIndex];

                                    if (target.GetType().Name == element.Name.LocalName)
                                        break;

                                    UpdateProgressElapsed("Loading object " + element.Name.LocalName + "...");
                                    ReadObject(element);

                                    elementIndex++;
                                }
                            }
                        }
                    }
                    else
                    {
                        while (elementIndex < elementCount)
                        {
                            XElement element = elements[elementIndex++];
                            UpdateProgressElapsed("Loading object " + element.Name.LocalName +
                                " (" + elementIndex.ToString() + ") ...");
                            ReadObject(element);
                        }
                    }
                }

                SaveCachedObjects();
            }
            catch (Exception exception)
            {
                Error = "Exception: " + exception.Message;

                if (exception.InnerException != null)
                    Error += " (" + exception.InnerException.Message + ")";
            }

            UpdateProgressElapsed("Doing fixups...");

            EndFixups();

            EndContinuedProgress();

            if (Timer != null)
            {
                //Timer.Stop();
                OperationTime = Timer.GetTimeInSeconds();
            }

            if (!String.IsNullOrEmpty(Error))
                throw new ObjectException(Error);
        }

        // Read a repository object that is a target.
        public void ReadTarget(IBaseObject target, XElement element)
        {
            if (target.GetType().Name != element.Name.LocalName)
                throw new ObjectException("Element \"" + element.Name.LocalName + "\"  doesn't match target type \"" + target.GetType().Name + "\".");

            IBaseObjectKeyed keyedObject = target as IBaseObjectKeyed;

            if (keyedObject != null)
            {
                object key = keyedObject.Key;
                string directory = null;
                MultiLanguageString title = null;
                MultiLanguageString description = null;
                bool doUpdate = true;

                if (keyedObject is BaseObjectTitled)
                {
                    BaseObjectTitled titledObject = keyedObject as BaseObjectTitled;
                    directory = titledObject.Directory;
                    title = titledObject.Title;
                    description = titledObject.Description;
                    titledObject.IsPublic = MakeTargetPublic;
                }

                if (keyedObject is BaseObjectNode)
                {
                    BaseObjectNode node = keyedObject as BaseObjectNode;
                    object parentKey = node.ParentKey;
                    BaseObjectNodeTree tree = node.Tree;
                    int index = node.Index;
                    node.Xml = element;
                    node.Index = index;
                    node.Tree = tree;
                    node.ParentKey = parentKey;
                }
                else if (keyedObject is BaseObjectContent)
                {
                    BaseObjectContent content = keyedObject as BaseObjectContent;
                    string parentKey = content.ContentParentKey;
                    BaseObjectNode node = content.Node;
                    int index = content.Index;
                    content.Xml = element;
                    content.Index = index;
                    content.Key = key;
                    if (Fixups != null)
                        content.OnFixup(Fixups);
                    content.Node = node;
                    content.ContentParentKey = parentKey;
                    content.Modified = false;
                    doUpdate = false;
                }
                else if (keyedObject is BaseObjectTitled)
                {
                    BaseObjectTitled titledObject = keyedObject as BaseObjectTitled;
                    int index = titledObject.Index;
                    titledObject.Xml = element;
                    titledObject.Index = index;
                }
                else
                    keyedObject.Xml = element;

                if (PreserveTargetNames && (keyedObject is BaseObjectTitled))
                {
                    BaseObjectTitled titledObject = keyedObject as BaseObjectTitled;
                    titledObject.Title = title;
                    titledObject.Description = description;
                    titledObject.Directory = directory;
                }

                if (doUpdate)
                {
                    AddFixupCheck(keyedObject);
                    keyedObject.Key = key;

                    if (keyedObject.CreationTime == DateTime.MinValue)
                        keyedObject.TouchAndClearModified();
                    else
                        keyedObject.Modified = false;

                    if (!String.IsNullOrEmpty(keyedObject.Source)
                        && (keyedObject.Source != "Nodes"))  // Older exporter incorrectly set node Source to "Nodes".
                    {
                        if (!Repositories.UpdateReference(keyedObject.Source, null, keyedObject))
                            throw new ObjectException("Error updating target \"" + element.Name.LocalName + "\" name \"" + keyedObject.Name + "\".");

                        if (keyedObject is BaseObjectNodeTree)
                            NodeUtilities.UpdateTreeReference(keyedObject as BaseObjectNodeTree, false);
                    }
                }
            }
            else
                target.Xml = element;
        }

        // Read a repository object that is not a target.
        public void ReadObject(XElement element)
        {
            IBaseObject obj = ObjectUtilities.ResurrectBaseObject(element);

            if (obj == null)
                throw new ObjectException("Unknown element type: " + element.Name.LocalName);

            SaveObject(obj);
        }

        public override void Write(Stream stream)
        {
            ItemCount = 0;
            ProgressCount = ProgressIndex = 0;

            RootXml = new XElement("JTLanguage");
            RootXml.Add(new XAttribute("Version", Version));

            if (Targets != null)
            {
                List<IBaseObjectKeyed> references = new List<IBaseObjectKeyed>();
                List<IBaseObjectKeyed> externalSavedChildren = new List<IBaseObjectKeyed>();
                List<IBaseObjectKeyed> externalNonSavedChildren = new List<IBaseObjectKeyed>();

                foreach (IBaseObject target in Targets)
                {
                    if (target is BaseObjectTitled)
                        (target as BaseObjectTitled).ResolveReferences(Repositories, false, true);

                    CollectReferences(target, references, externalSavedChildren, externalNonSavedChildren,
                        NodeKeyFlags, ContentKeyFlags, null, LanguageFlags);
                }

                SaveReferences(references);
                SaveExternalSavedChildren(externalSavedChildren);
                SaveExternalNonSavedChildren(externalNonSavedChildren);

                foreach (IBaseObject target in Targets)
                    WriteTarget(target);

                using (StreamWriter writer = new StreamWriter(stream, new System.Text.UTF8Encoding(true)))
                {
                    RootXml.Save(writer);
                    writer.Flush();
                }
            }
        }

        protected virtual void WriteTarget(IBaseObject target)
        {
            XElement targetElement;

            if ((NodeKeyFlags != null) || (ContentKeyFlags != null))
            {
                if (target is BaseContentContainer)
                {
                    BaseContentContainer contentContainer = (BaseContentContainer)target;
                    targetElement = contentContainer.GetElementFiltered(target.GetType().Name, NodeKeyFlags, ContentKeyFlags);
                    RootXml.Add(targetElement);
                    return;
                }
            }

            targetElement = target.Xml;
            RootXml.Add(targetElement);
        }

        protected void LoadReferences(List<XElement> referenceElements)
        {
            foreach (XElement element in referenceElements)
                LoadReference(element);
        }

        protected void LoadReference(XElement referenceElement)
        {
            IBaseObject obj = ObjectUtilities.ResurrectBaseObject(referenceElement);

            if (obj == null)
                throw new ObjectException("Unknown element type: " + referenceElement.Name.LocalName);

            IBaseObjectKeyed keyedObject = obj as IBaseObjectKeyed;

            if (keyedObject == null)
                throw new ObjectException("Objects which are not keyed cannot be references: " + referenceElement.Name.LocalName);

            SaveReferenceObject(keyedObject);
        }

        protected void SaveReferences(List<IBaseObjectKeyed> references)
        {
            if (references.Count() != 0)
            {
                XElement referencesElement = new XElement("References");

                foreach (IBaseObjectKeyed item in references)
                {
                    XElement referenceElement = item.Xml;
                    referencesElement.Add(referenceElement);
                }

                RootXml.Add(referencesElement);
            }
        }

        protected void LoadExternalSavedChildren(List<XElement> childElements)
        {
            foreach (XElement element in childElements)
                LoadExternalSavedChild(element);
        }

        protected void LoadExternalSavedChild(XElement childElement)
        {
            IBaseObject obj = ObjectUtilities.ResurrectBaseObject(childElement);

            if (obj == null)
                throw new ObjectException("Unknown element type: " + childElement.Name.LocalName);

            IBaseObjectKeyed keyedObject = obj as IBaseObjectKeyed;

            if (keyedObject == null)
                throw new ObjectException("Objects which are not keyed cannot be an external child: " + childElement.Name.LocalName);

            if (String.IsNullOrEmpty(keyedObject.Source) || (keyedObject.Source == "Nodes"))
                throw new ObjectException("Objects which are not stored cannot be an external child: " + childElement.Name.LocalName);

            SaveObject(keyedObject);
        }

        protected void LoadExternalNonSavedChildren(List<XElement> childElements)
        {
            foreach (XElement element in childElements)
                LoadExternalNonSavedChild(element);
        }

        protected void LoadExternalNonSavedChild(XElement childElement)
        {
            IBaseObject obj = ObjectUtilities.ResurrectBaseObject(childElement);

            if (obj == null)
                throw new ObjectException("Unknown element type: " + childElement.Name.LocalName);

            IBaseObjectKeyed keyedObject = obj as IBaseObjectKeyed;

            if (keyedObject == null)
                throw new ObjectException("Objects which are not keyed cannot be an external child: " + childElement.Name.LocalName);

            SaveNonSavedObject(keyedObject);
        }

        protected void SaveExternalSavedChildren(List<IBaseObjectKeyed> children)
        {
            if (children.Count() != 0)
            {
                XElement childrenElement = new XElement("ExternalSavedChildren");

                foreach (IBaseObjectKeyed item in children)
                {
                    XElement childElement;

                    if (item is BaseContentStorage)
                    {
                        BaseContentStorage contentStorage = (BaseContentStorage)item;
                        BaseObjectContent content = contentStorage.Content;
                        BaseObjectNode node = content.Node;

                        if (ContentKeyFlags != null)
                        {
                            bool useIt = true;

                            if (!ContentKeyFlags.TryGetValue(content.KeyString, out useIt))
                                useIt = true;

                            /*
                            while (((content = content.ContentParent) != null) && useIt)
                                ContentKeyFlags.TryGetValue(content.KeyString, out useIt);
                            */

                            if (!useIt)
                                continue;
                        }

                        if (NodeKeyFlags != null)
                        {
                            bool useIt = true;

                            if (NodeKeyFlags.TryGetValue(node.KeyInt, out useIt) && !useIt)
                                continue;
                        }

                        if (ItemKeyFlags != null)
                        {
                            childElement = contentStorage.GetElementFiltered(contentStorage.GetType().Name, ItemKeyFlags);
                            childrenElement.Add(childElement);
                            continue;
                        }
                    }

                    childElement = item.Xml;
                    childrenElement.Add(childElement);
                }

                RootXml.Add(childrenElement);
            }
        }

        protected void SaveExternalNonSavedChildren(List<IBaseObjectKeyed> children)
        {
            if (children.Count() != 0)
            {
                XElement childrenElement = new XElement("ExternalNonSavedChildren");

                foreach (IBaseObjectKeyed item in children)
                {
                    XElement childElement;

                    if (item is BaseObjectNode)
                    {
                        BaseObjectNode node = (BaseObjectNode)item;

                        if (NodeKeyFlags != null)
                        {
                            bool useIt = true;

                            if (NodeKeyFlags.TryGetValue(node.KeyInt, out useIt) && !useIt)
                                continue;
                        }

                        if ((NodeKeyFlags != null) || (ContentKeyFlags != null))
                        {
                            childElement = node.GetElementFiltered(node.GetType().Name, NodeKeyFlags, ContentKeyFlags);
                            childrenElement.Add(childElement);
                            continue;
                        }
                    }

                    childElement = item.Xml;
                    childrenElement.Add(childElement);
                }

                RootXml.Add(childrenElement);
            }
        }

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            PreserveTargetNames = GetFlagArgumentDefaulted("PreserveTargetNames", "flag", "r",
                PreserveTargetNames, "Preserve target names", PreserveTargetNamesHelp, null, null);
            MakeTargetPublic = GetFlagArgumentDefaulted("MakeTargetPublic", "flag", "r",
                MakeTargetPublic, "Make public", MakeTargetPublicHelp, null, null);

            if (!PreserveGuid)
                PreserveGuid = PreserveTargetNames;
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetFlagArgument("PreserveTargetNames", "flag", "r", PreserveTargetNames, "Preserve target names",
                PreserveTargetNamesHelp, null, null);
            SetFlagArgument("MakeTargetPublic", "flag", "r", MakeTargetPublic, "Make targets public",
                MakeTargetPublicHelp, null, null);
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        // Check for supported capability.
        // importExport:  "Import" or "Export"
        // componentName: class name or GetComponentName value
        // capability: "Supported" for general support, "UseFlags" for component item select support, "ComponentFlags" for lesson component select support,
        //  "NodeFlags" for sub-object select support.
        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            switch (componentName)
            {
                case "BaseObjectNodeTree":
                case "BaseObjectNode":
                case "BaseObjectContent":
                case "ContentStudyList":
                case "ContentMediaList":
                case "ContentMediaItem":
                case "BaseMarkupContainer":
                case "NodeMaster":
                case "MarkupTemplate":
                case "MultiLanguageItem":
                case "MultiLanguageString":
                case "DictionaryEntry":
                case "AudioMultiReference":
                case "AudioInstance":
                    if (capability == "Support")
                        return true;
                    return false;
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
