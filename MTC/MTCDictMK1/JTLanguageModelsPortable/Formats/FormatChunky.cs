using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Formats
{
    // Chunky format class.
    //
    // file format:
    //
    //      Field       Size            Content
    //      Header ID   2               'CH'
    //      Version     variable 1-5    packed int
    //      Chunks...   variable        chunk
    //
    // Chunk format:
    //
    //      Field       Size            Content
    //      Chunk Type  1               'O' for object, 'F' for file, 'S' for section header
    //      Chunk Size  variable 1-5    Size of chunk data
    //      Chunk Data  from Chunk Size Chunk type dependent
    //
    // Object chunk data format:
    //
    //      Field       Size            Content
    //      Data        variable        Xml encoded as UTF-8
    //
    // File chunk data format:
    //
    //      Field       Size            Content
    //      SLength     variable 1-5    Partial file path string size
    //      Path        from SLength    Partial file path string relative to object media directory
    //      Size        variable 1-5    Size of file
    //      File Data   from Size       File bytes
    //
    // Section header chunk data format:
    //
    //      Field       Size            Content
    //      Sub Type    1               'R' for references
    //                                  'X' for saved external references
    //                                  'N' for non-saved
    //                                  'P' for main payload chunks
    //                                  'M' for media file chunks
    //      Count       variable 1-5    Number of object chunks following this chunk.

    public class FormatChunky : Format
    {
        protected int Version = 1;
        private static string FormatDescription = "Native JTLanguage binary format based on typed and sized binary chunks.";
        public string UserMediaFilePath;
        public bool IncludeMedia { get; set; }
        public bool OverwriteExistingMedia { get; set; }
        private List<string> MediaFiles;
        private IArchiveFile Archive;
        private string MediaFilePath;
        private string globalMediaDir;
        private string globalUserMediaDir;
        private string globalUserMediaDirLower;
        private string externalUserPathNode;
        private string externalUserPath;
        private string externalUserPathLower;
        private string mediaDirSlashLower;
        private List<string> directoriesToDelete;
        private const byte HeaderByte0 = (byte)'C';
        private const byte HeaderByte1 = (byte)'H';
        private const byte ObjectChunkCode = (byte)'O';
        private const byte FileChunkCode = (byte)'F';
        private const byte SectionHeaderChunkCode = (byte)'S';
        private const byte ReferencesSubCode = (byte)'R';
        private const byte SavedExternalReferencesSubCode = (byte)'X';
        private const byte NonSavedExternalReferencesSubCode = (byte)'N';
        private const byte PayloadSubCode = (byte)'P';
        private const byte MediaSubCode = (byte)'M';

        public FormatChunky()
            : base("JTLanguage Chunky", "FormatChunky", FormatDescription, String.Empty, String.Empty,
                  "application/octet-stream", ".jtc", null, null, null, null, null)
        {
            UserMediaFilePath = null;
            IncludeMedia = true;
            OverwriteExistingMedia = false;
        }

        public FormatChunky(FormatChunky other)
            : base(other)
        {
            UserMediaFilePath = other.UserMediaFilePath;
            IncludeMedia = other.IncludeMedia;
            OverwriteExistingMedia = other.OverwriteExistingMedia;
        }

        // For derived classes.
        public FormatChunky(string name, string type, string description, string targetType, string importExportType,
                string mimeType, string defaultExtension,
                UserRecord userRecord, UserProfile userProfile, IMainRepository repositories, LanguageUtilities languageUtilities,
                NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension, userRecord, userProfile, repositories, languageUtilities,
                  nodeUtilities)
        {
            UserMediaFilePath = null;
            IncludeMedia = true;
            OverwriteExistingMedia = false;
        }

        public override Format Clone()
        {
            return new FormatChunky(this);
        }

        public override void Read(Stream stream)
        {
            try
            {
                int b1 = stream.ReadByte();
                int b2 = stream.ReadByte();

                if ((b1 == HeaderByte0) && (b2 == HeaderByte1))
                {
                    Initialize();
                    ItemCount = 0;
                    ResetProgress();

                    //if (Timer != null)
                    //    Timer.Start();

                    DeleteMediaFiles = GetUserOptionFlag("DeleteMediaFiles", true);

                    if (DeleteBeforeImport)
                        DeleteFirst();

                    StartFixups();

                    int version;

                    if (!ReadPackedInteger(stream, out version))
                        throw new ObjectException("No \"Version\" field.");

                    if (version != Version)
                        throw new ObjectException("Format is too new for this version of JTLanguage.");

                    int sectionCount = 5;  // references, non-saved externals, saved externals, payload, media
                    int sectionIndex = 0;
                    int targetCount = ((Targets != null) ? Targets.Count() : 0);
                    int progressCount = sectionCount + (targetCount == 0 ? 1 : targetCount) + 3;

                    ContinueProgress(sectionIndex + ProgressCountBase + progressCount);

                    if (targetCount == 1)
                    {
                        if (Targets[0] is BaseObjectNode)
                        {
                            BaseObjectNode node = Targets[0] as BaseObjectNode;

                            if (!node.IsTree())
                                Tree = node.Tree;
                        }
                    }

                    UpdateProgressElapsed("Loading references...");
                    LoadReferences(stream);

                    UpdateProgressElapsed("Loading subordinate saved items...");
                    LoadExternalSavedChildren(stream);

                    UpdateProgressElapsed("Loading subordinate non-saved items...");
                    LoadExternalNonSavedChildren(stream);

                    UpdateProgressElapsed("Loading target objects...");
                    LoadTargets(stream);

                    UpdateProgressElapsed("Doing fixups...");

                    EndFixups();

                    UpdateProgressElapsed("Cleaning up...");

                    if (directoriesToDelete.Count() != 0)
                    {
                        foreach (string deleteDir in directoriesToDelete)
                        {
                            try
                            {
                                FileSingleton.DeleteDirectory(deleteDir);
                            }
                            catch (Exception exc)
                            {
                                string msg = "Exception during deleting external directories: " + exc.Message;
                                if (exc.InnerException != null)
                                    msg += exc.InnerException.Message;
                            }
                        }
                    }

                    string externalPath = MediaFilePath + externalUserPathNode;

                    if (FileSingleton.DirectoryExists(externalPath))
                    {
                        try
                        {
                            FileSingleton.DeleteDirectory(externalPath);
                        }
                        catch (Exception exc)
                        {
                            string msg = "Exception during deleting external directory: " + exc.Message;
                            if (exc.InnerException != null)
                                msg += exc.InnerException.Message;
                        }
                    }

                    EndContinuedProgress();

                    if (Timer != null)
                    {
                        //Timer.Stop();
                        OperationTime = Timer.GetTimeInSeconds();
                    }
                }
                else
                    Error = "Read not supported for this file type.";
            }
            catch (Exception exception)
            {
                Error = "Exception: " + exception.Message;

                if (exception.InnerException != null)
                    Error += " (" + exception.InnerException.Message + ")";
            }

            Reset();

            if (!String.IsNullOrEmpty(Error))
                throw new ObjectException(Error);
        }

        public void LoadTargets(Stream stream)
        {
            int sectionType;
            int chunkCount;
            int targetCount = ((Targets != null) ? Targets.Count() : 0);
            XElement element;

            if (!ReadSectionChunk(stream, out sectionType, out chunkCount))
                throw new Exception("Error reading target objects section header.");

            if (targetCount > chunkCount)
                throw new Exception("Target count is greater than target count in file.");

            if (Targets != null)
            {
                if (targetCount == 1)
                {
                    if (!ReadObjectChunk(stream, out element))
                        throw new Exception("Error reading target object.");

                    IBaseObject target = Targets.First();

                    UpdateProgressMessageElapsed("Loading target " + element.Name.LocalName + "...");
                    ReadTarget(stream, target, element);

                    for (int chunkIndex = 1; chunkIndex < chunkCount; chunkIndex++)
                    {
                        if (!ReadObjectChunk(stream, out element))
                            throw new Exception("Error reading target object.");

                        UpdateProgressMessageElapsed("Loading object " + element.Name.LocalName + "...");

                        ReadObject(stream, element);
                    }
                }
                else
                {
                    foreach (IBaseObject target in Targets)
                    {
                        if (targetCount != chunkCount)
                            throw new Exception("Target count different from target count in file.");

                        if (!ReadObjectChunk(stream, out element))
                            throw new Exception("Error reading target object.");

                        UpdateProgressMessageElapsed("Loading target " + element.Name.LocalName + "...");

                        ReadTarget(stream, target, element);
                    }
                }
            }
            else
            {
                for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                {
                    if (!ReadObjectChunk(stream, out element))
                        throw new Exception("Error reading payload object.");

                    UpdateProgressMessageElapsed("Loading payload object " + element.Name.LocalName + "...");

                    ReadObject(stream, element);
                }
            }

            SaveCachedObjects();
        }

        // Read a repository object that is a target.
        public void ReadTarget(Stream stream, IBaseObject target, XElement element)
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

            LoadMedia(stream, target);
        }

        // Read a repository object that is not a target.
        public void ReadObject(Stream stream, XElement element)
        {
            IBaseObject obj = ObjectUtilities.ResurrectBaseObject(element);

            if (obj == null)
                throw new ObjectException("Unknown element type: " + element.Name.LocalName);

            SaveObject(obj);

            LoadMedia(stream, obj);
        }

        public override void Write(Stream stream)
        {
            ItemCount = 0;
            ProgressCount = ProgressIndex = 0;

            try
            {
                Initialize();

                stream.WriteByte((byte)HeaderByte0);
                stream.WriteByte((byte)HeaderByte1);

                WritePackedInteger(stream, Version);

                List<IBaseObjectKeyed> references = new List<IBaseObjectKeyed>();
                List<IBaseObjectKeyed> externalSavedChildren = new List<IBaseObjectKeyed>();
                List<IBaseObjectKeyed> externalNonSavedChildren = new List<IBaseObjectKeyed>();

                if (Targets != null)
                {
                    foreach (IBaseObject target in Targets)
                    {
                        if (target is BaseObjectTitled)
                            (target as BaseObjectTitled).ResolveReferences(Repositories, false, true);

                        CollectReferences(target, references, externalSavedChildren, externalNonSavedChildren,
                            NodeKeyFlags, ContentKeyFlags, null, LanguageFlags);
                    }
                }

                SaveReferences(stream, references);
                SaveExternalSavedChildren(stream, externalSavedChildren);
                SaveExternalNonSavedChildren(stream, externalNonSavedChildren);
                SaveTargets(stream, Targets);

                stream.Flush();
            }
            catch (Exception exception)
            {
                Error = "Exception: " + exception.Message;

                if (exception.InnerException != null)
                    Error += " (" + exception.InnerException.Message + ")";
            }

            Reset();

            if (!String.IsNullOrEmpty(Error))
                throw new ObjectException(Error);
        }

        protected void SaveTargets(Stream stream, List<IBaseObject> targets)
        {
            int itemCount = (targets != null ? targets.Count() : 0);

            if (!WriteSectionChunk(stream, PayloadSubCode, itemCount))
                throw new Exception("Error writing payload section header chunk.");

            if (targets != null)
            {
                foreach (IBaseObject target in targets)
                    WriteTarget(stream, target);
            }
        }

        protected virtual void WriteTarget(Stream stream, IBaseObject target)
        {
            XElement targetElement;

            if ((NodeKeyFlags != null) || (ContentKeyFlags != null))
            {
                if (target is BaseContentContainer)
                {
                    BaseContentContainer contentContainer = (BaseContentContainer)target;

                    targetElement = contentContainer.GetElementFiltered(target.GetType().Name, NodeKeyFlags, ContentKeyFlags);

                    if (!WriteObjectChunk(stream, targetElement))
                        throw new Exception("Error writing payload object chunk.");

                    SaveMedia(stream, target);

                    return;
                }
            }

            targetElement = target.Xml;

            if (!WriteObjectChunk(stream, targetElement))
                throw new Exception("Error writing payload object chunk.");

            SaveMedia(stream, target);
        }

        protected void LoadReferences(Stream stream)
        {
            int sectionType = 0;
            int chunkCount = 0;

            if (!ReadSectionChunk(stream, out sectionType, out chunkCount))
                throw new Exception("Error reading references section header.");

            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                XElement element;

                if (!ReadObjectChunk(stream, out element))
                    throw new Exception("Error reading reference object.");

                LoadReference(element);
            }
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

        protected void SaveReferences(Stream stream, List<IBaseObjectKeyed> references)
        {
            int itemCount = references.Count();

            if (!WriteSectionChunk(stream, ReferencesSubCode, itemCount))
                throw new Exception("Error writing references section header chunk.");

            if (itemCount != 0)
            {
                foreach (IBaseObjectKeyed item in references)
                {
                    XElement element = item.Xml;

                    if (!WriteObjectChunk(stream, element))
                        throw new Exception("Error writing reference object chunk.");
                }
            }
        }

        protected void LoadExternalSavedChildren(Stream stream)
        {
            int sectionType = 0;
            int chunkCount = 0;

            if (!ReadSectionChunk(stream, out sectionType, out chunkCount))
                throw new Exception("Error reading external non-saved object section header.");

            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                XElement element;

                if (!ReadObjectChunk(stream, out element))
                    throw new Exception("Error reading external non-saved object.");

                LoadExternalSavedChild(element);
            }
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

        protected void LoadExternalNonSavedChildren(Stream stream)
        {
            int sectionType = 0;
            int chunkCount = 0;

            if (!ReadSectionChunk(stream, out sectionType, out chunkCount))
                throw new Exception("Error reading external non-saved object section header.");

            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                XElement element;

                if (!ReadObjectChunk(stream, out element))
                    throw new Exception("Error reading external non-saved object.");

                LoadExternalNonSavedChild(element);
            }
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

        protected void SaveExternalSavedChildren(Stream stream, List<IBaseObjectKeyed> children)
        {
            List<IBaseObjectKeyed> filteredList = new List<IBaseObjectKeyed>();

            foreach (IBaseObjectKeyed item in children)
            {
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

                        if (!useIt)
                            continue;
                    }

                    if (NodeKeyFlags != null)
                    {
                        bool useIt = true;

                        if (NodeKeyFlags.TryGetValue(node.KeyInt, out useIt) && !useIt)
                            continue;
                    }

                    filteredList.Add(contentStorage);
                }
                else
                    filteredList.Add(item);
            }

            int itemCount = filteredList.Count();

            if (!WriteSectionChunk(stream, SavedExternalReferencesSubCode, itemCount))
                throw new Exception("Error writing external saved references section header chunk.");

            foreach (IBaseObjectKeyed item in filteredList)
            {
                XElement element = null;

                if (item is BaseContentStorage)
                {
                    BaseContentStorage contentStorage = (BaseContentStorage)item;

                    if (ItemKeyFlags != null)
                        element = contentStorage.GetElementFiltered(contentStorage.GetType().Name, ItemKeyFlags);
                    else
                        element = contentStorage.Xml;
                }
                else
                    element = item.Xml;

                if (!WriteObjectChunk(stream, element))
                    throw new Exception("Error writing external saved references object chunk.");
            }
        }

        protected void SaveExternalNonSavedChildren(Stream stream, List<IBaseObjectKeyed> children)
        {
            List<IBaseObjectKeyed> filteredList = new List<IBaseObjectKeyed>();

            foreach (IBaseObjectKeyed item in children)
            {
                if (item is BaseObjectNode)
                {
                    BaseObjectNode node = (BaseObjectNode)item;

                    if (NodeKeyFlags != null)
                    {
                        bool useIt = true;

                        if (NodeKeyFlags.TryGetValue(node.KeyInt, out useIt) && !useIt)
                            continue;
                    }

                    filteredList.Add(node);
                }
                else
                    filteredList.Add(item);
            }

            int itemCount = filteredList.Count();

            if (!WriteSectionChunk(stream, NonSavedExternalReferencesSubCode, itemCount))
                throw new Exception("Error writing external non-saved references section header chunk.");

            foreach (IBaseObjectKeyed item in filteredList)
            {
                XElement element = null;

                if (item is BaseObjectNode)
                {
                    BaseObjectNode node = (BaseObjectNode)item;

                    if ((NodeKeyFlags != null) || (ContentKeyFlags != null))
                        element = node.GetElementFiltered(node.GetType().Name, NodeKeyFlags, ContentKeyFlags);
                    else
                        element = node.Xml;
                }
                else
                    element = item.Xml;

                if (!WriteObjectChunk(stream, element))
                    throw new Exception("Error writing external non-saved references object chunk.");
            }
        }

        public void LoadMedia(Stream stream, IBaseObject target)
        {
            int sectionType = 0;
            int chunkCount = 0;
            string filePath;
            string mediaDir;

            if (target is BaseObjectTitled)
                mediaDir = (target as BaseObjectTitled).MediaDirectoryPath;
            else
                mediaDir = globalUserMediaDir;

            if (!ReadSectionChunk(stream, out sectionType, out chunkCount))
                throw new Exception("Error reading media section header.");

            if (IncludeMedia)
            {
                for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                {
                    if (!ReadFileChunk(stream, mediaDir, out filePath))
                        throw new Exception("Error reading media file chunk.");

                    if (!String.IsNullOrEmpty(filePath))
                        MediaFiles.Add(filePath);
                }
            }
            else
            {
                for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                {
                    if (!ReadSkipChunk(stream, FileChunkCode))
                        throw new Exception("Error skipping media file chunk.");
                }
            }
        }

        public void SaveMedia(Stream stream, IBaseObject target)
        {
            int itemCount = 0;
            string mediaDir;
            List<string> mediaFiles = new List<string>();

            GetMediaFiles(target, mediaFiles);

            itemCount = mediaFiles.Count();

            if (target is BaseObjectTitled)
                mediaDir = (target as BaseObjectTitled).MediaDirectoryPath;
            else
                mediaDir = globalUserMediaDir;

            if (!mediaDir.EndsWith(ApplicationData.PlatformPathSeparator))
                mediaDir += ApplicationData.PlatformPathSeparator;

            if (!WriteSectionChunk(stream, MediaSubCode, itemCount))
                throw new Exception("Error writing media section header chunk.");

            foreach (string mediaFile in mediaFiles)
            {
                if (!WriteFileChunk(stream, mediaDir, mediaFile))
                    throw new Exception("Error writing media file chunk.");
            }

            MediaFiles.AddRange(mediaFiles);
        }

        public void GetMediaFiles(IBaseObject target, List<string> mediaFiles)
        {
            if (target != null)
            {
                // Collect the media files.
                if (IncludeMedia)
                    CollectMediaFiles(target, mediaFiles);
                else
                    CollectNodeImageFiles(target, mediaFiles);
            }
        }

        protected override void CollectMediaFiles(IBaseObject obj, List<string> mediaFiles)
        {
            obj.CollectReferences(null, null, null, NodeKeyFlags, ContentKeyFlags, ItemKeyFlags,
                LanguageFlags, mediaFiles, MediaUtilities.VisitMediaDontCollectAlternatesToo);
        }

        public static bool VisitNodeCollectNodeImage(BaseObjectNode node, ItemWalker<List<string>> walker, List<string> mediaFiles)
        {
            if (node.ImageFileIsExternal)
                return true;

            string imageFile = node.ImageFileName;

            if (imageFile == null)
                return true;

            if (imageFile.StartsWith("../"))
                return true;

            string imageFilePath = node.ImageFilePath;

            if (!mediaFiles.Contains(imageFilePath))
                mediaFiles.Add(imageFilePath);

            return true;
        }

        protected override void CollectNodeImageFiles(IBaseObject obj, List<string> mediaFiles)
        {
            if (obj is BaseObjectNodeTree)
            {
                ItemWalker<List<string>> itemWalker = new ItemWalker<List<string>>();
                itemWalker.VisitTreeFunction = VisitNodeCollectNodeImage;
                itemWalker.VisitNodeFunction = VisitNodeCollectNodeImage;
                itemWalker.WalkTree(obj as BaseObjectNodeTree, mediaFiles);
            }
            else if (obj is BaseObjectNode)
            {
                ItemWalker<List<string>> itemWalker = new ItemWalker<List<string>>();
                itemWalker.VisitTreeFunction = VisitNodeCollectNodeImage;
                itemWalker.VisitNodeFunction = VisitNodeCollectNodeImage;
                itemWalker.WalkNode(obj as BaseObjectNode, mediaFiles);
            }
        }

        protected override void LazyConvertMediaFiles(List<string> mediaFiles)
        {
            foreach (string mediaFile in mediaFiles)
            {
                string mimeType = MediaUtilities.GetMimeTypeFromFileName(mediaFile);

                try
                {
                    /* Already checked for existance.
                    if (!FileSingleton.Exists(mediaFile))
                    {
                        continue;
                    }
                    */

                    string message;
                    MediaConvertSingleton.LazyConvert(mediaFile, mimeType, false, out message);
                }
                catch (Exception)
                {
                }
            }
        }

        public bool ReadPackedInteger(Stream stream, out int value)
        {
            int b;
            int shift = 0;

            value = 0;

            do
            {
                if (shift > 32)
                    return false;

                b = stream.ReadByte();

                if (b == -1)
                    return false;

                value += ((b & 0x7f) << shift);
                shift += 7;
            }
            while ((b & 0x80) == 0);

            return true;
        }

        public bool WritePackedInteger(Stream stream, int value)
        {
            uint data = (uint)value;
            byte b;

            do
            {
                b = (byte)(data & 0x7f);
                data >>= 7;

                if (data == 0)
                    b |= 0x80;

                stream.WriteByte(b);
            }
            while (data != 0);

            return true;
        }

        public int GetPackedIntegerSize(int value)
        {
            uint data = (uint)value;
            int size = 0;

            do
            {
                data >>= 7;
                size++;
            }
            while (data != 0);

            return size;
        }

        public bool ReadString(Stream stream, out string value)
        {
            int length = 0;

            value = null;

            if (!ReadPackedInteger(stream, out length))
                return false;

            byte[] data = new byte[length];

            if (!ReadData(stream, length, data))
                return false;

            value = ApplicationData.Encoding.GetString(data, 0, length);

            return true;
        }

        public bool WriteString(Stream stream, string value)
        {
            byte[] data = ApplicationData.Encoding.GetBytes(value);
            int length = data.Length;

            if (!WritePackedInteger(stream, length))
                return false;

            stream.Write(data, 0, length);

            return true;
        }

        public int GetStringFieldLength(string value)
        {
            byte[] data = ApplicationData.Encoding.GetBytes(value);
            int length = data.Length;

            length += GetPackedIntegerSize(length);

            return length;
        }

        public bool ReadObjectChunk(Stream stream, out XElement element)
        {
            int chunkType = stream.ReadByte();
            int chunkSize;
            byte isCompressed;
            int dataSize;
            byte[] decompressedData;

            element = null;

            if (chunkType != ObjectChunkCode)
                return false;

            if (!ReadPackedInteger(stream, out chunkSize))
                return false;

            isCompressed = (byte)stream.ReadByte();

            dataSize = chunkSize - 1;   // Subtract isCompressed size.

            byte[] data = new byte[dataSize];

            if (!ReadData(stream, dataSize, data))
                return false;

            if (isCompressed == 1)
            {
                if (!Archive.Decompress(data, out decompressedData))
                    return false;

                data = decompressedData;
                dataSize = data.Length;
            }

            string xmlString = ApplicationData.Encoding.GetString(data, 0, dataSize);
            element = XElement.Parse(xmlString, LoadOptions.PreserveWhitespace);

            return true;
        }

        public bool WriteObjectChunk(Stream stream, XElement element)
        {
            string xmlString = element.ToString();
            byte[] data = ApplicationData.Encoding.GetBytes(xmlString);
            int chunkSize;
            int size = data.Length;
            byte[] compressedData;
            int compressedSize;
            byte isCompressed = 0;

            if (Archive.Compress(data, out compressedData))
            {
                compressedSize = compressedData.Length;

                if (compressedSize < size)
                {
                    data = compressedData;
                    size = compressedSize;
                    chunkSize = compressedSize;
                    isCompressed = 1;
                }
                else
                    chunkSize = size;
            }
            else
                chunkSize = size;

            chunkSize += 1;     // Add isCompressedFlag;

            stream.WriteByte(ObjectChunkCode);

            if (!WritePackedInteger(stream, chunkSize))
                return false;

            stream.WriteByte(isCompressed);

            stream.Write(data, 0, size);

            return true;
        }

        public bool ReadFileChunk(Stream stream, string mediaDir, out string filePath)
        {
            int chunkType = stream.ReadByte();
            int chunkSize;
            string logicalFilePath;
            int fileLength;
            bool returnValue = true;

            filePath = null;

            if (chunkType != FileChunkCode)
                return false;

            if (!ReadPackedInteger(stream, out chunkSize))
                return false;

            if (!ReadString(stream, out logicalFilePath))
                return false;

            if (!ReadPackedInteger(stream, out fileLength))
                return false;

            if (ApplicationData.PlatformPathSeparator != @"\")
                logicalFilePath = logicalFilePath.Replace(@"\", ApplicationData.PlatformPathSeparator);

            int logicalFilePathsize = GetStringFieldLength(logicalFilePath);
            int referenceChunkSize = logicalFilePathsize + fileLength + GetPackedIntegerSize(fileLength);

            if (referenceChunkSize != chunkSize)
                throw new Exception("Bad chunk size in file chunk.");

            if (fileLength == 0)
            {
                filePath = null;
                return true;
            }

            if (logicalFilePath.StartsWith(externalUserPathNode))
            {
                // Remap external files to the correct place.
                string newFilePath = logicalFilePath.Substring(externalUserPathNode.Length);
                filePath = MediaUtilities.ConcatenateFilePath(globalUserMediaDir, newFilePath);
            }
            else
                filePath = MediaUtilities.ConcatenateFilePath(mediaDir, logicalFilePath);

            FileSingleton.DirectoryExistsCheck(filePath);

            if (!OverwriteExistingMedia && FileSingleton.Exists(filePath))
            {
                try
                {
                    if (stream.CanSeek)
                        stream.Seek(fileLength, SeekOrigin.Current);
                    else
                    {
                        int readSize = fileLength;
                        const int bufferSize = 0x1000;
                        byte[] buffer = new byte[bufferSize];
                        int read;
                        int blockSize;

                        while (readSize > 0)
                        {
                            blockSize = (readSize < bufferSize ? readSize : bufferSize);

                            read = stream.Read(buffer, 0, blockSize);

                            if (read == 0)
                                throw new Exception("ReadFileChunk: Reading beyond end of stream.");

                            readSize -= read;
                        }
                    }
                }
                catch (Exception)
                {
                    returnValue = false;
                }
            }
            else
            {
                using (Stream fileStream = FileSingleton.Open(filePath, PortableFileMode.Create))
                {
                    try
                    {
                        int readSize = fileLength;
                        const int bufferSize = 0x1000;
                        byte[] buffer = new byte[bufferSize];
                        int read;
                        int blockSize;

                        while (readSize > 0)
                        {
                            blockSize = (readSize < bufferSize ? readSize : bufferSize);

                            read = stream.Read(buffer, 0, blockSize);

                            if (read == 0)
                                throw new Exception("ReadFileChunk: Reading beyond end of stream.");

                            fileStream.Write(buffer, 0, read);

                            readSize -= read;
                        }
                    }
                    catch (Exception)
                    {
                        returnValue = false;
                    }
                    finally
                    {
                        FileSingleton.Close(fileStream);
                    }
                }
            }

            return returnValue;
        }

        public bool WriteFileChunk(Stream stream, string mediaDir, string filePath)
        {
            string logicalFilePath;
            int fileSize = 0;
            bool returnValue = true;

            try
            {
                fileSize = (int)FileSingleton.GetFileSize(filePath);
            }
            catch (Exception)
            {
                fileSize = 0;
            }

            string relativeFilePath = filePath.ToLower();

            if (relativeFilePath.StartsWith(mediaDir.ToLower()))
                logicalFilePath = filePath.Substring(mediaDir.Length);
            else if (relativeFilePath.StartsWith(globalUserMediaDirLower))
                logicalFilePath = filePath.Substring(globalUserMediaDirLower.Length);
            else
                logicalFilePath = MediaUtilities.MakeRelativePath(MediaFilePath, filePath);

            int logicalFilePathsize = GetStringFieldLength(logicalFilePath);
            int chunkSize = logicalFilePathsize + fileSize + GetPackedIntegerSize(fileSize);

            stream.WriteByte(FileChunkCode);

            if (!WritePackedInteger(stream, chunkSize))
                return false;

            if (!WriteString(stream, logicalFilePath))
                return false;

            if (!WritePackedInteger(stream, fileSize))
                return false;

            if (fileSize != 0)
            {
                using (Stream fileStream = FileSingleton.OpenRead(filePath))
                {
                    try
                    {
                        const int bufferSize = 0x1000;
                        byte[] buffer = new byte[bufferSize];
                        int read;
                        int readAccum = 0;

                        while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                        {
                            stream.Write(buffer, 0, read);
                            readAccum += read;
                        }

                        if (readAccum != fileSize)
                            return false;
                    }
                    catch (Exception)
                    {
                        returnValue = false;
                    }
                    finally
                    {
                        FileSingleton.Close(fileStream);
                    }
                }
            }

            return returnValue;
        }

        public bool ReadSkipChunk(Stream stream, int expectedChunkType)
        {
            int chunkType = stream.ReadByte();
            int chunkSize;
            bool returnValue = true;

            if (chunkType != expectedChunkType)
                return false;

            if (!ReadPackedInteger(stream, out chunkSize))
                return false;

            try
            {
                if (stream.CanSeek)
                    stream.Seek(chunkSize, SeekOrigin.Current);
                else
                {
                    for (int index = 0; index < chunkSize; index++)
                    {
                        if (stream.ReadByte() == -1)
                            return false;
                    }
                }
            }
            catch (Exception)
            {
                returnValue = false;
            }

            return returnValue;
        }

        public bool ReadSectionChunk(Stream stream, out int sectionType, out int chunkCount)
        {
            int chunkType = stream.ReadByte();
            int chunkSize;

            sectionType = 0;
            chunkCount = 0;

            if (chunkType != SectionHeaderChunkCode)
                return false;

            if (!ReadPackedInteger(stream, out chunkSize))
                return false;

            sectionType = stream.ReadByte();

            if (!ReadPackedInteger(stream, out chunkCount))
                return false;

            return true;
        }

        public bool WriteSectionChunk(Stream stream, int sectionType, int chunkCount)
        {
            int chunkSize = 1 + GetPackedIntegerSize(chunkCount);

            stream.WriteByte(SectionHeaderChunkCode);

            if (!WritePackedInteger(stream, chunkSize))
                return false;

            stream.WriteByte((byte)sectionType);

            if (!WritePackedInteger(stream, chunkCount))
                return false;

            return true;
        }

        public bool ReadData(Stream stream, int size, byte[] data)
        {
            int readSize = 0;

            while (readSize < size)
            {
                int readCount = stream.Read(data, readSize, size - readSize);

                if (readCount == 0)
                    return false;

                readSize += readCount;
            }

            return true;
        }

        public bool WriteData(Stream stream, int size, byte[] data)
        {
            stream.Write(data, 0, size);
            return true;
        }

        protected void Initialize()
        {
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string dir = ApplicationData.TempPath + ApplicationData.PlatformPathSeparator +
                userName + ApplicationData.PlatformPathSeparator;
            string mediaDir;

            switch (TargetType)
            {
                case "DictionaryEntry":
                    globalMediaDir = ApplicationData.ContentPath +
                        ApplicationData.PlatformPathSeparator +
                        "Dictionary" +
                        ApplicationData.PlatformPathSeparator;
                    globalUserMediaDir = globalMediaDir;
                    break;
                default:
                    globalMediaDir = ApplicationData.MediaPath + ApplicationData.PlatformPathSeparator;
                    if (String.IsNullOrEmpty(UserMediaFilePath))
                        globalUserMediaDir = globalMediaDir + userName + ApplicationData.PlatformPathSeparator;
                    else
                        globalUserMediaDir = UserMediaFilePath + ApplicationData.PlatformPathSeparator;
                    break;
            }

            globalUserMediaDirLower = globalUserMediaDir.ToLower();
            externalUserPathNode = "externalUser" + ApplicationData.PlatformPathSeparator;
            externalUserPath = externalUserPathNode;
            externalUserPathLower = externalUserPath.ToLower();

            if ((Targets != null) && (Targets.Count() == 1) && (Targets[0] is BaseObjectTitled))
            {
                BaseObjectTitled titledObject = Targets[0] as BaseObjectTitled;

                if (titledObject is BaseObjectNode)
                {
                    BaseObjectNode node = titledObject as BaseObjectNode;
                    mediaDir = node.MediaDirectoryPath;
                }
                else
                    mediaDir = titledObject.MediaDirectoryPath;

                mediaDir += ApplicationData.PlatformPathSeparator;
            }
            else if (!String.IsNullOrEmpty(TargetMediaDirectory))
                mediaDir = TargetMediaDirectory + ApplicationData.PlatformPathSeparator;
            else
                mediaDir = globalUserMediaDir;

            MediaFilePath = mediaDir;
            mediaDirSlashLower = mediaDir.ToLower();
            directoriesToDelete = new List<string>();
            MediaFiles = new List<string>();
            Archive = FileSingleton.Archive();
        }

        protected void Reset()
        {
            MediaFiles = null;
            Archive = null;
        }

        public static string IncludeMediaHelp = "Check this to include media files.";
        public static string OverwriteExistingMediaHelp = "Check this to overwrite existing media files.";
        public static string MediaLanguagesHelp = "Select media languages.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            PreserveTargetNames = GetFlagArgumentDefaulted("PreserveTargetNames", "flag", "r",
                PreserveTargetNames, "Preserve target names", PreserveTargetNamesHelp, null, null);

            MakeTargetPublic = GetFlagArgumentDefaulted("MakeTargetPublic", "flag", "r",
                MakeTargetPublic, "Make public", MakeTargetPublicHelp, null, null);

            if (!PreserveGuid)
                PreserveGuid = PreserveTargetNames;

            IncludeMedia = GetFlagArgumentDefaulted("IncludeMedia", "flag", "rw",
                IncludeMedia, "Include media", IncludeMediaHelp,
                new List<string>(2) { "MediaLanguages", "OverwriteExistingMedia" }, null);

            string direction = (ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion ? "w" : "r");

            OverwriteExistingMedia = GetFlagArgumentDefaulted("OverwriteExistingMedia", "flag", direction,
                OverwriteExistingMedia, "Overwrite existing media", OverwriteExistingMediaHelp,
                null, null);

            LanguageFlags = GetFlagListArgumentDefaulted("MediaLanguages", "languageflaglist", "w",
                LanguageFlags, LanguageFlagNames, "Media languages", MediaLanguagesHelp, null, null);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetFlagArgument("PreserveTargetNames", "flag", "r", PreserveTargetNames, "Preserve target names",
                PreserveTargetNamesHelp, null, null);

            SetFlagArgument("MakeTargetPublic", "flag", "r", MakeTargetPublic, "Make targets public",
                MakeTargetPublicHelp, null, null);

            SetFlagArgument("IncludeMedia", "flag", "rw", IncludeMedia, "Include media",
                IncludeMediaHelp, null, null);

            string direction = (ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion ? "w" : "r");

            SetFlagArgument("OverwriteExistingMedia", "flag", direction, OverwriteExistingMedia, "Overwrite existing media",
                OverwriteExistingMediaHelp, null, null);

            SetFlagListArgument("MediaLanguages", "languageflaglist", "w", LanguageFlags, LanguageFlagNames, "Media languages",
                MediaLanguagesHelp, null, null);
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
                case "DictionaryEntry":
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

        public static new string TypeStringStatic { get { return "JTLanguage Chunky"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
