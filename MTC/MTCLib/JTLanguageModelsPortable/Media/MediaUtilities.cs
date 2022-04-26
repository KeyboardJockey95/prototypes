using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Crawlers;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Media
{
    public static class MediaUtilities
    {
        // Keep in sync with MediaTypeCode.
        public static string[] MediaTypes =
        {
            "Unknown",
            "Audio",
            "Video",
            "Image",
            "TextFile",
            "PDF",
            "Embedded",
            "MultiPart"
        };
        public static string MimeTypeMp3 = "audio/mpeg3";
        public static string AudioFileExtension = ".mp3";
        public static string[] MimeTypes =
        {
            "",
            "audio/wav",
            "audio/mpeg3",
            "audio/ogg",
            "audio/speex",
            "audio/mp4a-latm",
            "audio/3gpp",
            "audio/amr",
            "video/x-ms-wmv",
            "video/mp4",
            "video/ogg",
            "video/webm",
            "image/jpeg",
            "image/png",
            "image/gif",
            "text/plaintext",
            "application/pdf",
            "application/xml",
            "multipart/form-data"
        };
        public static string[] SupportedMediaMimeTypes =
        {
            "audio/mp3",
            "audio/mpeg3",
            "audio/mpeg",
            "audio/ogg",
            "video/mp4",
            "video/ogg",
            "image/jpeg",
            "image/png",
            "image/gif",
            "text/plaintext",
            "application/pdf"
        };
        public static string SupportedMediaFormatsString = ".mp3, .ogg, .mp4, .jpg, .png, .gif, .txt, .pdf";
        public static string[] SupportedAudioMimeTypes =
        {
            "audio/mp3",
            "audio/mpeg3",
            "audio/mpeg"
        };
        public static string SupportedAudioFormatsString = ".mp3";
        public static string[] SupportedVideoMimeTypes =
        {
            "video/mp4"
        };
        public static string SupportedVideoFormatsString = ".mp4";
        public static string[] SupportedAudioVideoMimeTypes =
        {
            "audio/mp3",
            "audio/mpeg3",
            "audio/mpeg",
            "video/mp4"
        };
        public static string SupportedAudioVideoFormatsString = ".mp3, .mp4";
        public static string[] SupportedPictureMimeTypes =
        {
            "image/jpeg",
            "image/png",
            "image/gif"
        };
        public static string SupportedPictureFormatsString = ".jpg, .png, .gif";
        public static string[] MimeExtensions =
        {
            "audio/wav", ".wav",
            "audio/mpeg3", ".mp3",
            "audio/mpeg", ".mp3",
            "audio/mp3", ".mp3",
            "audio/ogg", ".ogg",
            "audio/speex", ".spx",
            "audio/mp4a-latm", ".m4a",
            "audio/3gpp", ".3gpp",
            "audio/amr", ".amr",
            "video/x-ms-wmv", ".wmv",
            "video/mp4", ".mp4",
            "video/ogg", ".ogg",
            "video/webm", ".webm",
            "video/3gpp", ".3gpp",
            "image/jpeg", ".jpg",
            "image/png", ".png",
            "image/gif", ".gif",
            "text/plaintext", ".txt",
            "application/pdf", ".pdf",
            "application/xml", ".xml",
            "application/unknown", ".exe",
            "multipart/form-data",""
        };
        public static string[] FileTypeExtensions =
        {
            "Audio", ".wav",
            "Audio", ".mp3",
            "Audio", ".mp3",
            "Audio", ".mp3",
            "Audio", ".ogg",
            "Audio", ".spx",
            "Audio", ".m4a",
            "Audio", ".3gpp",
            "Audio", ".amr",
            "Video", ".wmv",
            "Video", ".mp4",
            "Video", ".ogg",
            "Video", ".webm",
            "Video", ".3gpp",
            "Image", ".jpg",
            "Image", ".png",
            "Image", ".gif",
            "Text", ".txt",
            "Application", ".pdf",
            "Application", ".xml",
            "Application", ".exe",
            "Multipart",""
        };
        public static string[] ContentTypes =
        {
            "Full Lesson",
            "Text",
            "Dialog",
            "Vocabulary",
            "Exercises",
            "Notes",
            "Review",
            "Other"
        };

        public static int MaxMediaFilePathLength = 260;
        public static int MaxMediaFileNameLength = 150;
        //public static long MaxMediaPackageSize = 55000000;
        public static long MaxMediaPackageSize = 100000000;

        public static bool VisitMediaAlwaysCollect(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                mediaFiles.Add(filePath);

            return true;
        }

        public static bool VisitMediaConditionalCollect(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            if (FileSingleton.Exists(filePath))
            {
                if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                    mediaFiles.Add(filePath);
            }

            return true;
        }

        public static bool VisitMediaConditionalCollectMarkAbsent(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            if (FileSingleton.Exists(filePath))
            {
                if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                    mediaFiles.Add(filePath);
            }

            SetOwnerStorageState(ownerObject, MediaStorageState.Absent);

            return true;
        }

        public static bool VisitMediaConditionalCollectAndSetState(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            //ContentStorageStateCode contentStorageState;

            if (FileSingleton.Exists(filePath))
            {
                if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                    mediaFiles.Add(filePath);

                SetOwnerStorageState(ownerObject, MediaStorageState.Present);
                //contentStorageState = ContentStorageStateCode.NotEmpty;
            }
            else
            {
                SetOwnerStorageState(ownerObject, MediaStorageState.Absent);
                //contentStorageState = ContentStorageStateCode.Empty;
            }

            //if (ownerObject is MediaDescription)
            //    SetContentStorageState(content, contentStorageState);

            return true;
        }

        public static bool VisitMediaGetStateConditional(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            switch (GetOwnerStorageState(ownerObject))
            {
                case MediaStorageState.Present:
                case MediaStorageState.Downloaded:
                    if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                        mediaFiles.Add(filePath);
                    break;
                case MediaStorageState.Unknown:
                    if (FileSingleton.Exists(filePath))
                    {
                        if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                            mediaFiles.Add(filePath);
                        SetOwnerStorageState(ownerObject, MediaStorageState.Present);
                        //if (ownerObject is MediaDescription)
                        //    SetContentStorageState(content, ContentStorageStateCode.NotEmpty);
                    }
                    break;
                default:
                    if (FileSingleton.Exists(filePath))
                    {
                        if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                            mediaFiles.Add(filePath);
                        SetOwnerStorageState(ownerObject, MediaStorageState.Present);
                        //if (ownerObject is MediaDescription)
                        //    SetContentStorageState(content, ContentStorageStateCode.NotEmpty);
                    }
                    break;
            }

            return true;
        }

        public static bool VisitMediaMobileToBeDownloaded(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            string fileExt = GetFileExtension(filePath);

            // Only download mp3 and mp4.
            switch (fileExt.ToLower())
            {
                case ".mp3":
                case ".mp4":
                case ".png":
                case ".jpg":
                    break;
                default:
                    return true;
            }

            switch (GetOwnerStorageState(ownerObject))
            {
                case MediaStorageState.Present:
                case MediaStorageState.Downloaded:
                    if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                        mediaFiles.Add(filePath);
                    break;
                case MediaStorageState.BadLink:
                    break;
                case MediaStorageState.Absent:
                case MediaStorageState.Unknown:
                default:
                    if (!FileSingleton.Exists(filePath))
                    {
                        if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                            mediaFiles.Add(filePath);
                    }
                    break;
            }

            return true;
        }

        public static bool VisitMediaMobileToBeDownloadedOverwrite(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            string fileExt = GetFileExtension(filePath);

            // Only download mp3 and mp4.
            switch (fileExt.ToLower())
            {
                case ".mp3":
                case ".mp4":
                case ".png":
                case ".jpg":
                    break;
                default:
                    return true;
            }

            switch (GetOwnerStorageState(ownerObject))
            {
                case MediaStorageState.Present:
                case MediaStorageState.Downloaded:
                    if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                        mediaFiles.Add(filePath);
                    break;
                case MediaStorageState.BadLink:
                    break;
                case MediaStorageState.Absent:
                case MediaStorageState.Unknown:
                default:
                    if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                        mediaFiles.Add(filePath);
                    break;
            }

            return true;
        }

        public static bool VisitMediaDontCollectAlternatesToo(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            if (!MediaConvertSingleton.IsAlternate(filePath))
            {
                if (FileSingleton.Exists(filePath))
                {
                    if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                        mediaFiles.Add(filePath);
                }
            }

            return true;
        }

        public static bool VisitMediaCollectAlternatesToo(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            List<string> alternateFilePaths = MediaConvertSingleton.GetAllFileNames(filePath, mimeType);

            if (FileSingleton.Exists(filePath))
            {
                if ((mediaFiles != null) && !mediaFiles.Contains(filePath))
                    mediaFiles.Add(filePath);
            }

            foreach (string alternateFilePath in alternateFilePaths)
            {
                if (FileSingleton.Exists(alternateFilePath))
                {
                    if ((mediaFiles != null) && !mediaFiles.Contains(alternateFilePath))
                        mediaFiles.Add(alternateFilePath);
                }
            }

            return true;
        }

        public static bool VisitMediaLazyConvert(
            List<string> mediaFiles,
            object content,
            object ownerObject,
            string filePath,
            string mimeType)
        {
            string message;
            if (!FileSingleton.Exists(filePath))
                return true;
            MediaConvertSingleton.LazyConvert(filePath, mimeType, false, out message);
            if (mediaFiles != null)
                return VisitMediaCollectAlternatesToo(mediaFiles, content, ownerObject, filePath, mimeType);
            return true;
        }

        public static MediaStorageState GetOwnerStorageState(object ownerObject)
        {
            if (ownerObject == null)
                return MediaStorageState.BadLink;

            if (ownerObject is MediaRun)
                return ((MediaRun)ownerObject).StorageState;
            else if (ownerObject is MediaDescription)
                return ((MediaDescription)ownerObject).StorageState;
            else if (ownerObject is BaseObjectNode)
                return MediaStorageState.Unknown;

            return MediaStorageState.BadLink;
        }

        public static void SetOwnerStorageState(object ownerObject, MediaStorageState storageState)
        {
            if (ownerObject == null)
                return;

            if (ownerObject is MediaRun)
                ((MediaRun)ownerObject).StorageState = storageState;
            else if (ownerObject is MediaDescription)
                ((MediaDescription)ownerObject).StorageState = storageState;
        }

        public static void SetContentStorageState(
            object content, ContentStorageStateCode contentStorageState)
        {
            if (content != null)
            {
                if (content is BaseMarkupContainer)
                {
                    BaseMarkupContainer markupContainer = content as BaseMarkupContainer;
                    markupContainer.ContentStorageState = contentStorageState;
                    BaseObjectNode parentNode = null;

                    if (contentStorageState == ContentStorageStateCode.NotEmpty)
                    {
                        if (markupContainer is BaseObjectContent)
                            parentNode = (markupContainer as BaseObjectContent).Node;
                        else if (markupContainer is BaseObjectNode)
                            parentNode = (markupContainer as BaseObjectNode).Parent;

                        while (parentNode != null)
                        {
                            parentNode.ContentStorageState = ContentStorageStateCode.NotEmpty;
                            parentNode.Tree.ContentStorageState = ContentStorageStateCode.NotEmpty;
                            parentNode = parentNode.Parent;
                        }
                    }
                }
                else if (content is BaseContentStorage)
                {
                    BaseContentStorage contentStorage = content as BaseContentStorage;
                    BaseObjectContent contentObject = contentStorage.Content;
                    contentObject.ContentStorageState = contentStorageState;
                    BaseObjectNode parentNode = null;

                    if (contentStorageState == ContentStorageStateCode.NotEmpty)
                    {
                        parentNode = contentObject.Node;

                        while (parentNode != null)
                        {
                            parentNode.ContentStorageState = ContentStorageStateCode.NotEmpty;
                            parentNode.Tree.ContentStorageState = ContentStorageStateCode.NotEmpty;
                            parentNode = parentNode.Parent;
                        }
                    }
                }
            }
        }

        public static string GetPicturesTildeDirectoryUrl(LanguageID languageID)
        {
            return ApplicationData.ContentTildeUrl + "/Dictionary/Pictures/" + languageID.LanguageCultureExtensionCode;
        }

        public static string GetAudioTildeDirectoryUrl(LanguageID languageID)
        {
            return ApplicationData.ContentTildeUrl + "/Dictionary/Audio/" + languageID.LanguageCultureExtensionCode;
        }

        public static string GetAudioTildeUrl(string dictionaryPath, LanguageID languageID)
        {
            dictionaryPath = dictionaryPath.Replace('\\', '/');
            string returnValue = "";

            if (!dictionaryPath.ToLower().StartsWith("~/content"))
                returnValue += "~/Content";

            if (!dictionaryPath.StartsWith("/"))
                returnValue += "/";

            if (dictionaryPath.ToLower().StartsWith("dictionary"))
                returnValue += dictionaryPath;
            else
                returnValue += "Dictionary/" + languageID.LanguageCultureExtensionCode + "/" + dictionaryPath;

            if (dictionaryPath.ToLower().StartsWith("audio"))
                returnValue += dictionaryPath;
            else
                returnValue += "Audio/" + languageID.LanguageCultureExtensionCode + "/" + dictionaryPath;

            if (!MediaUtilities.HasFileExtension(returnValue))
                returnValue = MediaUtilities.ChangeFileExtension(returnValue, ".mp3");

            return returnValue;
        }

        public static string GetDictionaryAudioDirectoryPath(LanguageID languageID)
        {
            string filePath = "Dictionary" + ApplicationData.PlatformPathSeparator + "Audio" + ApplicationData.PlatformPathSeparator + languageID.LanguageCultureExtensionCode;
            return filePath;
        }

        public static string GetDictionaryAudioFilePath(LanguageID languageID, string audioName, string mimeType)
        {
            string fileExtension = MediaUtilities.GetFileExtensionFromMimeType(mimeType);
            string filePath = "Dictionary" + ApplicationData.PlatformPathSeparator + "Audio" + ApplicationData.PlatformPathSeparator + languageID.LanguageCultureExtensionCode + ApplicationData.PlatformPathSeparator + audioName + fileExtension;
            return filePath;
        }

        public static string GetDictionaryAudioFilePath(LanguageID languageID, string fileName)
        {
            string filePath = "Dictionary" + ApplicationData.PlatformPathSeparator + "Audio" + ApplicationData.PlatformPathSeparator + languageID.LanguageCultureExtensionCode + ApplicationData.PlatformPathSeparator + fileName;
            return filePath;
        }

        public static string GetMediaTildeUrlFromPlatformPath(string platformPath)
        {
            if (String.IsNullOrEmpty(platformPath))
                return platformPath;

            string returnValue = platformPath.Replace('\\', '/');
            int offset = returnValue.IndexOf("/Content/");

            if (offset < 0)
                return String.Empty;

            returnValue = "~" + returnValue.Substring(offset);

            return returnValue;
        }

        public static string ImageMissingUrl = "~/Content/Images/DefaultPicture.png";

        public static string ImageMissingCheck(string url)
        {
            if (String.IsNullOrEmpty(url))
                return ImageMissingUrl;

            if (url.StartsWith("http://") || url.StartsWith("https://"))
                return url;

            string filePath = ApplicationData.MapToFilePath(url);

            if (!FileSingleton.Exists(filePath))
                return ImageMissingUrl;

            return url;
        }

        public static string RemoveTilde(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (url.StartsWith("~"))
                    return url.Substring(1);
            }
            return url;
        }

        public static string RemoveTildeAndSlash(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (url.StartsWith("~/"))
                    return url.Substring(2);
                else if (url.StartsWith("~"))
                    return url.Substring(1);
                else if (url.StartsWith("/"))
                    return url.Substring(1);
            }
            return url;
        }

        public static string GetFullUrlFromTildeUrl(string tildeUrl, string serverUrl)
        {
            if (String.IsNullOrEmpty(tildeUrl) || !tildeUrl.StartsWith("~"))
                return tildeUrl;

            string basePath = RemoveTildeAndSlash(tildeUrl);
            string url = ConcatenateUrlPath(serverUrl, basePath);

            return url;
        }

        public static bool IsUrl(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                string lower = url.ToLower();

                if (lower.StartsWith("http:") || lower.StartsWith("https:") || lower.StartsWith("file:"))
                    return true;
            }

            return false;
        }

        public static bool IsFullUrl(string url)
        {
            if (!String.IsNullOrEmpty(url) && url.Contains(":"))
                return true;

            return false;
        }

        public static bool IsPath(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (url.Contains("/") || url.Contains("\\"))
                    return true;
            }

            return false;
        }

        public static bool IsFullFilePath(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return false;
            
            if (filePath.StartsWith("/") || filePath.StartsWith("\\"))
                return true;

            if ((filePath.Length > 2) && (filePath[1] == ':'))
                return true;

            return false;
        }

        public static string GetMediaUrl(string directoryUrl, string fileName)
        {
            string url = fileName;

            if (String.IsNullOrEmpty(url))
                return String.Empty;

            if (IsFullUrl(url))
                return url;

            if (String.IsNullOrEmpty(directoryUrl))
                directoryUrl = "~/";

            if (!directoryUrl.EndsWith("/"))
                directoryUrl += "/";

            url = directoryUrl + url;

            return url;
        }

        public static string GetMediaFilePath(string directoryUrl, string fileName)
        {
            string url = GetMediaUrl(directoryUrl, fileName);
            string path = ApplicationData.MapToFilePath(url);
            return path;
        }

        public static string GetAlternateFilePath(string oldFilePath, string newSuffix)
        {
            string newFilePath;
            if (String.IsNullOrEmpty(oldFilePath))
                return oldFilePath;
            int offset = oldFilePath.LastIndexOf('.');
            if (offset >= 0)
                newFilePath = oldFilePath.Substring(0, offset) + newSuffix;
            else
                newFilePath = oldFilePath + newSuffix;
            return newFilePath;
        }

        public static string GetContentUrl(string contentRelativePath, string serviceEndpointPrefix)
        {
            string filePath = contentRelativePath;

            if (!String.IsNullOrEmpty(filePath) && !filePath.StartsWith("http:") && !filePath.StartsWith("https:") && !filePath.StartsWith("file:"))
            {
                filePath = filePath.Replace('\\', '/');

                if (filePath.StartsWith("~"))
                    filePath = filePath.Substring(1);

                if (ApplicationData.IsMobileVersion)
                {
                    if (filePath.StartsWith("/"))
                        filePath = filePath.Substring(1);
                }
                else
                {
                    if (!filePath.StartsWith("/"))
                        filePath = "/" + filePath;
                }

                /*
                if (!filePath.StartsWith("http:") && !filePath.StartsWith("https:") && !filePath.StartsWith("file:"))
                {
                    if (serviceEndpointPrefix.EndsWith("/"))
                        filePath = serviceEndpointPrefix + filePath;
                    else
                        filePath = serviceEndpointPrefix + "/" + filePath;
                }
                */
            }

            return filePath;
        }

        public static string GetContentUrl(string contentRelativePath)
        {
            string filePath = contentRelativePath;

            if (!String.IsNullOrEmpty(filePath) && !filePath.StartsWith("http:") && !filePath.StartsWith("https:") && !filePath.StartsWith("file:"))
            {
                filePath = filePath.Replace('\\', '/');

                if (filePath.StartsWith("~"))
                    filePath = filePath.Substring(1);

                if (ApplicationData.IsMobileVersion)
                {
                    if (filePath.StartsWith("/"))
                        filePath = filePath.Substring(1);
                }
                else
                {
                    if (!filePath.StartsWith("/"))
                        filePath = "/" + filePath;
                }

                /*
                if (!filePath.StartsWith("http:") && !filePath.StartsWith("https:") && !filePath.StartsWith("file:"))
                {
                    if (serviceEndpointPrefix.EndsWith("/"))
                        filePath = serviceEndpointPrefix + filePath;
                    else
                        filePath = serviceEndpointPrefix + "/" + filePath;
                }
                */
            }

            return filePath;
        }

        public static string StripQuery(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;
            int index = url.IndexOf('?');
            if (index != -1)
                url = url.Substring(0, index);
            return url;
        }

        public static char[] FilePathDelimiters = { '/', '\\' };

        public static List<string> GetFilePathNodes(string path)
        {
            if (String.IsNullOrEmpty(path))
                return new List<string>();
            string[] parts = path.Split(FilePathDelimiters);
            return parts.ToList();
        }

        public static string GetFileNameFromPath(string path)
        {
            string[] parts = path.Split(FilePathDelimiters);
            if (parts.Count() != 0)
                return parts.Last();
            return "";
        }

        public static string GetFileSubPath(string prefix, string filePath)
        {
            if (filePath.StartsWith(prefix))
                return filePath.Substring(prefix.Length);
            return filePath;
        }

        public static string GetFileName(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return String.Empty;
            string fileName = filePath;
            string[] parts = filePath.Split(FilePathDelimiters, StringSplitOptions.RemoveEmptyEntries);
            if ((parts != null) && (parts.Count() != 0))
                fileName = parts[parts.Count() - 1];
            int offset = fileName.IndexOf('?');
            if (offset > 0)
                fileName = fileName.Substring(0, offset);
            return fileName;
        }

        public static string GetBaseFileName(string filePath)
        {
            string fileName = GetFileName(filePath);
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset = fileName.LastIndexOf('.');
            if (offset > 0)
                fileName = fileName.Substring(0, offset);
            return fileName;
        }

        public static string GetFileExtension(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return String.Empty;
            int offset = fileName.LastIndexOf('.');
            if (offset < 0)
                return String.Empty;
            string extension = fileName.Substring(offset);
            offset = extension.IndexOf('?');
            if (offset > 0)
                extension = extension.Substring(0, offset);
            return extension;
        }

        public static string GetFilePath(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return String.Empty;
            int offset1 = filePath.LastIndexOf('\\');
            int offset2 = filePath.LastIndexOf('/');
            int offset;
            if (offset1 > offset2)
                offset = offset1;
            else
                offset = offset2;
            if (offset < 0)
                return String.Empty;
            string path = filePath.Substring(0, offset);
            return path;
        }

        public static string GetFilePathFromUrl(string urlPath)
        {
            if (String.IsNullOrEmpty(urlPath))
                return String.Empty;

            string urlPathTemp = StripHostOrTildeFromUrl(urlPath);
            string filePath;

            if (!urlPathTemp.StartsWith("/"))
                return String.Empty;

            try
            {
                filePath = ApplicationData.MapToFilePath(urlPathTemp);
            }
            catch (Exception)
            {
                filePath = String.Empty;
            }

            return filePath;
        }

        public static string StripHostOrTildeFromUrl(string urlPath)
        {
            if (String.IsNullOrEmpty(urlPath))
                return String.Empty;

            string urlPathLower = urlPath.ToLower();
            string urlPathTemp = urlPath;

            if (urlPathLower.StartsWith("http://"))
            {
                // Strip http://
                urlPathTemp = urlPathTemp.Substring(7);

                int offset = urlPathTemp.IndexOf("/");

                if (offset != -1)
                    // Strip host.
                    urlPathTemp = urlPathTemp.Substring(offset);
            }
            else if (urlPathLower.StartsWith("https://"))
            {
                // Strip https://
                urlPathTemp = urlPathTemp.Substring(8);

                int offset = urlPathTemp.IndexOf("/");

                if (offset != -1)
                    // Strip host.
                    urlPathTemp = urlPathTemp.Substring(offset);
            }
            else if (urlPathLower.StartsWith("file://"))
            {
                // Strip file://
                urlPathTemp = urlPathTemp.Substring(7);

                int offset = urlPathTemp.IndexOf("/");

                if (offset != -1)
                    // Strip host.
                    urlPathTemp = urlPathTemp.Substring(offset);
            }
            else if (urlPathTemp.StartsWith("~"))
                urlPathTemp = urlPathTemp.Substring(1);

            if (!urlPathTemp.StartsWith("/"))
                return String.Empty;


            return urlPathTemp;
        }

        public static TimeSpan GetMediaUrlTimeSpan(string url)
        {
            string filePath = GetFilePathFromUrl(url);

            if (String.IsNullOrEmpty(filePath))
                return TimeSpan.Zero;

            if (!filePath.EndsWith(".mp3"))
                return TimeSpan.Zero;

            if (!FileSingleton.Exists(filePath))
                return TimeSpan.Zero;

            string message;
            WaveAudioBuffer waveData = MediaConvertSingleton.Mp3Decoding(filePath, out message);

            if (waveData != null)
                return waveData.Duration;

            return TimeSpan.Zero;
        }

        public static bool TrimAudioMp3(
            string audioFilePath,
            int threshhold,             // Raw amplitude.
            int leadingSampleCount,
            int trailingSampleCount,
            out string errorMessage)
        {
            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(audioFilePath, out errorMessage);

            errorMessage = null;

            if (audioWavData == null)
            {
                errorMessage = "No audio data given.";
                return false;
            }

            if (!audioWavData.TrimSamples(threshhold, leadingSampleCount, trailingSampleCount))
            {
                errorMessage = "Trimming operation failed.";
                return false;
            }

            if (!MediaConvertSingleton.Mp3Encoding(audioFilePath, audioWavData, out errorMessage))
                return false;

            return true;
        }

        public static bool AdjustAmplitudeAudioMp3(
            string audioFilePath,
            int desiredPeakAmplitude,
            int noGoAmplitudeThreshold,
            out string errorMessage)
        {
            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(audioFilePath, out errorMessage);
            int averageCount = 10;

            errorMessage = null;

            if (audioWavData == null)
            {
                errorMessage = "No audio data given.";
                return false;
            }

            if (!audioWavData.AdjustAmplitude(-1, -1, averageCount, desiredPeakAmplitude, noGoAmplitudeThreshold))
            {
                errorMessage = "Amplitude too low to adjust.";
                return false;
            }

            if (!MediaConvertSingleton.Mp3Encoding(audioFilePath, audioWavData, out errorMessage))
                return false;

            return true;
        }

        public static bool AdjustAmplitudeAndTrimAudioMp3(
            string audioFilePath,
            int desiredPeakAmplitude,
            int silenceThreshold,             // Raw amplitude.
            int leadingSampleCount,
            int trailingSampleCount,
            out string errorMessage)
        {
            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(audioFilePath, out errorMessage);
            int averageCount = 10;

            errorMessage = null;

            if (audioWavData == null)
            {
                errorMessage = "No audio data given.";
                return false;
            }

            if (!audioWavData.AdjustAmplitude(-1, -1, averageCount, desiredPeakAmplitude, silenceThreshold))
            {
                errorMessage = "Amplitude too low to adjust.";
                return false;
            }

            if (!audioWavData.TrimSamples(silenceThreshold, leadingSampleCount, trailingSampleCount))
            {
                errorMessage = "Trimming operation failed.";
                return false;
            }

            if (!MediaConvertSingleton.Mp3Encoding(audioFilePath, audioWavData, out errorMessage))
                return false;

            return true;
        }

        public static bool GetPeakAmplitudeAudioMp3(
            string audioFilePath,
            out int peakAmplitude,
            out string errorMessage)
        {
            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(audioFilePath, out errorMessage);
            int averageCount = 10;

            peakAmplitude = 0;
            errorMessage = null;

            if (audioWavData == null)
            {
                errorMessage = "No audio data given.";
                return false;
            }

            if (!audioWavData.FindPeakAmplitude(-1, -1, averageCount, out peakAmplitude))
            {
                errorMessage = "Amplitude too low to adjust.";
                return false;
            }

            if (!MediaConvertSingleton.Mp3Encoding(audioFilePath, audioWavData, out errorMessage))
                return false;

            return true;
        }

        public static bool ProcessAudioMp3(
            string audioFilePath,
            bool trim,
            int trimThreshold,
            int trimLeadingSampleCount,
            int trimTrailingSampleCount,
            bool squelch,
            int squelchThreshold,
            int squelchMinimumWidth,
            bool amplitude,
            int amplitudeDesiredPeak,
            int amplitudeNoGoThreshold,
            int amplitudeAverageCount,
            bool peak,
            int peakAverageCount,
            out int peakAmplitude,
            out string errorMessage)
        {
            WaveAudioBuffer audioWavData = MediaConvertSingleton.Mp3Decoding(audioFilePath, out errorMessage);
            bool returnValue;

            returnValue = ProcessAudio(
                audioWavData,
                trim,
                trimThreshold,
                trimLeadingSampleCount,
                trimTrailingSampleCount,
                squelch,
                squelchThreshold,
                squelchMinimumWidth,
                amplitude,
                amplitudeDesiredPeak,
                amplitudeNoGoThreshold,
                amplitudeAverageCount,
                peak,
                peakAverageCount,
                out peakAmplitude,
                out errorMessage);

            if (!returnValue)
                return returnValue;

            if (!MediaConvertSingleton.Mp3Encoding(audioFilePath, audioWavData, out errorMessage))
                returnValue = false;

            return returnValue;
        }

        public static bool ProcessAudio(
            WaveAudioBuffer audioWavData,
            bool trim,
            int trimThreshold,
            int trimLeadingSampleCount,
            int trimTrailingSampleCount,
            bool squelch,
            int squelchThreshold,
            int squelchMinimumWidth,
            bool amplitude,
            int amplitudeDesiredPeak,
            int amplitudeNoGoThreshold,
            int amplitudeAverageCount,
            bool peak,
            int peakAverageCount,
            out int peakAmplitude,
            out string errorMessage)
        {
            bool returnValue = true;

            peakAmplitude = -1;
            errorMessage = null;

            if (audioWavData == null)
            {
                errorMessage = "No audio data given.";
                return false;
            }

            if (peak || amplitude || trim || squelch)
            {
                if (!audioWavData.FindPeakAmplitude(-1, -1, peakAverageCount, out peakAmplitude))
                {
                    errorMessage = "Finding peak amplitude failed.";
                    return false;
                }
            }

            if (amplitude)
            {
                if (peakAmplitude > amplitudeNoGoThreshold)
                {
                    if (!audioWavData.AdjustAmplitude(
                        -1,
                        -1,
                        amplitudeAverageCount,
                        amplitudeDesiredPeak,
                        amplitudeNoGoThreshold))
                    {
                        errorMessage = "Amplitude too low to adjust.";
                        return false;
                    }
                }
                else
                {
                    errorMessage = "Amplitude adjustment operation failed because the peak amplitude is below the no-go threshold.";
                    return false;
                }
            }

            if (trim)
            {
                if (peakAmplitude > trimThreshold)
                {
                    if (!audioWavData.TrimSamples(trimThreshold, trimLeadingSampleCount, trimTrailingSampleCount))
                    {
                        errorMessage = "Trimming operation failed.";
                        return false;
                    }
                }
                else
                {
                    errorMessage = "Trimming operation failed because the peak amplitude is below the trim threshold.";
                    return false;
                }
            }

            if (squelch)
            {
                if (peakAmplitude > squelchThreshold)
                {
                    if (!audioWavData.SquelchSamples(
                        -1,
                        -1,
                        squelchThreshold,
                        squelchMinimumWidth))
                    {
                        errorMessage = "Squelch operation failed.";
                        return false;
                    }
                }
                else
                {
                    errorMessage = "Squelch operation failed because the peak amplitude is below the squelch threshold.";
                    return false;
                }
            }

            return returnValue;
        }

        public static bool HasFileExtension(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return false;
            int nameOffset1 = fileName.LastIndexOf('/');
            int nameOffset2 = fileName.LastIndexOf('\\');
            int nameOffset = (nameOffset1 > nameOffset2 ? nameOffset1 : nameOffset2);
            int offset = fileName.IndexOf('.', (nameOffset != -1 ? nameOffset : 0));
            if (offset < 0)
                return false;
            if (offset == fileName.Length - 1)
                return false;
            return true;
        }

        public static string ChangeFileExtension(string fileName, string newExtension)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset = fileName.LastIndexOf('.');
            if (offset >= 0)
                fileName = fileName.Substring(0, offset) + newExtension;
            else
                fileName += newExtension;
            return fileName;
        }

        public static string RemoveFileExtension(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset = fileName.LastIndexOf('.');
            if (offset >= 0)
                fileName = fileName.Substring(0, offset);
            return fileName;
        }

        public static string ChangeFileName(string fileName, string newName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset = fileName.LastIndexOf(ApplicationData.PlatformPathSeparator);
            if (offset >= 0)
                fileName = fileName.Substring(0, offset + 1) + newName;
            else
                fileName = newName;
            return fileName;
        }

        public static string AddFileNameSuffix(string fileName, string suffix)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            if (String.IsNullOrEmpty(suffix))
                return fileName;
            int offset = fileName.LastIndexOf('.');
            if (offset >= 0)
            {
                string ext = fileName.Substring(offset);
                fileName = fileName.Substring(0, offset) + suffix + ext;
            }
            else
                fileName += suffix;
            return fileName;
        }

        public static string RemoveFileNameSuffix(string fileName, string suffix)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            if (String.IsNullOrEmpty(suffix))
                return fileName;
            int offset = fileName.LastIndexOf(suffix);
            if (offset >= 0)
                fileName = fileName.Substring(0, fileName.Length - suffix.Length);
            return fileName;
        }

        public static string NormalizeUrlPath(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;
            int ofs;
            while ((ofs = url.IndexOf("/..")) != -1)
            {
                int i;
                for (i = ofs - 1; i >= 0; i--)
                {
                    if (url[i] == '/')
                        break;
                }
                if ((i >= 0) && (url[i] == '/'))
                {
                    int len = (ofs - i) + 3;
                    url = url.Remove(i, len);
                }
            }
            return url;
        }

        public static string GetFirstNode(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset1 = fileName.IndexOf('/');
            int offset2 = fileName.IndexOf('\\');
            int offset = offset1;
            if ((offset2 != -1) && (offset2 < offset1))
                offset = offset2;
            if (offset < 0)
                return fileName;
            fileName = fileName.Substring(0, offset);
            return fileName;
        }

        public static string RemoveFirstNode(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset1 = fileName.IndexOf('/');
            int offset2 = fileName.IndexOf('\\');
            int offset = offset1;
            if ((offset2 != -1) && (offset2 < offset1))
                offset = offset2;
            if (offset < 0)
                return fileName;
            fileName = fileName.Substring(offset + 1);
            return fileName;
        }

        public static string GetLastNode(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset1 = fileName.LastIndexOf('/');
            int offset2 = fileName.LastIndexOf('\\');
            int offset = offset1;
            if (offset2 > offset1)
                offset = offset2;
            if (offset < 0)
                return fileName;
            fileName = fileName.Substring(offset + 1);
            return fileName;
        }

        public static string RemoveLastNode(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset1 = fileName.LastIndexOf('/');
            int offset2 = fileName.LastIndexOf('\\');
            int offset = offset1;
            if (offset2 > offset1)
                offset = offset2;
            if (offset < 0)
                return fileName;
            fileName = fileName.Substring(0, offset);
            return fileName;
        }

        public static string GetHost(string url)
        {
            if (String.IsNullOrEmpty(url))
                return String.Empty;
            int offset = url.IndexOf("://");
            if (offset < 0)
                return String.Empty;
            offset = url.IndexOf("/", offset + 3);
            if (offset < 0)
                return url;
            string returnValue = url.Substring(0, offset);
            return returnValue;
        }

        public static string GetDomain(string url)
        {
            if (String.IsNullOrEmpty(url))
                return String.Empty;
            int offset = url.IndexOf("://");
            if (offset >= 0)
                url = url.Substring(offset + 3);
            offset = url.IndexOf("/");
            if (offset > 0)
                url = url.Substring(0, offset);
            return url;
        }

        public static string GetDomainRoot(string url)
        {
            url = GetDomain(url);
            if (String.IsNullOrEmpty(url))
                return String.Empty;
            if (url.StartsWith("www."))
                url = url.Substring(4);
            int offset = url.IndexOf(".");
            if (offset > 0)
                url = url.Substring(0, offset);
            return url;
        }

        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file"))
                relativePath = relativePath.Replace("/", ApplicationData.PlatformPathSeparator);

            return relativePath;
        }

        public static String MakeRelativeUrl(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            if (fromPath.StartsWith("~"))
                fromPath = fromPath.Substring(1);

            if (toPath.StartsWith("~"))
                toPath = toPath.Substring(1);

            if (fromPath.StartsWith("/"))
                fromPath = "http://www.jtlanguage.com" + fromPath;

            if (toPath.StartsWith("/"))
                toPath = "http://www.jtlanguage.com" + toPath;

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }

        public static void MakeUniqueFileNames(List<string> mediaFiles)
        {
            if (mediaFiles == null)
                return;

            int count = mediaFiles.Count();
            int index;
            List<string> usedFileNames = new List<string>();

            for (index = 0; index < count; index++)
            {
                string fileName = GetFileName(mediaFiles[index]);

                if (usedFileNames.Contains(fileName))
                {
                    string newFileName = String.Empty;
                    int ordinal = 0;

                    do
                    {
                        ordinal++;
                        newFileName = fileName + "(" + ordinal.ToString() + ")";
                    }
                    while (usedFileNames.Contains(newFileName));

                    mediaFiles[index] = mediaFiles[index].Replace(fileName, newFileName);
                    fileName = newFileName;
                }

                usedFileNames.Add(fileName);
            }
        }

        public static bool GetMediaDirectoryList(
            string directoryPath,
            List<string> extensions,
            out List<string> fileNames)
        {
            fileNames = new List<string>();

            try
            {
                List<string> allFiles = FileSingleton.GetFiles(directoryPath);

                foreach (string file in allFiles)
                {
                    string fileName = GetFileName(file);

                    if (extensions.Contains(GetFileExtension(fileName.ToLower())))
                        fileNames.Add(fileName);
                }

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private static bool DoInfoVersion = true;

        public static long GetMediaDirectorySize(
            string directoryPath,
            List<string> extensions)
        {
            long totalSize = 0;

            if (DoInfoVersion)
                totalSize = FileSingleton.GetDirectorySize(directoryPath, extensions, false);
            else
            {
                if (!FileSingleton.DirectoryExists(directoryPath))
                    return totalSize;

                List<string> allFiles = FileSingleton.GetFiles(directoryPath);

                foreach (string file in allFiles)
                {
                    string fileName = GetFileName(file);

                    if (extensions.Contains(GetFileExtension(fileName.ToLower())))
                    {
                        try
                        {
                            long size = FileSingleton.FileSize(file);

                            if (size > 0L)
                                totalSize += size;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            return totalSize;
        }

        public static long GetMediaFilesSize(List<string> mediaFiles)
        {
            long size = 0L;

            foreach (string mediaFile in mediaFiles)
            {
                try
                {
                    size += FileSingleton.FileSize(mediaFile);
                }
                catch (Exception)
                {
                }
            }

            return size;
        }

        public static bool DeleteMediaFiles(string mediaDir, List<string> mediaFiles, ref string errorMessage)
        {
            bool returnValue = true;

            // Delete the media files.
            foreach (string filePath in mediaFiles)
            {
                string mimeType = GetMimeTypeFromFileName(filePath);

                try
                {
                    MediaConvertSingleton.DeleteAlternates(filePath, mimeType);

                    if (FileSingleton.Exists(filePath))
                        FileSingleton.Delete(filePath);
                }
                catch (Exception exc)
                {
                    errorMessage += "Exception during media file delete: " + exc.Message;

                    if (exc.InnerException != null)
                        errorMessage = errorMessage + ": " + exc.InnerException.Message;

                    errorMessage += "\n";

                    returnValue = false;
                }
            }

            return returnValue;
        }

        public static bool CreateMediaZipFile(bool flat, bool alternateMediaFiles, string mediaDir,
            List<string> mediaFiles, Stream outputStream, ref string errorMessage)
        {
            string mediaDirSlashLower = mediaDir.ToLower() + ApplicationData.PlatformPathSeparator;
            IArchiveFile zipFile = FileSingleton.Archive();
            bool returnValue = true;

            /*
            if (mediaDirSlashLower.Contains("jtlanguageweb"))
            {
                int offset = mediaDirSlashLower.IndexOf("jtlanguageweb");
                mediaDirSlashLower = mediaDirSlashLower.Substring(0, offset + 14);
            }
            */

            if (flat)
                MediaUtilities.MakeUniqueFileNames(mediaFiles);

            try
            {
                if (zipFile.Create())
                {
                    // Add the media files.
                    foreach (string filePath in mediaFiles)
                    {
                        string relativeFilePath = filePath;

                        if (!alternateMediaFiles && (filePath.Contains("-aa.") || filePath.Contains("-av.")))
                            continue;

                        if (flat)
                            relativeFilePath = "";
                        else if (relativeFilePath.ToLower().StartsWith(mediaDirSlashLower))
                        {
                            relativeFilePath = relativeFilePath.Substring(mediaDirSlashLower.Length);
                            relativeFilePath = MediaUtilities.GetFilePath(relativeFilePath);
                        }

                        if (FileSingleton.Exists(filePath))
                            zipFile.AddFile(filePath, relativeFilePath);
                    }

                    // Save the zip file.
                    zipFile.Save(outputStream);
                }
            }
            catch (Exception exc)
            {
                errorMessage = "Exception during content zip create: " + exc.Message;

                if (exc.InnerException != null)
                    errorMessage = errorMessage + ": " + exc.InnerException.Message;

                returnValue = false;
            }
            finally
            {
                if (zipFile != null)
                    zipFile.Close();
            }

            return returnValue;
        }

        public static void GetDefaultMediaZipName(BaseObjectTitled titledObject, LanguageID uiLanguageID,
            string label, ref string file)
        {
            if (String.IsNullOrEmpty(file))
            {
                file = titledObject.GetTitleString(uiLanguageID);
                if (String.IsNullOrEmpty(file))
                    file = titledObject.Label;
                file = TextUtilities.MakeValidFileBase(file);
                if (!file.Contains(label))
                    file += "_" + label;
                file += "Media";
                file += ".zip";
            }
            if (!MediaUtilities.HasFileExtension(file))
                file += ".zip";
        }

        public static string ComposeStudyItemFileName(string itemKey, int sentenceIndex,
            LanguageID languageID, string mediaRunKey, string fileExtension)
        {
            if (sentenceIndex == -1)
                sentenceIndex = 0;

            string languageSuffix = String.Empty;

            if (languageID != null)
                languageSuffix += "_" + FileFriendlyName(LanguageLookup.GetMediaLanguageID(languageID).LanguageCultureExtensionCode);

            if (mediaRunKey == "SlowAudio")
                languageSuffix += "_Slow";

            string fileName = itemKey + "_" + sentenceIndex.ToString() + languageSuffix + fileExtension;

            return fileName;
        }

        public static string ComposeLanguageFilePath(string path, string baseName, LanguageID languageID, string fileExtension, string separator)
        {
            string filePath = String.Empty;

            if (!String.IsNullOrEmpty(path))
            {
                filePath = path;

                if (!filePath.EndsWith(separator))
                    filePath += separator;
            }

            if (!String.IsNullOrEmpty(baseName))
                filePath += baseName;

            if (languageID != null)
                filePath += "_" + LanguageLookup.GetMediaLanguageID(languageID).LanguageCultureExtensionCode;

            if (!String.IsNullOrEmpty(fileExtension))
                filePath += fileExtension;

            return filePath;
        }

        public static string ComposeFilePath(string path, string baseName, string fileExtension, string separator)
        {
            string filePath = String.Empty;

            if (!String.IsNullOrEmpty(path))
            {
                filePath = path;

                if (!filePath.EndsWith(separator))
                    filePath += separator;
            }

            if (!String.IsNullOrEmpty(baseName))
                filePath += baseName;

            if (!String.IsNullOrEmpty(fileExtension))
                filePath += fileExtension;

            return filePath;
        }

        public static string GetPathSeparator(string path)
        {
            if (String.IsNullOrEmpty(path))
                return ApplicationData.PlatformPathSeparator;
            if (path.StartsWith("http:") || path.StartsWith("https:") || path.StartsWith("file:"))
                return "/";
            if (path.Contains("/"))
                return "/";
            return ApplicationData.PlatformPathSeparator;
        }

        public static bool IsSupportedMediaMimeType(string mimeType)
        {
            if (SupportedMediaMimeTypes.Contains(mimeType))
                return true;
            return false;
        }

        public static bool IsSupportedAudioMimeType(string mimeType)
        {
            if (SupportedAudioMimeTypes.Contains(mimeType))
                return true;
            return false;
        }

        public static bool IsSupportedVideoMimeType(string mimeType)
        {
            if (SupportedVideoMimeTypes.Contains(mimeType))
                return true;
            return false;
        }

        public static bool IsSupportedPictureMimeType(string mimeType)
        {
            if (SupportedPictureMimeTypes.Contains(mimeType))
                return true;
            return false;
        }

        public static string GetMimeTypeFromFileName(string fileName)
        {
            int count = MimeExtensions.Count();
            int index;
            string mimeType = null;
            string extension = GetFileExtension(fileName);
            for (index = 0; index < count; index += 2)
            {
                if (MimeExtensions[index + 1] == extension)
                {
                    mimeType = MimeExtensions[index];
                    break;
                }
            }
            return mimeType;
        }

        public static string GetFileTypeFromFileName(string fileName)
        {
            int count = FileTypeExtensions.Count();
            int index;
            string mediaType = null;
            string extension = GetFileExtension(fileName);
            for (index = 0; index < count; index += 2)
            {
                if (FileTypeExtensions[index + 1] == extension)
                {
                    mediaType = FileTypeExtensions[index];
                    break;
                }
            }
            return mediaType;
        }

        public static string GetMimeTypeFromMediaTypeAndFileName(string mediaType, string fileName)
        {
            string fileExtension = GetFileExtension(fileName);
            string mimeType = GetMimeTypeFromMediaTypeAndFileExtension(mediaType, fileExtension);
            return mimeType;
        }

        public static string GetMimeTypeFromMediaTypeAndFileExtension(string mediaType, string fileExtension)
        {
            int count = MimeExtensions.Count();
            int index;
            string mimeType = null;
            string type = mediaType.ToLower();
            for (index = 0; index < count; index += 2)
            {
                if (MimeExtensions[index + 1] == fileExtension)
                {
                    mimeType = MimeExtensions[index];
                    if (mimeType.StartsWith(type))
                        break;
                    else
                        mimeType = null;
                }
            }
            return mimeType;
        }

        public static string GetMimeTypeFromContent(Stream contentStream)
        {
            if (contentStream == null)
                return String.Empty;

            byte[] buffer = new byte[4];

            if (contentStream.CanSeek)
                contentStream.Seek(0, SeekOrigin.Begin);

            if (contentStream.Read(buffer, 0, 4) == 4)
                return GetMimeTypeFromContent(buffer);

            return String.Empty;
        }

        public static string GetMimeTypeFromContent(byte[] contentData)
        {
            if (contentData == null)
                return null;
            else if (contentData.Count() < 4)
                return null;

            string header = TextUtilities.GetStringFromBytes(contentData, 0, 4);

            switch (header)
            {
                case "RIFF":
                    return "audio/wav";
                case "OGGS":
                    return "audio/speex";
                default:
                    break;
            }

            header = TextUtilities.GetStringFromBytes(contentData, 0, 3);

            if (header == "ID3")
                return "audio/mpeg3";

            for (int index = 0; index < 256; index++)
            {
                if (contentData[index] == 0xff)
                {
                    if ((contentData[index + 1] & 0xe0) == 0xe0)
                        return "audio/mpeg3";
                }
            }

            return null;
        }

        public static string GetFileExtensionFromMimeType(string mimeType)
        {
            int count = MimeExtensions.Count();
            int index;
            string extension = null;
            for (index = 0; index < count; index += 2)
            {
                if (MimeExtensions[index] == mimeType)
                {
                    extension = MimeExtensions[index + 1];
                    break;
                }
            }
            return extension;
        }

        public static string GetMediaTypeStringFromCode(MediaTypeCode mediaType)
        {
            return MediaTypes[(int)mediaType];
        }

        public static MediaTypeCode GetMediaTypeFromString(string mediaTypeString)
        {
            MediaTypeCode code;

            switch (mediaTypeString)
            {
                case "Unknown":
                    code = MediaTypeCode.Unknown;
                    break;
                case "Audio":
                    code = MediaTypeCode.Audio;
                    break;
                case "Video":
                    code = MediaTypeCode.Video;
                    break;
                case "Image":
                    code = MediaTypeCode.Image;
                    break;
                case "TextFile":
                    code = MediaTypeCode.TextFile;
                    break;
                case "PDF":
                    code = MediaTypeCode.PDF;
                    break;
                case "Embedded":
                    code = MediaTypeCode.Embedded;
                    break;
                case "MultiPart":
                    code = MediaTypeCode.MultiPart;
                    break;
                default:
                    //throw new ObjectException("MediaDescription.GetMediaTypeFromString:  Unknown media type:  " + mediaTypeString);
                    code = MediaTypeCode.Unknown;
                    break;
            }

            return code;
        }

        public static MediaTypeCode GetMediaTypeFromFileName(string mediaFileName)
        {
            MediaTypeCode code;
            string fileName = mediaFileName.ToLower();
            string fileExtension = GetFileExtension(fileName);

            switch (fileExtension)
            {
                case ".wmv":
                case ".mp4":
                case ".webm":
                    code = MediaTypeCode.Video;
                    break;
                case ".mp3":
                case ".ogg":
                case ".spx":
                case ".wav":
                    code = MediaTypeCode.Audio;
                    break;
                case ".jpg":
                case ".png":
                case ".gif":
                    code = MediaTypeCode.Image;
                    break;
                case ".txt":
                    code = MediaTypeCode.TextFile;
                    break;
                case ".pdf":
                    code = MediaTypeCode.PDF;
                    break;
                case "":
                    code = MediaTypeCode.MultiPart;
                    break;
                default:
                    code = MediaTypeCode.Unknown;
                    break;
            }

            return code;
        }

        public static string GetDictionaryMediaClassFromFileName(string mediaFileName)
        {
            MediaTypeCode code = GetMediaTypeFromFileName(mediaFileName);
            string className;

            switch (code)
            {
                case MediaTypeCode.Video:
                    className = "Video";
                    break;
                case MediaTypeCode.Audio:
                    className = "Audio";
                    break;
                case MediaTypeCode.Image:
                    className = "Pictures";
                    break;
                case MediaTypeCode.TextFile:
                    className = "Text";
                    break;
                case MediaTypeCode.PDF:
                    className = "PDF";
                    break;
                case MediaTypeCode.MultiPart:
                    className = "Data";
                    break;
                case MediaTypeCode.Unknown:
                default:
                    className = "Unknown";
                    break;
            }

            return className;
        }

        public static MediaTypeCode GetMediaTypeFromMimeType(string mimeType)
        {
            MediaTypeCode code;

            if (mimeType.StartsWith("audio/"))
                code = MediaTypeCode.Audio;
            else if (mimeType.StartsWith("video/"))
                code = MediaTypeCode.Video;
            else if (mimeType.StartsWith("image/"))
                code = MediaTypeCode.Image;
            else if (mimeType.StartsWith("text/"))
                code = MediaTypeCode.TextFile;
            else if (mimeType == "application/pdf")
                code = MediaTypeCode.PDF;
            else if (mimeType == "multipart/form-data")
                code = MediaTypeCode.MultiPart;
            else
                code = MediaTypeCode.Unknown;

            return code;
        }

        public static int CompareMediaTypeCodes(MediaTypeCode other1, MediaTypeCode other2)
        {
            if (other1 == other2)
                return 0;
            else if (other1 > other2)
                return 1;
            return -1;
        }

        public static bool DirectoryDeleteSafe(string path)
        {
            bool returnValue = true;

            try
            {
                if (FileSingleton.DirectoryExists(path))
                    FileSingleton.DeleteDirectory(path);
            }
            catch (Exception)
            {
                returnValue = false;
            }

            return returnValue;
        }

        public static int CompareMediaTypes(MediaDescription object1, MediaDescription object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return CompareMediaTypeCodes(object1.MediaType, object2.MediaType);
        }

        public static bool RenameMediaDescriptions(List<MediaDescription> mediaDescriptions, string mediaDirectoryTildeUrl,
            string baseName, bool renameFiles)
        {
            bool returnValue = true;

            if (mediaDescriptions == null)
                return returnValue;

            foreach (MediaDescription mediaDescription in mediaDescriptions)
            {
                if (mediaDescription.BaseFileName != baseName)
                {
                    string filePathOld = mediaDescription.GetDirectoryPath(mediaDirectoryTildeUrl);
                    mediaDescription.BaseFileName = baseName;
                    if (renameFiles)
                    {
                        string filePathNew = mediaDescription.GetDirectoryPath(mediaDirectoryTildeUrl);
                        try
                        {
                            bool fileUploadedOld = FileSingleton.Exists(filePathOld);
                            bool fileUploadedNew = FileSingleton.Exists(filePathNew);
                            if (fileUploadedOld && !fileUploadedNew)
                                FileSingleton.Move(filePathOld, filePathNew);
                            MediaConvertSingleton.RenameAlternates(filePathOld, filePathNew, mediaDescription.MimeType);
                        }
                        catch (Exception)
                        {
                            returnValue = false;
                        }
                    }
                }
            }

            return returnValue;
        }

        public static string DictionaryLanguageDirectoryPrefix(LanguageID languageID)
        {
            string path = "Dictionary\\";

            if (languageID != null)
                path += FileFriendlyName(languageID.LanguageName(LanguageLookup.English));
            else
                path += "any";

            return path;
        }

        /*
        public static string TreeDirectoryPrefix(BaseObjectTitled treeOrReference)
        {
            string treeTitle = String.Empty;

            if (treeOrReference != null)
                treeTitle = FileFriendlyName(treeOrReference.GetTitleStringFirst());

            return treeTitle;
        }

        public static string TreeMediaDirectory(BaseObjectTitled treeOrReference)
        {
            string owner = treeOrReference.Owner;
            string treeTitle = TreeDirectoryPrefix(treeOrReference);
            string filePath = "Content\\Media\\" + FileFriendlyName(owner) + "\\" + treeTitle;
            return filePath;
        }

        public static string TreeMediaTildeUrl(BaseObjectTitled treeOrReference)
        {
            return "~/" + TreeMediaDirectory(treeOrReference).Replace('\\', '/');
        }

        public static string TreeMediaDirectoryPath(BaseObjectTitled treeOrReference)
        {
            if (ApplicationData.MapToFilePath != null)
                return ApplicationData.MapToFilePath(TreeMediaTildeUrl(treeOrReference).Replace('\\', '/'));

            return TreeMediaDirectory(treeOrReference).Replace('\\', '/');
        }

        public static string TreeNodeDirectoryPrefix(BaseObjectTitled treeOrReference, BaseObjectTitled nodeContentOrTool)
        {
            string treeTitle = "";
            string nodeTitle = "";
            string contentTitle = "";
            string prefix = String.Empty;

            if (treeOrReference == null)
            {
                if (nodeContentOrTool != null)
                {
                    if (nodeContentOrTool is BaseObjectNode)
                        treeOrReference = (nodeContentOrTool as BaseObjectNode).Tree;
                    else if (nodeContentOrTool is BaseObjectContent)
                    {
                        if ((nodeContentOrTool as BaseObjectContent).Node != null)
                            treeOrReference = (nodeContentOrTool as BaseObjectContent).Node.Tree;
                    }
                }
            }

            if (treeOrReference == null)
                return prefix;
            else
                treeTitle = FileFriendlyName(treeOrReference.GetTitleStringFirst());

            if (nodeContentOrTool == null)
                prefix = treeTitle;
            else if (nodeContentOrTool is BaseObjectNode)
            {
                nodeTitle = FileFriendlyName(nodeContentOrTool.GetTitleStringFirst());
                prefix = treeTitle + "\\" + nodeTitle;
            }
            else if (nodeContentOrTool is BaseObjectContent)
            {
                BaseObjectContent content = nodeContentOrTool as BaseObjectContent;
                BaseObjectNode node = content.Node;

                if (node != null)
                {
                    nodeTitle = FileFriendlyName(nodeContentOrTool.GetTitleStringFirst());
                    prefix = treeTitle + "\\" + nodeTitle;
                }
                else
                    prefix = treeTitle;

                if (content is ContentStudyList)
                {
                    contentTitle = FileFriendlyName(content.ContentType);
                    prefix = prefix + "\\" + contentTitle;
                }
                else if (content is ToolStudyList)
                {
                    contentTitle = FileFriendlyName(content.ContentType);
                    prefix = "Tool\\" + prefix + "\\" + contentTitle;
                }
            }

            return prefix;
        }

        public static string TreeNodeMediaPrefix(BaseObjectTitled treeOrReference, BaseObjectTitled nodeContentOrTool)
        {
            string owner = (nodeContentOrTool != null ? nodeContentOrTool.Owner : treeOrReference.Owner);
            string treeNodeTitle = TreeNodeDirectoryPrefix(treeOrReference, nodeContentOrTool);
            string filePath = "Content\\Media\\" + FileFriendlyName(owner) + "\\" + treeNodeTitle;
            return filePath;
        }

        public static string TreeNodeMediaDirectory(BaseObjectTitled treeOrReference, BaseObjectTitled nodeContentOrTool,
            string contentType, LanguageID languageID)
        {
            string owner = (nodeContentOrTool != null ? nodeContentOrTool.Owner : treeOrReference.Owner);
            string treeNodeTitle = TreeNodeDirectoryPrefix(treeOrReference, nodeContentOrTool);
            string subNodeTitle = (!String.IsNullOrEmpty(contentType) ? "\\" + FileFriendlyName(contentType) : "")
                + (languageID != null ? "-" + languageID.LanguageCultureExtensionCode : "");
            string filePath = "Content\\Media\\" + FileFriendlyName(owner) + "\\" + treeNodeTitle + subNodeTitle;
            return filePath;
        }

        public static string TreeNodeMediaTildeUrl(BaseObjectTitled treeOrReference, BaseObjectTitled nodeContentOrTool,
            string contentType, LanguageID languageID)
        {
            return "~/" + TreeNodeMediaDirectory(treeOrReference, nodeContentOrTool, contentType, languageID).Replace('\\', '/');
        }

        public static string TreeNodeMediaDirectoryPath(BaseObjectTitled treeOrReference, BaseObjectTitled nodeContentOrTool,
            string contentType, LanguageID languageID)
        {
            if (ApplicationData.MapToFilePath != null)
                return ApplicationData.MapToFilePath(TreeNodeMediaTildeUrl(treeOrReference, nodeContentOrTool, contentType, languageID).Replace('\\', '/'));

            return TreeNodeMediaDirectory(treeOrReference, nodeContentOrTool, contentType, languageID).Replace('\\', '/');
        }

        public static string TreeNodeMediaTildePrefixUrl(BaseObjectTitled treeOrReference, BaseObjectTitled nodeContentOrTool)
        {
            return "~/" + TreeNodeMediaPrefix(treeOrReference, nodeContentOrTool).Replace('\\', '/');
        }

        public static string ComposeMediaPath(string owner, string subPath)
        {
            return "~/Content/Media/" + FileFriendlyName(owner) + "/" + subPath.Replace('\\', '/');
        }
        */

        public static string GetFullUrl(string host, string tildeOrFullUrl)
        {
            string url = tildeOrFullUrl;

            if (String.IsNullOrEmpty(tildeOrFullUrl))
                return url;

            if (tildeOrFullUrl.StartsWith("http:") || tildeOrFullUrl.StartsWith("https:"))
                return url;

            if (url.StartsWith("~"))
                url = host + url.Substring(1);
            else if (url.StartsWith("/"))
                url = host + url;
            else
                url = host + "/" + url;

            return url;
        }

        public static string ConcatenateUrlPath(string url, string subPath)
        {
            if (String.IsNullOrEmpty(url))
                return subPath;

            if (String.IsNullOrEmpty(subPath))
                return url;

            if (!url.EndsWith("/") && !subPath.StartsWith("/"))
                return url + "/" + subPath;

            return url + subPath;
        }

        public static string ConcatenateUrlArgument(string url, string argumentName, string value)
        {
            string field = argumentName + "=" + value;

            if (String.IsNullOrEmpty(url))
                return field;

            if (url.EndsWith("?") || url.EndsWith("&"))
                url += field;
            else if (url.Contains("?"))
                url += "&" + field;
            else
                url += "?" + field;

            return url;
        }

        public static string ConcatenateFilePath(string path, string subPath)
        {
            if (String.IsNullOrEmpty(path))
                return subPath;

            if (String.IsNullOrEmpty(subPath))
                return path;

            if (!path.EndsWith(ApplicationData.PlatformPathSeparator) && !subPath.StartsWith(ApplicationData.PlatformPathSeparator))
                return path + ApplicationData.PlatformPathSeparator + subPath;

            return path + subPath;
        }

        public static string ConcatenateFilePath3(string path, string subPath, string subSubPath)
        {
            return
                ConcatenateFilePath(
                    ConcatenateFilePath(
                        path,
                        subPath),
                    subSubPath);
        }

        public static string ConcatenateFilePath4(string path, string subPath, string subSubPath, string subSubSubPath)
        {
            return
                ConcatenateFilePath(
                    ConcatenateFilePath3(
                        path,
                        subPath,
                        subSubPath),
                    subSubSubPath);
        }

        public static string ConcatenateFilePath5(string path, string subPath, string subSubPath, string subSubSubPath, string subSubSubSubPath)
        {
            return
                ConcatenateFilePath(
                    ConcatenateFilePath4(
                        path,
                        subPath,
                        subSubPath,
                        subSubSubPath),
                    subSubSubSubPath);
        }

        public static string FileFriendlyName(string name)
        {
            return FileFriendlyName(name, 255);
        }

        public static string FileFriendlyName(string name, int maxLength)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            name = FileFriendlyNameNoLimit(name);

            if (name.Length > maxLength)
            {
                int index;
                int count = name.Length;
                int lastUpperIndex = -1;

                if (count > maxLength)
                    count = maxLength;

                for (index = 0; index < count; index++)
                {
                    if (char.IsUpper(name[index]))
                        lastUpperIndex = index;
                }

                if (lastUpperIndex > maxLength/3)
                    name = name.Substring(0, lastUpperIndex);
                else
                    name = name.Substring(0, maxLength);
            }

            return name;
        }

        public static string FileFriendlyNameNoLimit(string name)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            foreach (string str in LanguageLookup.SpaceAndPunctuationCharactersList)
            {
                name = name.Replace(str, "");
            }

            name = name.Replace("\\", "");
            name = name.Replace("/", "");
            name = name.Replace("|", "");
            name = name.Replace("<", "");
            name = name.Replace(">", "");
            name = name.Replace("*", "");
            name = name.Replace("...", "");
            name = name.Replace("&#039;", "");

            name = TextUtilities.GetNormalizedString(name);

            return name;
        }

        public static string[] FilterTheseFromSpeech = new string[]
        {
            "~",
            "_",
            "`",
            "-",
            "*",
            "/",
            "$"
        };

        public static string FilterTextBeforeSpeech(
            string text,
            LanguageID languageID,
            UserProfile userProfile,
            bool filterAsides)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            if (userProfile != null)
                text = TextUtilities.SubstitutionCheck(text, languageID, userProfile);

            foreach (string filter in FilterTheseFromSpeech)
                text = text.Replace(filter, "");

            if (filterAsides)
                text = TextUtilities.FilterSpeechAsides(text);

            return text;
        }

        public static string FilterVariableName(string name)
        {
            name = FilterTextBeforeSpeech(name, null, null, false);
            name = FileFriendlyNameNoLimit(name);
            return name;
        }

        public static TimeSpan ConvertSecondsToTimeSpan(double seconds)
        {
            return new TimeSpan((long)(seconds * 10000000));
        }

        public static bool BackupFile(string filePath, out string message)
        {
            bool fileExists = FileSingleton.Exists(filePath);

            message = null;

            if (!fileExists)
                return true;

            string fileExtension = GetFileExtension(filePath);
            string backupFilePath = ChangeFileExtension(filePath, "_backup" + fileExtension);

            try
            {
                FileSingleton.Copy(filePath, backupFilePath, true);
            }
            catch (Exception exc)
            {
                message = "Exception while backup up file: " + exc.Message;
                if (exc.InnerException != null)
                    message += ":\n    " +
                        exc.InnerException.Message;
                return false;
            }

            return true;
        }

        // Sort order:
        //      NotSynthesized
        //      Region
        //      DontUseVotes
        //      UseVotes
        //      SourceVotes
        //      Country
        public class AudioAttributeComparer : IComparer<AudioInstance>
        {
            public int Compare(AudioInstance x, AudioInstance y)
            {
                int isSynthesizedX = (x.SourceName == AudioInstance.SynthesizedSourceName ? 1 : 0);
                int isSynthesizedY = (y.SourceName == AudioInstance.SynthesizedSourceName ? 1 : 0);

                int diff = isSynthesizedX - isSynthesizedY;

                if (diff == 0)
                {
                    string regionX = x.GetAttribute(AudioInstance.Region);
                    string regionY = y.GetAttribute(AudioInstance.Region);

                    diff = String.Compare(regionX, regionY);
                }

                if (diff == 0)
                    diff = x.DontUseVotes - y.DontUseVotes;

                if (diff == 0)
                    diff = x.UseVotes - y.UseVotes;

                if (diff == 0)
                    diff = x.SourceVotes - y.SourceVotes;

                if (diff == 0)
                {
                    string countryX = x.GetAttribute(AudioInstance.Country);
                    string countryY = y.GetAttribute(AudioInstance.Country);
                    diff = String.Compare(countryX, countryY);
                }

                // don't sort on gender.

                return diff;
            }
        }

        private static AudioAttributeComparer AudioAttributeComparerStatic = new AudioAttributeComparer();

        public static bool CheckForAudios(
            string[] texts,
            string languageCode,
            out string[] failedTexts,
            out string errorMessage)
        {
            List<string> failedTextsList = null;
            bool returnValue = true;

            failedTexts = null;
            errorMessage = null;

            if (texts == null)
            {
                errorMessage = "Null audio texts.";
                return false;
            }

            foreach (string text in texts)
            {
                string localErrorMessage;

                if (!CheckForAudio(text, languageCode, out localErrorMessage))
                {
                    if (failedTextsList == null)
                        failedTextsList = new List<string>() { text };
                    else
                        failedTextsList.Add(text);

                    if (!String.IsNullOrEmpty(localErrorMessage))
                    {
                        if (!String.IsNullOrEmpty(errorMessage))
                            errorMessage = errorMessage + "\n" + localErrorMessage;
                        else
                            errorMessage = localErrorMessage;
                    }

                    returnValue = false;
                }
            }

            if (failedTextsList != null)
                failedTexts = failedTextsList.ToArray();

            return returnValue;
        }

        public static bool CheckForAudio(
            string text,
            string languageCode,
            out string errorMessage)
        {
            errorMessage = null;

            if (String.IsNullOrEmpty(text))
            {
                errorMessage = "No audio text.";
                return false;
            }

            string audioKey = text;
            IMainRepository repositories = ApplicationData.Repositories;
            LanguageID preferedMediaLanguageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);

            AudioMultiReference audioReference = ApplicationData.Repositories.DictionaryMultiAudio.Get(
                audioKey,
                preferedMediaLanguageID);

            if ((audioReference == null) || (audioReference.AudioInstanceCount() == 0))
            {
                // If we are an entry with no instances, just delete it and start over.
                if ((audioReference != null) && (audioReference.AudioInstanceCount() == 0))
                    ApplicationData.Repositories.DictionaryMultiAudio.DeleteKey(audioKey, preferedMediaLanguageID);
                else
                    return true;
            }

            return false;
        }

        public static bool PrimeAudios(
            string[] texts,
            string languageCode,
            string source,
            Voice voice,
            UserRecord userRecord,
            UserProfile userProfile,
            IMainRepository repositories,
            LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities,
            out string[] failedTexts,
            out string errorMessage)
        {
            failedTexts = null;
            errorMessage = null;

            if (texts == null)
            {
                errorMessage = "Null audio texts.";
                return false;
            }

            LanguageID preferedMediaLanguageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            bool returnValue = true;

            foreach (string text in texts)
            {
                string localErrorMessage;

                if (!PrimeAudio(
                    text,
                    preferedMediaLanguageID,
                    source,
                    voice,
                    userRecord,
                    userProfile,
                    repositories,
                    languageUtilities,
                    nodeUtilities,
                    out localErrorMessage))
                {
                    if (String.IsNullOrEmpty(errorMessage))
                        errorMessage = localErrorMessage;
                    else if (!String.IsNullOrEmpty(localErrorMessage))
                        errorMessage = errorMessage + "\n" + localErrorMessage;

                    returnValue = false;
                }
            }

            return returnValue;
        }

        public static bool PrimeAudio(
            string text,
            LanguageID languageID,
            string source,
            Voice voice,
            UserRecord userRecord,
            UserProfile userProfile,
            IMainRepository repositories,
            LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities,
            out string errorMessage)
        {
            errorMessage = null;

            if (String.IsNullOrEmpty(text))
            {
                errorMessage = "No audio text.";
                return false;
            }

            string audioKey = text;
            string wordAudioSource = source;
            string owner = userRecord.UserName;
            AudioMultiReference audioRecord = ApplicationData.Repositories.DictionaryMultiAudio.Get(
                audioKey,
                languageID);
            bool returnValue = false;

            if ((audioRecord == null) || (audioRecord.AudioInstanceCount() == 0))
            {
                // If we are an entry with no instances, just delete it and start over.
                if ((audioRecord != null) && (audioRecord.AudioInstanceCount() == 0))
                    ApplicationData.Repositories.DictionaryMultiAudio.DeleteKey(audioKey, languageID);

                if (wordAudioSource == AudioReference.SynthesizedSourceName)
                    returnValue = GetWordAudioSynthesizer(
                        audioKey,
                        languageID,
                        voice,
                        owner,
                        nodeUtilities,
                        ref audioRecord,
                        out errorMessage);
                else if (wordAudioSource == AudioReference.ForvoSourceName)
                    returnValue = GetWordAudioForvo(
                        audioKey,
                        languageID,
                        userRecord,
                        userProfile,
                        repositories,
                        languageUtilities,
                        nodeUtilities,
                        out audioRecord,
                        out errorMessage);
                else if (wordAudioSource == AudioReference.DontCareSourceName)
                {
                    string message1;
                    string message2;
                    returnValue = GetWordAudioForvo(
                        audioKey,
                        languageID,
                        userRecord,
                        userProfile,
                        repositories,
                        languageUtilities,
                        nodeUtilities,
                        out audioRecord,
                        out message1);
                    returnValue = GetWordAudioSynthesizer(
                        audioKey,
                        languageID,
                        voice,
                        owner,
                        nodeUtilities,
                        ref audioRecord,
                        out message2) || returnValue;
                    if (!returnValue)
                    {
                        if (!String.IsNullOrEmpty(message1) && !String.IsNullOrEmpty(message2))
                            errorMessage = message1 + "\n" + message2;
                        else if (!String.IsNullOrEmpty(message1))
                            errorMessage = message1;
                        else
                            errorMessage = message2;
                    }
                }
            }

            return returnValue;
        }

        public static bool GetWordAudioSynthesizer(
            string word,
            LanguageID languageID,
            Voice voice,
            string owner,
            NodeUtilities nodeUtilities,
            ref AudioMultiReference audioRecord,
            out string errorMessage)
        {
            bool isNew = false;
            AudioInstance audioInstance = null;

            errorMessage = null;

            if (audioRecord == null)
            {
                audioRecord = ApplicationData.Repositories.DictionaryMultiAudio.GetAudio(word, languageID);

                if (audioRecord == null)
                {
                    audioRecord = new AudioMultiReference(
                        word,
                        languageID,
                        new List<AudioInstance>());
                    isNew = true;
                }
            }

            audioInstance = audioRecord.GetAudioInstanceBySourceAndAttribute(
                AudioInstance.SynthesizedSourceName,
                AudioInstance.Speaker,
                voice.GetAttribute(AudioInstance.Speaker));

            if (audioInstance != null)
                return true;

            audioInstance = new AudioInstance(
                word,
                owner,
                MediaUtilities.MimeTypeMp3,
                audioRecord.AllocateAudioFileName(),
                AudioInstance.SynthesizedSourceName,
                voice.CloneAttributes());

            string audioFilePath = audioInstance.GetFilePath(languageID);

            bool entryHasAudio = nodeUtilities.AddSynthesizedVoiceDefault(
                word,
                audioFilePath,
                languageID);

            if (entryHasAudio)
                audioRecord.AddAudioInstance(audioInstance);
            else
            {
                errorMessage = "Audio synthesis failed for: " + word;
                audioRecord = null;
                return false;
            }

            audioRecord.TouchAndClearModified();
            audioInstance.TouchAndClearModified();

            if (isNew)
            {
                try
                {
                    if (!ApplicationData.Repositories.DictionaryMultiAudio.Add(audioRecord, languageID))
                        errorMessage = "Error adding audio record for: " + word;
                }
                catch (Exception exc)
                {
                    errorMessage = "Exception while adding audio record: " + exc.Message;

                    if (exc.InnerException != null)
                        errorMessage += errorMessage + ":\r\n" + exc.InnerException.Message;
                }
            }
            else
            {
                try
                {
                    if (!ApplicationData.Repositories.DictionaryMultiAudio.Update(audioRecord, languageID))
                        errorMessage = "Error updating audio record for: " + word;
                }
                catch (Exception exc)
                {
                    errorMessage = "Exception while updating audio record: " + exc.Message;

                    if (exc.InnerException != null)
                        errorMessage += errorMessage + ":\r\n" + exc.InnerException.Message;
                }
            }

            return entryHasAudio;
        }

        public static bool GetWordAudioForvo(
            string word,
            LanguageID languageID,
            UserRecord userRecord,
            UserProfile userProfile,
            IMainRepository repositories,
            LanguageUtilities languageUtilities,
            NodeUtilities nodeUtilities,
            out AudioMultiReference audioRecord,
            out string errorMessage)
        {
            string formatType = "Crawler";

            audioRecord = null;
            errorMessage = null;

            FormatCrawler importFormatObject = (FormatCrawler)ApplicationData.Formats.Create(formatType, "AudioReference", "Audio",
                userRecord, userProfile, repositories, languageUtilities, nodeUtilities);

            if (importFormatObject == null)
            {
                errorMessage = "Failed to create format object.";
                return false;
            }

            importFormatObject.SetUpDumpStringCheck();

            if (importFormatObject.WebCrawler != null)
                importFormatObject.WebCrawler.SetUpDumpStringCheck();

            importFormatObject.WebFormatType = "Forvo Audio";
            importFormatObject.ImportExportType = "Web";

            importFormatObject.InitializeCrawler();

            importFormatObject.Arguments = new List<FormatArgument>();

            string Word = word;
            string WordPrompt = "Single word";
            string WordHelp = "Enter a single word to get audio for.";

            importFormatObject.SetArgument("Word", "string", "rw", Word,
                WordPrompt, WordHelp, null, null);

            importFormatObject.LoadFromArguments();

            Crawler importCrawlerObject = importFormatObject.WebCrawler;

            int progressCount = 0;

            importFormatObject.InitializeProgress("Read", false, progressCount);

            try
            {
                importFormatObject.Read(null);

                if (importFormatObject.ReadObjects != null)
                    audioRecord = importFormatObject.ReadObjects.FirstOrDefault() as AudioMultiReference;
                else
                    return false;
            }
            catch (Exception exc)
            {
                errorMessage = "Exception during import: " + exc.Message;

                if (exc.InnerException != null)
                    errorMessage += errorMessage + ":\r\n" + exc.InnerException.Message;
                return false;
            }

            importFormatObject.FinishProgress("Read", false);

            return true;
        }
    }
}
