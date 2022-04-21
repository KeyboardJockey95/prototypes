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
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatMediaZip : Format
    {
        // Parameters.
        private List<string> MediaFiles;
        private string MediaFilePath;

        private static string FormatDescription = "Format for importing/exporting media only from a .zip file.";

        public FormatMediaZip()
            : base(
                  "MediaZip",
                  "FormatMediaZip",
                  FormatDescription,
                  String.Empty,
                  String.Empty,
                  "text/plain",
                  ".zip",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
            ClearFormatMediaZip();
        }

        public FormatMediaZip(FormatMediaZip other)
            : base(other)
        {
            CopyFormatMediaZip(other);
        }

        public FormatMediaZip(
            string name,
            string type,
            string description,
            string targetType,
            string importExportType,
            string mimeType,
            string defaultExtension,
            UserRecord userRecord,
            UserProfile userProfile,
            IMainRepository repositories,
            LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities)
            : base(name, type, description, targetType, importExportType, mimeType, defaultExtension,
                  userRecord, userProfile, repositories, languageUtilities, nodeUtilities)
        {
            ClearFormatMediaZip();
        }

        public void ClearFormatMediaZip()
        {
        }

        public void CopyFormatMediaZip(FormatMediaZip other)
        {
        }

        public override Format Clone()
        {
            return new FormatMediaZip(this);
        }

        private string globalMediaDir;
        private string globalUserMediaDir;
        private string globalUserMediaDirLower;

        public override void Read(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string dir = ApplicationData.TempPath + ApplicationData.PlatformPathSeparator +
                userName + ApplicationData.PlatformPathSeparator;
            string filePath;
            string mediaDir;

            globalMediaDir = ApplicationData.MediaPath + ApplicationData.PlatformPathSeparator;

            globalUserMediaDir = globalMediaDir + userName + ApplicationData.PlatformPathSeparator;

            globalUserMediaDirLower = globalUserMediaDir.ToLower();

            MediaFilePath = null;

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

                try
                {
                    zipFile = FileSingleton.Archive();

                    if (zipFile.Create(filePath))
                    {
                        ContinueProgress(zipFile.Count() + 2);
                        zipFile.Extract(mediaDir, true, HandleReadFile);
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

                    UpdateProgressElapsed("Cleaning up...");

                    if (FileSingleton.Exists(filePath))
                        FileSingleton.Delete(filePath);

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

            ApplicationData.Global.PutConsoleMessage("HandleReadFile(\"" + filePath + "\", \"" + baseDirectory + "\")");

            string fileName = MediaUtilities.GetFileName(filePath);
            string fullFilePath = MediaUtilities.ConcatenateFilePath(
                MediaFilePath,
                filePath.Replace("/", ApplicationData.PlatformPathSeparator));
            string fullNewFilePath = MediaUtilities.ConcatenateFilePath(MediaFilePath, fileName);

            try
            {
                if (fullFilePath != fullNewFilePath)
                {
                    if (FileSingleton.Exists(fullNewFilePath))
                        FileSingleton.Delete(fullNewFilePath);

                    FileSingleton.Move(fullFilePath, fullNewFilePath);
                }
            }
            catch (Exception)
            {
                returnValue = false;
            }

            MediaFiles.Add(fileName);

            UpdateProgressElapsed("Unpacked " + fileName + "...");

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

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            if (importExport == "Export")
                return false;

            switch (contentName)
            {
                case "BaseObjectContent":
                case "ContentStudyList":
                    if (capability == "Support")
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

        public static new string TypeStringStatic { get { return "FormatZip"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
