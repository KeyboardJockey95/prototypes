using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatDictionaryMedia : Format
    {
        // Argument data.

        protected LanguageID LanguageID { get; set; }
        protected static string LanguageIDPrompt = "Language";
        protected static string LanguageIDHelp = "Enter the default language, or leave blank to determine it from the updload.";

        protected string Gender { get; set; }
        protected static string GenderPrompt = "Gender";
        protected static string GenderHelp = "Enter the default gender, or leave blank to determine it from the updload.";

        protected string Region { get; set; }
        protected static string RegionPrompt = "Region";
        protected static string RegionHelp = "Enter the default region, or leave blank to determine it from the updload.";

        protected string Country { get; set; }
        protected static string CountryPrompt = "Country";
        protected static string CountryHelp = "Enter the default country, or leave blank to determine it from the updload.";

        protected string Speaker { get; set; }
        protected static string SpeakerPrompt = "Speaker";
        protected static string SpeakerHelp = "Enter the default speaker, or leave blank to determine it from the updload or current user.";

        protected List<string> Tags { get; set; }
        protected static string TagsPrompt = "Tags";
        protected static string TagsHelp = "Enter the default comma-separated lists of optional tags to set.";

        protected bool IncludeMediaRecords { get; set; }
        protected static string IncludeMediaRecordsPrompt = "Include media records";
        protected static string IncludeMediaRecordsHelp = "Check this to include media files.";

        protected bool IncludeMediaFiles { get; set; }
        protected static string IncludeMediaFilesPrompt = "Include media files";
        protected static string IncludeMediaFilesHelp = "Check this to include media files.";

        protected bool UseMediaPaths { get; set; }
        protected static string UseMediaPathsPrompt = "Use media paths";
        protected static string UseMediaPathsHelp = "Check this to use subdirectory paths in the zip file.";

        protected bool Merge { get; set; }
        protected static string MergePrompt = "Merge";
        protected static string MergeHelp = "Check this to merge upload with existing instances and files.";

        protected bool Overwrite { get; set; }
        protected static string OverwritePrompt = "Overwrite";
        protected static string OverwriteHelp = "Check this overwrite existing instances or files.";

        // Implementation.

        // Path to the where the .zip is unpacked.
        private string ZipDirectoryFilePath;

        // Path to the media records ZML file.
        private string ContentFilePath;

        // Base path for the dictionary media (".../Content/Dictionary").
        private string DictionaryDirectory;

        // List of zip media file paths. In sync with DictionaryMediaFilePaths.
        private List<string> ZipMediaFilePaths;

        // List of dictionary media file paths. In sync with ZipMediaFilePaths.
        private List<string> DictionaryMediaFilePaths;

        // List of zip media relative file paths. In sync with ZipMediaFilePaths.
        private List<string> ZipMediaRelativePaths;

        // The content element read.
        private XElement ContentElement;

        private static string FormatDescription = "Format for importing/exporting dictionary media from a"
            + " .zip of the output of the program \"subs2srs\".";

        public FormatDictionaryMedia()
            : base(
                  "Dictionary Media",
                  "FormatDictionaryMedia",
                  FormatDescription,
                  String.Empty,
                  String.Empty,
                  "application/gzip",
                  ".zip",
                  null,
                  null,
                  null,
                  null,
                  null)
        {
            ClearFormatDictionaryMedia();
        }

        public FormatDictionaryMedia(FormatDictionaryMedia other)
            : base(other)
        {
            CopyFormatDictionaryMedia(other);
        }

        public FormatDictionaryMedia(
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
            ClearFormatDictionaryMedia();
        }

        public void ClearFormatDictionaryMedia()
        {
            // Local parameters.

            LanguageID = null;
            Gender = null;
            Region = null;
            Country = null;
            Speaker = null;
            Tags = null;
            IncludeMediaRecords = true;
            IncludeMediaFiles = true;
            UseMediaPaths = true;
            Merge = true;
            Overwrite = true;

            // Implementation.

            ZipDirectoryFilePath = null;
            ContentFilePath = null;
            DictionaryDirectory = null;
            ZipMediaFilePaths = null;
            DictionaryMediaFilePaths = null;
            ZipMediaRelativePaths = null;
            ContentElement = null;
        }

        public void CopyFormatDictionaryMedia(FormatDictionaryMedia other)
        {
            LanguageID = other.LanguageID;
            Gender = other.Gender;
            Region = other.Region;
            Country = other.Country;
            Speaker = other.Speaker;
            Tags = (other.Tags != null ? new List<string>(other.Tags) : null);
            IncludeMediaRecords = other.IncludeMediaRecords;
            IncludeMediaFiles = other.IncludeMediaFiles;
            UseMediaPaths = other.UseMediaPaths;
            Merge = other.Merge;
            Overwrite = other.Overwrite;
        }

        public override Format Clone()
        {
            return new FormatDictionaryMedia(this);
        }

        public override void Read(Stream stream)
        {
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string userTempDir = MediaUtilities.ConcatenateFilePath(ApplicationData.TempPath, userName);
            string filePath;

            ZipDirectoryFilePath = MediaUtilities.ConcatenateFilePath(userTempDir, "Zip");
            DictionaryDirectory = MediaUtilities.ConcatenateFilePath(ApplicationData.ContentPath, "Dictionary");
            ContentFilePath = null;
            ZipMediaFilePaths = new List<string>();
            DictionaryMediaFilePaths = new List<string>();
            ContentElement = null;

            stream.Seek(0, SeekOrigin.Begin);

            if ((b1 == 'P') && (b2 == 'K'))
            {
                filePath = MediaUtilities.ConcatenateFilePath(userTempDir, "mediapackage" + DefaultFileExtension);

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
                        zipFile.Extract(ZipDirectoryFilePath, true, HandleReadFile);
                        zipFile.Close();
                        zipFile = null;
                    }

                    if (IncludeMediaRecords)
                    {
                        if (ContentElement != null)
                            ProcessContentRecords(ContentElement);
                        else if (DictionaryMediaFilePaths.Count() != 0)
                            CreateMediaRecordsFromFiles(DictionaryMediaFilePaths);
                        else
                            PutError("No media records.");
                    }

                    if (IncludeMediaFiles)
                        CopyZippedMediaToDictionaryMedia();
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

                    CleanUp(filePath);

                    EndContinuedProgress();
                }
            }
            else
            {
                Error = "Read not supported for this file type.";
                throw new Exception(Error);
            }
        }

        public override void Write(Stream stream)
        {
            if ((Targets == null) || (Targets.Count() == 0))
            {
                PutError("Nothing to export.");
                return;
            }

            string userName = (UserRecord != null ? UserRecord.UserName : "Guest");
            string userTempDir = MediaUtilities.ConcatenateFilePath(ApplicationData.TempPath, userName);

            ZipDirectoryFilePath = MediaUtilities.ConcatenateFilePath(userTempDir, "Zip");
            DictionaryDirectory = MediaUtilities.ConcatenateFilePath(ApplicationData.ContentPath, "Dictionary");
            ContentFilePath = MediaUtilities.ConcatenateFilePath(ZipDirectoryFilePath, "MediaRecords.xml");
            ZipMediaFilePaths = new List<string>();
            DictionaryMediaFilePaths = new List<string>();
            ZipMediaRelativePaths = new List<string>();

            // Clean up anything that may have been left over from an aborted run.
            CleanUp(null);

            XElement contentElement = new XElement("MediaRecords");
            List<AudioMultiReference> audioReferences = new List<AudioMultiReference>();
            List<AudioInstance> audioInstances = new List<AudioInstance>();

            foreach (IBaseObject target in Targets)
            {
                if (target is AudioMultiReference)
                {
                    AudioMultiReference audioReference = target as AudioMultiReference;

                    if (!audioReferences.Contains(audioReference))
                    {
                        audioReferences.Add(audioReference);
                        contentElement.Add(audioReference.Xml);
                        GetMediaFilesFromAudioReference(audioReference);
                    }
                }
                else if (target is AudioInstance)
                {
                    AudioInstance audioInstance = target as AudioInstance;

                    if (!audioInstances.Contains(audioInstance))
                    {
                        audioInstances.Add(target as AudioInstance);
                        contentElement.Add(audioInstance.Xml);
                        GetMediaFileFromAudioInstance(audioInstance, LanguageID);
                    }
                }
            }

            if (IncludeMediaRecords)
            {
                Stream contentStream = null;

                try
                {
                    FileSingleton.DirectoryExistsCheck(ContentFilePath);

                    contentStream = FileSingleton.Create(ContentFilePath);

                    using (StreamWriter writer = new StreamWriter(contentStream, new System.Text.UTF8Encoding(true)))
                    {
                        contentElement.Save(writer);
                        writer.Flush();
                    }
                }
                catch (Exception exc)
                {
                    PutExceptionError("Exception during content write", exc);
                }
                finally
                {
                    // Close the content file.
                    FileSingleton.Close(contentStream);
                }
            }

            IArchiveFile zipFile = FileSingleton.Archive();

            try
            {
                if (zipFile.Create())
                {
                    // Add the content file.
                    if (IncludeMediaRecords)
                        zipFile.AddFile(ContentFilePath, "");

                    if (IncludeMediaFiles)
                    {
                        int mediaCount = DictionaryMediaFilePaths.Count();
                        int mediaIndex;

                        // Add the media files.
                        for (mediaIndex = 0; mediaIndex < mediaCount; mediaIndex++)
                        {
                            string mediaFilePath = DictionaryMediaFilePaths[mediaIndex];
                            string relativeFilePath = MediaUtilities.GetFilePath(ZipMediaRelativePaths[mediaIndex]);

                            if (FileSingleton.Exists(mediaFilePath))
                                zipFile.AddFile(mediaFilePath, relativeFilePath);
                        }
                    }

                    // Save the zip file.
                    zipFile.Save(stream);
                }
            }
            catch (Exception exc)
            {
                PutExceptionError("Exception during zip create", exc);
            }
            finally
            {
                if (zipFile != null)
                    zipFile.Close();

                CleanUp(null);
            }
        }

        public bool HandleReadFile(string filePath, ref string baseDirectory)
        {
            bool returnValue = true;

            ApplicationData.Global.PutConsoleMessage("HandleReadFile(\"" + filePath + "\", \"" + baseDirectory + "\")");

            if (filePath.EndsWith(".xml"))
            {
                Stream contentStream = null;

                try
                {
                    ContentFilePath = MediaUtilities.ConcatenateFilePath(baseDirectory, filePath);
                    contentStream = FileSingleton.OpenRead(ContentFilePath);

                    if (contentStream != null)
                    {
                        try
                        {
                            ContentElement = XElement.Load(contentStream);
                        }
                        catch (Exception exc)
                        {
                            PutExceptionError("Error parsing content element", exc);
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
            else if (filePath.EndsWith("/"))
            {
                // Just a subdirectory.
            }
            else if (String.IsNullOrEmpty(filePath))
            {
                // Shouldn't happen.
            }
            else
            {
                List<string> fileNodes = MediaUtilities.GetFilePathNodes(filePath);
                string fileName = fileNodes.Last();
                string mediaClass = MediaUtilities.GetDictionaryMediaClassFromFileName(fileName);
                string languageCode = null;

                if (fileNodes.Count() == 1)
                {
                    if (LanguageID != null)
                        languageCode = LanguageID.LanguageCultureExtensionCode;
                    else
                        PutError("Need a language ID.");
                }
                else if (fileNodes.Count() == 2)
                {
                    if (LanguageLookup.IsKnownLanguageCode(fileNodes[0]))
                        languageCode = fileNodes[0];
                    else if (fileNodes[0] == mediaClass)
                    {
                    }
                    else
                    {
                        PutError("File path should be one of: \"(fileName)\", \"(languageCode)/(fileName)\", \"Audio/(fileName)\", \"Audio/(languageCode)/(fileName)\"");
                        return false;
                    }
                }
                else if (fileNodes.Count() == 3)
                {
                    if ((fileNodes[0] != mediaClass) || !LanguageLookup.IsKnownLanguageCode(fileNodes[1]))
                    {
                        PutError("File path should be one of: \"(fileName)\", \"(languageCode)/(fileName)\", \"Audio/(fileName)\", \"Audio/(languageCode)/(fileName)\"");
                        return false;
                    }

                    languageCode = fileNodes[1];
                }
                else
                {
                    PutError("File path should be one of: \"(fileName)\", \"(languageCode)/(fileName)\", \"Audio/(fileName)\", \"Audio/(languageCode)/(fileName)\"");
                    return false;
                }

                if (String.IsNullOrEmpty(languageCode))
                {
                    if (LanguageID != null)
                        languageCode = LanguageID.LanguageCultureExtensionCode;
                    else if (TargetLanguageID != null)
                        languageCode = TargetLanguageID.LanguageCultureExtensionCode;
                }

                string fullZipFilePath = MediaUtilities.ConcatenateFilePath(
                    ZipDirectoryFilePath,
                    filePath.Replace("/", ApplicationData.PlatformPathSeparator));
                string fullMediaFilePath = MediaUtilities.ConcatenateFilePath(
                    MediaUtilities.ConcatenateFilePath(
                        MediaUtilities.ConcatenateFilePath(
                            DictionaryDirectory,
                            mediaClass),
                        languageCode),
                    fileName);

                ZipMediaFilePaths.Add(fullZipFilePath);
                DictionaryMediaFilePaths.Add(fullMediaFilePath);

                UpdateProgressElapsed("Unpacked " + fileName + "...");
            }

            return returnValue;
        }

        protected void ProcessContentRecords(XElement recordsElement)
        {
            foreach (XElement childElement in recordsElement.Elements())
            {
                string elementType = childElement.Name.LocalName;

                switch (elementType)
                {
                    case "AudioMultiReference":
                        {
                            AudioMultiReference audioReference = new AudioMultiReference(childElement);
                            AddAudioReference(audioReference);
                        }
                        break;
                    case "AudioInstance":
                        {
                            AudioInstance audioInstance = new AudioInstance(childElement);
                            AddAudioInstance(audioInstance);
                        }
                        break;
                    default:
                        throw new Exception("Unexpected media record type: " + elementType);
                }
            }
        }

        protected void CreateMediaRecordsFromFiles(List<string> mediaFilePaths)
        {
            List<AudioMultiReference> audioRecords = new List<AudioMultiReference>();

            foreach (string filePath in mediaFilePaths)
            {
                string fileName = MediaUtilities.GetFileName(filePath);
                string fileBase = MediaUtilities.GetBaseFileName(fileName);
                string mimeType = MediaUtilities.GetMimeTypeFromFileName(fileName);
                string[] parts = fileBase.Split(LanguageLookup.Underscore);
                string word;
                LanguageID lagnuageID = GetLanguageIDFromMediaFilePath(filePath);
                List<KeyValuePair<string, string>> attributes = GetAttributes();

                if (parts.Length > 0)
                    word = parts[0];
                else
                    word = String.Empty;

                AudioInstance audioInstance = new AudioInstance(
                    word,
                    Owner,
                    mimeType,
                    fileName,
                    AudioInstance.UploadedSourceName,
                    attributes);

                if ((Tags != null) && (Tags.Count() != 0))
                    audioInstance.SetTags(Tags);

                AudioMultiReference audioReference = audioRecords.FirstOrDefault(x => TextUtilities.IsEqualStringsIgnoreCase(word, x.Name));

                if (audioReference == null)
                {
                    audioReference = new AudioMultiReference(
                        word,
                        LanguageID,
                        null);
                    audioRecords.Add(audioReference);
                }

                audioReference.AddAudioInstance(audioInstance);
            }

            foreach (AudioMultiReference audioReference in audioRecords)
                AddAudioReference(audioReference);
        }

        protected LanguageID GetLanguageIDFromMediaFilePath(string filePath)
        {
            if (filePath.StartsWith(DictionaryDirectory))
                filePath = filePath.Substring(DictionaryDirectory.Length + 1);

            List<string> fileNodes = MediaUtilities.GetFilePathNodes(filePath);
            string fileName = fileNodes.Last();
            string mediaClass = MediaUtilities.GetDictionaryMediaClassFromFileName(fileName);
            string languageCode = null;

            if (fileNodes.Count() == 1)
            {
                if (LanguageID != null)
                    languageCode = LanguageID.LanguageCultureExtensionCode;
                else
                    PutError("Need a language ID.");
            }
            else if (fileNodes.Count() == 2)
            {
                if (LanguageLookup.IsKnownLanguageCode(fileNodes[0]))
                    languageCode = fileNodes[0];
                else if (fileNodes[0] == mediaClass)
                {
                }
                else
                {
                    PutError("File path should be one of: \"(fileName)\", \"(languageCode)/(fileName)\", \"Audio/(fileName)\", \"Audio/(languageCode)/(fileName)\"");
                }
            }
            else if (fileNodes.Count() == 3)
            {
                if ((fileNodes[0] != mediaClass) || !LanguageLookup.IsKnownLanguageCode(fileNodes[1]))
                {
                    PutError("File path should be one of: \"(fileName)\", \"(languageCode)/(fileName)\", \"Audio/(fileName)\", \"Audio/(languageCode)/(fileName)\"");
                }

                languageCode = fileNodes[1];
            }
            else
            {
                PutError("File path should be one of: \"(fileName)\", \"(languageCode)/(fileName)\", \"Audio/(fileName)\", \"Audio/(languageCode)/(fileName)\"");
            }

            if (!String.IsNullOrEmpty(languageCode))
                return LanguageLookup.GetLanguageIDNoAdd(languageCode);

            return LanguageID;
        }

        protected List<KeyValuePair<string, string>> GetAttributes()
        {
            List<KeyValuePair<string, string>> attributes = new List<KeyValuePair<string, string>>();

            if (!String.IsNullOrEmpty(Gender))
                attributes.Add(new KeyValuePair<string, string>(AudioInstance.Gender, Gender));

            if (!String.IsNullOrEmpty(Region))
                attributes.Add(new KeyValuePair<string, string>(AudioInstance.Region, Region));

            if (!String.IsNullOrEmpty(Country))
                attributes.Add(new KeyValuePair<string, string>(AudioInstance.Country, Country));

            if (!String.IsNullOrEmpty(Speaker))
                attributes.Add(new KeyValuePair<string, string>(AudioInstance.Speaker, Speaker));

            return attributes;
        }

        protected void AddAudioReference(AudioMultiReference audioReference)
        {
            string word = audioReference.Name;
            LanguageID languageID = audioReference.LanguageID;
            AudioMultiReference oldReference = Repositories.DictionaryMultiAudio.Get(word, languageID);

            if (oldReference == null)
            {
                try
                {
                    if (!Repositories.DictionaryMultiAudio.Add(audioReference, languageID))
                        PutError("Error adding audio reference for: " + word);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error adding audio reference for: " + word, exc);
                }

                return;
            }

            if (!audioReference.HasAudioInstances())
                return;

            if (Overwrite)
            {
                DeleteAudioReference(oldReference);

                try
                {
                    if (!Repositories.DictionaryMultiAudio.Add(audioReference, languageID))
                        PutError("Error adding audio reference for: " + word);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error adding audio reference for: " + word, exc);
                }

                return;
            }

            foreach (AudioInstance audioInstance in audioReference.AudioInstances)
            {
                AudioInstance oldInstance = audioReference.GetAudioInstanceBySourceAndAttributes(
                    audioInstance.SourceName,
                    audioInstance.Attributes);

                if (oldInstance != null)
                    oldInstance = audioReference.GetAudioInstanceByFileName(audioInstance.FileName);

                if (oldInstance != null)
                {
                    DeleteAudioInstance(oldReference, oldInstance);
                    audioReference.AddAudioInstance(audioInstance);
                }
            }
        }

        protected void AddAudioInstance(AudioInstance audioInstance)
        {
            string word = audioInstance.Name;
            LanguageID languageID = LanguageID;

            if (languageID == null)
                throw new Exception("Need default language.");

            AudioMultiReference oldReference = Repositories.DictionaryMultiAudio.Get(word, languageID);

            if (oldReference == null)
            {
                AudioMultiReference audioReference = new AudioMultiReference(
                    word,
                    languageID,
                    null);

                audioReference.AddAudioInstance(audioInstance);

                try
                {
                    if (!Repositories.DictionaryMultiAudio.Add(audioReference, languageID))
                        PutError("Error adding audio reference for: " + word);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error adding audio reference for: " + word, exc);
                }

                return;
            }

            AudioInstance oldInstance = oldReference.GetAudioInstanceBySourceAndAttributes(
                audioInstance.SourceName,
                audioInstance.Attributes);

            if (oldInstance != null)
                oldInstance = oldReference.GetAudioInstanceByFileName(audioInstance.FileName);

            if (oldInstance != null)
                DeleteAudioInstance(oldReference, oldInstance);

            oldReference.AddAudioInstance(audioInstance);

            try
            {
                if (!Repositories.DictionaryMultiAudio.Update(oldReference, languageID))
                    PutError("Error updating audio reference for: " + word);
            }
            catch (Exception exc)
            {
                PutExceptionError("Error updating audio reference for: " + word, exc);
            }
        }

        protected void DeleteAudioReference(AudioMultiReference audioReference)
        {
            LanguageID languageID = audioReference.LanguageID;

            if (audioReference.HasAudioInstances())
            {
                foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                    DeleteAudioInstance(audioInstance, languageID);
            }
        }

        protected void DeleteAudioInstance(AudioMultiReference audioReference, AudioInstance audioInstance)
        {
            LanguageID languageID = audioReference.LanguageID;

            DeleteAudioInstance(audioInstance, languageID);
            audioReference.DeleteAudioInstance(audioInstance);
        }

        protected void DeleteAudioInstance(AudioInstance audioInstance, LanguageID languageID)
        {
            string filePath = audioInstance.GetFilePath(languageID);

            try
            {
                FileSingleton.Delete(filePath);
            }
            catch (Exception exc)
            {
                PutExceptionError("Error deleting audio file: " + filePath, exc);
            }
        }

        protected void GetMediaFilesFromAudioReference(AudioMultiReference audioReference)
        {
            if (!audioReference.HasAudioInstances())
                return;

            foreach (AudioInstance audioInstance in audioReference.AudioInstances)
                GetMediaFileFromAudioInstance(audioInstance, audioReference.LanguageID);
        }

        protected void GetMediaFileFromAudioInstance(AudioInstance audioInstance, LanguageID languageID)
        {
            string fileName = audioInstance.FileName;
            string relativeFilePath;
            string fullFilePath;

            if ((languageID == null) || !languageID.IsLanguage())
                throw new Exception("Need a default language.");

            relativeFilePath = MediaUtilities.ConcatenateFilePath(
                MediaUtilities.ConcatenateFilePath(
                    "Audio",
                    languageID.LanguageCultureExtensionCode),
                fileName);
            fullFilePath = MediaUtilities.ConcatenateFilePath(DictionaryDirectory, relativeFilePath);

            DictionaryMediaFilePaths.Add(fullFilePath);

            if (!UseMediaPaths)
                relativeFilePath = fileName;

            ZipMediaRelativePaths.Add(relativeFilePath);
        }

        protected bool CopyZippedMediaToDictionaryMedia()
        {
            int count = ZipMediaFilePaths.Count();
            bool returnValue = true;

            if (count != DictionaryMediaFilePaths.Count())
                throw new Exception("ZipMediaFilePaths.Count() and DictionaryMediaFilePaths.Count() doesn't match.");

            UpdateProgressElapsed("Copying media files...");

            for (int index = 0; index < count; index++)
            {
                string zippedFilePath = ZipMediaFilePaths[index];
                string dictionaryFilePath = DictionaryMediaFilePaths[index];

                try
                {
                    if (FileSingleton.Exists(dictionaryFilePath))
                        FileSingleton.Delete(dictionaryFilePath);

                    FileSingleton.Move(zippedFilePath, dictionaryFilePath);
                }
                catch (Exception exc)
                {
                    PutExceptionError("Error coping media files", exc);
                    returnValue = false;
                }
            }

            return returnValue;
        }

        protected void CleanUp(string zipFilePath)
        {
            UpdateProgressElapsed("Cleaning up...");

            if (FileSingleton.Exists(ContentFilePath))
                FileSingleton.Delete(ContentFilePath);

            if (!String.IsNullOrEmpty(zipFilePath))
            {
                if (FileSingleton.Exists(zipFilePath))
                    FileSingleton.Delete(zipFilePath);
            }

            if (FileSingleton.DirectoryExists(ZipDirectoryFilePath))
                FileSingleton.DeleteDirectory(ZipDirectoryFilePath);
        }

        public override void LoadFromArguments()
        {
            base.LoadFromArguments();

            if (LanguageID == null)
                LanguageID = TargetLanguageID;

            LanguageID = GetLanguageIDArgumentDefaulted("LanguageID", "languageID", "rw", LanguageID, LanguageIDPrompt, LanguageIDHelp);
            Gender = GetArgumentDefaulted("Gender", "string", "r", Gender, GenderPrompt, GenderHelp);
            Region = GetArgumentDefaulted("Region", "string", "r", Region, RegionPrompt, RegionHelp);
            Country = GetArgumentDefaulted("Country", "string", "r", Country, CountryPrompt, CountryHelp);
            Speaker = GetArgumentDefaulted("Speaker", "string", "r", Speaker, SpeakerPrompt, SpeakerHelp);
            Tags = GetArgumentStringListDefaulted("Tags", "string", "r", Tags, TagsPrompt, TagsHelp);
            IncludeMediaRecords = GetFlagArgumentDefaulted("IncludeMediaRecords", "flag", "rw", IncludeMediaRecords, IncludeMediaRecordsPrompt, IncludeMediaRecordsHelp, null, null);
            IncludeMediaFiles = GetFlagArgumentDefaulted("IncludeMediaFiles", "flag", "rw", IncludeMediaFiles, IncludeMediaFilesPrompt, IncludeMediaFilesHelp, null, null);
            UseMediaPaths = GetFlagArgumentDefaulted("UseMediaPaths", "flag", "w", UseMediaPaths, UseMediaPathsPrompt, UseMediaPathsHelp, null, null);
            Merge = GetFlagArgumentDefaulted("Merge", "flag", "r", Merge, MergePrompt, MergeHelp, null, null);
            Overwrite = GetFlagArgumentDefaulted("Overwrite", "flag", "r", Overwrite, OverwritePrompt, OverwriteHelp, null, null);
        }

        public override void SaveToArguments()
        {
            base.SaveToArguments();

            if (LanguageID == null)
                LanguageID = TargetLanguageID;

            SetLanguageIDArgument("LanguageID", "languageID", "rw", LanguageID, LanguageIDPrompt, LanguageIDHelp);
            SetArgument("Gender", "string", "r", Gender, GenderPrompt, GenderHelp, null, null);
            SetArgument("Region", "string", "r", Region, RegionPrompt, RegionHelp, null, null);
            SetArgument("Country", "string", "r", Country, CountryPrompt, CountryHelp, null, null);
            SetArgument("Speaker", "string", "r", Speaker, SpeakerPrompt, SpeakerHelp, null, null);
            SetArgumentStringList("Tags", "string", "r", Tags, TagsPrompt, TagsHelp, null, null);
            SetFlagArgument("IncludeMediaRecords", "flag", "rw", IncludeMediaRecords, IncludeMediaRecordsPrompt, IncludeMediaRecordsHelp, null, null);
            SetFlagArgument("IncludeMediaFiles", "flag", "rw", IncludeMediaFiles, IncludeMediaFilesPrompt, IncludeMediaFilesHelp, null, null);
            SetFlagArgument("UseMediaPaths", "flag", "w", UseMediaPaths, UseMediaPathsPrompt, UseMediaPathsHelp, null, null);
            SetFlagArgument("Merge", "flag", "r", Merge, MergePrompt, MergeHelp, null, null);
            SetFlagArgument("Overwrite", "flag", "r", Overwrite, OverwritePrompt, OverwriteHelp, null, null);
        }

        public override void DumpArguments(string label)
        {
            base.DumpArguments(label);
        }

        public static new bool IsSupportedStatic(string importExport, string contentName, string capability)
        {
            switch (contentName)
            {
                case "AudioMultiReference":
                case "AudioInstance":
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

        public static new string TypeStringStatic { get { return "Dictionary Media"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
