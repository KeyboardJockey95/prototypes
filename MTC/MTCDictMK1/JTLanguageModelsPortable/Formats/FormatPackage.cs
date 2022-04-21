using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatPackage : FormatXml
    {
        public string UserMediaFilePath;
        public bool IncludeMedia { get; set; }
        private List<string> MediaFiles;
        private string ContentFilePath;
        private string MediaFilePath;
        private static string FormatDescription = "Native JTLanguage package format."
            + " This is basically a .zip file containing: Content.xml, mediaFile1, mediaFile2, etc.,"
            + " where Content.xml is an object dump in the JTLangauge XML format.";

        public FormatPackage()
            : base("JTLanguage Package", "FormatPackage", FormatDescription, String.Empty, String.Empty,
                  "application/gzip", ".jtp", null, null, null, null, null)
        {
            UserMediaFilePath = null;
            IncludeMedia = true;
        }

        public FormatPackage(FormatPackage other)
            : base(other)
        {
            UserMediaFilePath = other.UserMediaFilePath;
            IncludeMedia = other.IncludeMedia;
        }

        public override Format Clone()
        {
            return new FormatPackage(this);
        }

        private string globalMediaDir;
        private string globalUserMediaDir;
        private string globalUserMediaDirLower;
        private string externalUserPathNode;
        private string externalUserPath;
        private string externalUserPathLower;
        private List<string> directoriesToDelete;

        public override void Read(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string dir = ApplicationData.TempPath + ApplicationData.PlatformPathSeparator +
                userName + ApplicationData.PlatformPathSeparator;
            string filePath;
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
            externalUserPathNode = "externalUser/";
            externalUserPath = externalUserPathNode;
            externalUserPathLower = externalUserPath.ToLower();
            directoriesToDelete = new List<string>();

            MediaFilePath = null;

            if ((Targets != null) && (Targets.Count() == 1) && (Targets[0] is BaseObjectTitled))
            {
                BaseObjectTitled titledObject = Targets[0] as BaseObjectTitled;

                if (titledObject is BaseObjectNode)
                {
                    BaseObjectNode node = titledObject as BaseObjectNode;

                    //if (node.Tree != null)
                    //    mediaDir = node.Tree.MediaDirectoryPath;
                    //else
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
            ContentFilePath = mediaDir + "Content.xml";
            MediaFiles = new List<string>();

            stream.Seek(0, SeekOrigin.Begin);

            if ((b1 == 'P') && (b2 == 'K'))
            {
                filePath = dir + "mediapackage" + DefaultFileExtension;

                if (FileSingleton.Exists(filePath))
                    FileSingleton.Delete(filePath);

                if (!WriteToTemporary(stream, filePath))
                    return;

                IArchiveFile zipFile = null;

                bool saveDeleteBeforeImport = DeleteBeforeImport;
                DeleteBeforeImport = false;

                try
                {
                    zipFile = FileSingleton.Archive();

                    if (zipFile.Create(filePath))
                    {
                        ContinueProgress(zipFile.Count() + 2);
                        zipFile.Extract(mediaDir, DeleteBeforeImport, HandleReadFile);
                        zipFile.Close();
                        zipFile = null;
                    }

                    UpdateProgressElapsed("Converting media files...");
                    PostProcessMediaFiles(mediaDir, MediaFiles);
                }
                catch (Exception exc)
                {
                    string msg = "Exception during zip: " + exc.Message;
                    if (exc.InnerException != null)
                        msg += exc.InnerException.Message;
                    throw new Exception(msg);
                }
                finally
                {
                    if (zipFile != null)
                        zipFile.Close();

                    DeleteBeforeImport = saveDeleteBeforeImport;

                    UpdateProgressElapsed("Cleaning up...");

                    if (FileSingleton.Exists(ContentFilePath))
                        FileSingleton.Delete(ContentFilePath);

                    if (FileSingleton.Exists(filePath))
                        FileSingleton.Delete(filePath);

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

                    string externalPath = mediaDir + externalUserPathNode;

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

                    ContentFilePath = null;
                    MediaFiles = null;

                    EndContinuedProgress();
                }
            }
            else
            {
                Error = "Read not supported for this file type.";
                throw new Exception(Error);
            }
        }

        public bool HandleReadFile(string filePath, ref string baseDirectory)
        {
            bool returnValue = true;

            if (filePath == "Content.xml")
            {
                Stream contentStream = null;

                try
                {
                    contentStream = FileSingleton.OpenRead(ContentFilePath);

                    if (contentStream != null)
                    {
                        // Get the content.
                        base.Read(contentStream);

                        string targetDir = null;

                        if (Targets != null)
                        {
                            if (Targets.Count() == 1)
                            {
                                if (Targets[0] is BaseObjectTitled)
                                    targetDir = ((BaseObjectTitled)Targets[0]).Directory;
                            }
                        }
                        else if ((ReadObjects != null) && (ReadObjects.Count() != 0))
                        {
                            int readTargetCount = CountReadTargets();

                            if (readTargetCount == 1)
                            {
                                IBaseObject lastObject = ReadObjects.Last();

                                if (lastObject is BaseObjectTitled)
                                {
                                    BaseObjectTitled titledObject = (BaseObjectTitled)lastObject;
                                    targetDir = titledObject.Directory;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(targetDir))
                        {
                            if (!baseDirectory.EndsWith(ApplicationData.PlatformPathSeparator + targetDir + ApplicationData.PlatformPathSeparator)
                                    && !baseDirectory.EndsWith(ApplicationData.PlatformPathSeparator + targetDir))
                            {
                                if (!baseDirectory.EndsWith(ApplicationData.PlatformPathSeparator))
                                    baseDirectory += ApplicationData.PlatformPathSeparator + targetDir;
                                else
                                    baseDirectory += targetDir;

                                if (!baseDirectory.EndsWith(ApplicationData.PlatformPathSeparator))
                                    baseDirectory += ApplicationData.PlatformPathSeparator;

                                MediaFilePath = baseDirectory;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    returnValue = false;
                }
                finally
                {
                    if (contentStream != null)
                        FileSingleton.Close(contentStream);

                    UpdateProgressElapsed("Unpacking media files...");
                }
            }
            else
            {
                string fileName = MediaUtilities.GetFileName(filePath);
                string filePathLower = filePath.ToLower();

                if (filePathLower.StartsWith(externalUserPathLower))
                {
                    string newFilePath = filePath.Substring(externalUserPath.Length);
                    string fullNewFilePath = globalUserMediaDir + newFilePath;
                    string fullFilePath = MediaFilePath + filePath;
                    fullFilePath = fullFilePath.Replace(@"\", @"/");
                    fullNewFilePath = fullNewFilePath.Replace(@"\", @"/");
                    string deletePath = MediaUtilities.GetFilePath(fullFilePath);
                    if (!directoriesToDelete.Contains(deletePath))
                        directoriesToDelete.Add(deletePath);
                    try
                    {
                        FileSingleton.DirectoryExistsCheck(fullNewFilePath);
                        if (FileSingleton.Exists(fullNewFilePath))
                            FileSingleton.Delete(fullNewFilePath);
                        FileSingleton.Move(fullFilePath, fullNewFilePath);
                    }
                    catch (Exception)
                    {
                        returnValue = false;
                    }
                    MediaFiles.Add(newFilePath);
                }
                else
                    MediaFiles.Add(filePath);

                UpdateProgressElapsed("Unpacked " + fileName + "...");
            }

            return returnValue;
        }

        private int CountReadTargets()
        {
            int count = 0;

            foreach (IBaseObject readTarget in ReadObjects)
            {
                if (readTarget.GetType().Name == TargetType)
                    count++;
            }

            return count;
        }

        public override void Write(Stream stream)
        {
            List<string> mediaFiles = new List<string>();
            string mediaDir;
            string mediaDirSlashLower;
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");

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

            if ((Targets != null) && (Targets.Count() == 1) && (Targets[0] is BaseObjectTitled))
            {
                BaseObjectTitled titledObject = Targets[0] as BaseObjectTitled;

                if (titledObject is BaseObjectNode)
                {
                    BaseObjectNode node = titledObject as BaseObjectNode;

                    //if (node.Tree != null)
                    //    mediaDir = node.Tree.MediaDirectoryPath;
                    //else
                        mediaDir = node.MediaDirectoryPath;
                }
                else
                   mediaDir = titledObject.MediaDirectoryPath;

                mediaDir += ApplicationData.PlatformPathSeparator;
            }
            else
                mediaDir = globalUserMediaDir;

            mediaDirSlashLower = mediaDir.ToLower();

            string contentFilePath = mediaDir + "Content.xml";

            // Collect the media files.
            if (IncludeMedia && (Targets != null))
            {
                foreach (IBaseObject target in Targets)
                    CollectMediaFiles(target, mediaFiles);
            }

            Stream contentStream = null;

            try
            {
                FileSingleton.DirectoryExistsCheck(contentFilePath);

                contentStream = FileSingleton.Create(contentFilePath);

                if (contentStream != null)
                {
                    // Write the content file.
                    base.Write(contentStream);
                }
            }
            catch (Exception exc)
            {
                Error = "Exception during content read: " + exc.Message;

                if (exc.InnerException != null)
                    Error = Error + ": " + exc.InnerException.Message;
            }
            finally
            {
                // Close the content file.
                FileSingleton.Close(contentStream);
            }

            IArchiveFile zipFile = FileSingleton.Archive();

            try
            {
                if (zipFile.Create())
                {
                    // Add the content file.
                    zipFile.AddFile(contentFilePath, "");

                    if (IncludeMedia)
                    {
                        // Add the media files.
                        foreach (string filePath in mediaFiles)
                        {
                            string relativeFilePath = filePath.ToLower();

                            if (relativeFilePath.StartsWith(mediaDirSlashLower))
                            {
                                relativeFilePath = filePath.Substring(mediaDirSlashLower.Length);
                                relativeFilePath = MediaUtilities.GetFilePath(relativeFilePath);
                            }
                            else if (relativeFilePath.StartsWith(globalUserMediaDirLower))
                            {
                                relativeFilePath = filePath.Substring(globalUserMediaDirLower.Length);
                                relativeFilePath = externalUserPathNode + MediaUtilities.GetFilePath(relativeFilePath);
                            }
                            else
                                continue;

                            if (FileSingleton.Exists(filePath))
                                zipFile.AddFile(filePath, relativeFilePath);
                        }
                    }

                    // Save the zip file.
                    zipFile.Save(stream);
                }
            }
            catch (Exception exc)
            {
                Error = "Exception during content zip create: " + exc.Message;

                if (exc.InnerException != null)
                    Error = Error + ": " + exc.InnerException.Message;
            }
            finally
            {
                if (zipFile != null)
                    zipFile.Close();

                // Delete the content file.
                FileSingleton.Delete(contentFilePath);
            }
        }

        public static string IncludeMediaHelp = "Check this to include media files.";
        public static string MediaLanguagesHelp = "Select media languages.";

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            IncludeMedia = GetFlagArgumentDefaulted("IncludeMedia", "flag", "w",
                IncludeMedia, "Include media", IncludeMediaHelp,
                new List<string>(1) { "MediaLanguages" }, null);

            LanguageFlags = GetFlagListArgumentDefaulted("MediaLanguages", "languageflaglist", "w",
                LanguageFlags, LanguageFlagNames, "Media languages", MediaLanguagesHelp, null, null);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            SetFlagArgument("IncludeMedia", "flag", "w", IncludeMedia, "Include media",
                IncludeMediaHelp, null, null);

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

        public static new string TypeStringStatic { get { return "JTLanguage Package"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
