using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
#if CORE
using System.Threading.Tasks;
#else
using System.Threading;
using System.Threading.Tasks;
#endif
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Crawlers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Service;

namespace JTLanguageModelsPortable.Application
{
    public enum MediaStorageState
    {
        Unknown,        // We don't know right now.
        Present,        // Media is present.
        Downloaded,     // Media has been downloaded, either on-demand or via a download page.
        Absent,         // Media is not present.
        External,       // The media is an external reference - do not download.
        BadLink         // The url is bad, or there was an network outage.
    };

    public enum ProgressMode
    {
        Start,
        Update,
        Stop,
        Hide,
        Show,
        DelayedShow
    }

    public enum ServiceTypeCode
    {
        LocalService,
        Direct,
        Executable
    }

    public class ApplicationData
    {
        // Settings managed by InitializeApplicationData
        public static string ApplicationName { get; set; }
        public static string BaseRepositoryName { get; set; }
        public static string BaseContentTildeUrl { get; set; }
        public static string Version { get; set; }
        public static string ServiceUrl { get; set; }
        public static string ContentTildeUrl { get; set; }
        public static string CrawlTildeUrl { get; set; }
        public static string DatabaseTildeUrl { get; set; }
        public static string ImagesTildeUrl { get; set; }
        public static string LocalDataTildeUrl { get; set; }
        public static string MediaTildeUrl { get; set; }
        public static string PicturesTildeUrl { get; set; }
        public static string SandboxTildeUrl { get; set; }
        public static string TagsTildeUrl { get; set; }
        public static string TempTildeUrl { get; set; }
        public static string UserDataTildeUrl { get; set; }
        public static string ContentPath { get; set; }
        public static string CrawlPath { get; set; }
        public static string DatabasePath { get; set; }
        public static string ImagesPath { get; set; }
        public static string LocalDataPath { get; set; }
        public static string MediaPath { get; set; }
        public static string PicturesPath { get; set; }
        public static string SandboxPath { get; set; }
        public static string TagsPath { get; set; }
        public static string TempPath { get; set; }
        public static string UserDataPath { get; set; }

        // Settings managed or overridable by classes derived for platform.
        public static bool IsDebugVersion { get; set; }
        public static bool IsDevelopmentVersion { get; set; }
        public static bool IsMobileVersion { get; set; }
        public static bool IsTestMobileVersion { get; set; }
        public static bool IsEditVersion { get; set; }
        public static bool IsAndroid { get; set; }
        public static bool IsIOS { get; set; }
        public static string BasePlatformDirectory { get; set; }
        public static string MediaVersion { get; set; }
        public static UTF8Encoding Encoding { get; set; }
        public static string PlatformPathSeparator { get; set; }
        public static string ActionUrlPrefix { get; set; }
        public static string MasterAdministratorUserName { get; set; }
        public static string RepositoryName { get; set; }
        public static IMainRepository Repositories { get; set; }
        public static List<IMainRepository> UserRepositories { get; set; }
        public static PersistentSettingsStorage Settings { get; set; }
        public static ClientServiceBase RemoteClient { get; set; }
        public static ClientMainRepository RemoteRepositories { get; set; }
        public static ILanguageTranslator Translator { get; set; }
        public static DictionarySources DictionarySources { get; set; }
        public static PersistentStringMapper InflectionLabels { get; set; }
        public static PersistentVoiceList VoiceList { get; set; }
        public static LanguageToolFactory LanguageTools { get; set; }
        public static FormatFactory Formats { get; set; }
        public static CrawlerFactory Crawlers { get; set; }
        public static ServiceTypeCode LocalServiceType = ServiceTypeCode.Direct;
        public static bool UseSpeechToText = false;
        public static bool UseGoogleSpeechToTextAPI = false;
        public static bool UseTextToSpeech = false;
        public static bool UseGoogleTextToSpeechAPI = false;
        public static bool UseOnlyGoogleTextToSpeechAPI = false;
        public static int DefaultAutomatedMarkupTemplateKey = -1;
        public static bool IsSupportsAutoPlay = false;
        public static string ContactCaptchaSiteKey = "";
        public static string ContactCaptchaSecretKey = "";
        public static bool IsDontInhibitEnglishTranslations = false;
        // Limits the Resources->Translate amount per transaction.
        public static int PerUserTranslateCharCountLimit = 10000;
        // Limits the number of Resources->Translate transactions per period.
        public static int PerUserTranslateTransactionLimit = 100;
        // Resources->Translate transaction period in days.
        public static int PerUserTranslateTransactionPeriod = 1;
        // Limits the content translation amount per transaction.
        public static int PerUserContentTranslateCharCountLimit = 10000;
        // Limits the number of content translation transactions per period.
        public static int PerUserContentTranslateTransactionLimit = 100;
        // Content translation transaction period in days.
        public static int PerUserContentTranslateTransactionPeriod = 1;

        // These flags control how content in the mobile version is referenced.

        // Copy media content when it is referenced, saving it locally for future use.
        public static bool CopyRemoteMediaOnDemand { get; set; }
        // Limit copied media to the size specified by DownloadedMediaSizeLimit.
        public static bool LimitRemoteMedia { get; set; }
        // Limit copied media to the to this size threshhold.
        public static long DownloadedMediaSizeLimit { get; set; }
        // If the downloaded media size limit is exceeded, delete local copies of older media.
        public static bool AutoDeleteOlderData { get; set; }
        // Timeout for media transfers.
        public static int TransferTimeoutMsec { get; set; }
        // Retry limit for media transfers.
        public static int TransferRetryLimit { get; set; }

        // Cached current total media size.
        public static long CurrentTotalMediaSize { get; set; }
        // Include media files in tree install package.
        public static bool IncludeMediaFilesInPackage { get; set; }
        // Cached loaded remote files.
        public static List<string> TemporaryCachedRemoteFiles { get; set; }
        // Cached bookkept files.
        public static List<string> TemporaryCachedBookkeptFiles { get; set; }
        // Cached loaded remote files size.
        public static long TemporaryCachedRemoteFilesSize { get; set; }

        // Global pointer to the ApplicationData instance.
        public static ApplicationData Global;
        public static string UpdateErrorMessage;

        // Delagate definitions.
        public delegate string ToLowerDelegate(string input, CultureInfo cultureInfo);
        public delegate string MapToFilePathDelegate(string tildePath); // Set before
        public delegate CultureInfo CreateCultureDelegate(string languageCode);
        public delegate LanguageID GetCurrentUILanguageIDDelegate();
        public delegate void DumpString(string text);

        // Delegate pointers managed by derived classes.
        // These must be set before InitializeApplicationData is called. 
        // ClearApplicationData and InitializeApplicationData must not change these!
        public static ToLowerDelegate ToLowerFunction;
        public static MapToFilePathDelegate MapToFilePath;
        public static CreateCultureDelegate CreateCulture;
        public static GetCurrentUILanguageIDDelegate GetCurrentUILanguageID;

        // Local instance data.
        protected List<BaseString> _UserOptions;

        public ApplicationData()
        {
            // Don't clear the members.  Let InitializeApplicationData make sure everything is
            // initialized, both here and in the derived classes.
        }

        // Only for the case where there is no derived class used.
        public ApplicationData(
            string applicationName,
            string baseRepositoryName,
            string version,
            string masterAdministratorUserName,
            bool isDevelopmentVersion,
            string serviceUrl,
            string basePlatformDirectory,
            string contentTildeUrl,
            string mediaVersion)
        {
            InitializeApplicationData(
                applicationName,
                baseRepositoryName,
                version,
                masterAdministratorUserName,
                isDevelopmentVersion,
                serviceUrl,
                basePlatformDirectory,
                contentTildeUrl,
                mediaVersion);
        }

        public void ClearApplicationData()
        {
            ApplicationName = String.Empty;
            BaseRepositoryName = String.Empty;
            BaseContentTildeUrl = String.Empty;
            Version = String.Empty;
            ServiceUrl = null;
            ContentTildeUrl = String.Empty;
            CrawlTildeUrl = String.Empty;
            DatabaseTildeUrl = String.Empty;
            ImagesTildeUrl = String.Empty;
            MediaTildeUrl = String.Empty;
            LocalDataTildeUrl = String.Empty;
            PicturesTildeUrl = String.Empty;
            SandboxTildeUrl = String.Empty;
            TagsTildeUrl = String.Empty;
            TempTildeUrl = String.Empty;
            UserDataTildeUrl = String.Empty;
            ContentPath = String.Empty;
            CrawlPath = String.Empty;
            DatabasePath = String.Empty;
            ImagesPath = String.Empty;
            MediaPath = String.Empty;
            LocalDataPath = String.Empty;
            PicturesPath = String.Empty;
            SandboxPath = String.Empty;
            TagsPath = String.Empty;
            TempPath = String.Empty;
            UserDataPath = String.Empty;

            Global = null;

#if DEBUG
            IsDebugVersion = true;
#else
            IsDebugVersion = false;
#endif

            IsDevelopmentVersion = false;
            IsMobileVersion = false;
            IsTestMobileVersion = false;
            IsEditVersion = false;
            IsAndroid = false;
            IsIOS = false;
            BasePlatformDirectory = null;
            Encoding = null;
            PlatformPathSeparator = String.Empty;
            MasterAdministratorUserName = String.Empty;

            CopyRemoteMediaOnDemand = false;
            LimitRemoteMedia = false;
            DownloadedMediaSizeLimit = 0L;
            AutoDeleteOlderData = false;
            TransferTimeoutMsec = 30000;
            TransferRetryLimit = 3;
            CurrentTotalMediaSize = 0;
            IncludeMediaFilesInPackage = true;

            RepositoryName = String.Empty;
            Repositories = null;
            UserRepositories = new List<IMainRepository>();
            Settings = null;
            RemoteClient = null;
            RemoteRepositories = null;
            Translator = null;
            DictionarySources = null;
            InflectionLabels = null;
            VoiceList = null;
            LanguageTools = null;
            Formats = null;
            Crawlers = null;
        }

        public virtual void InitializeApplicationData(
                string applicationName,
                string baseRepositoryName,
                string version,
                string masterAdministratorUserName,
                bool isDevelopmentVersion,
                string serviceUrl,
                string basePlatformDirectory,
                string contentTildeUrl,
                string mediaVersion)
        {
            InitializeApplicationDataStatic(
                applicationName,
                baseRepositoryName,
                version,
                masterAdministratorUserName,
                isDevelopmentVersion,
                serviceUrl,
                basePlatformDirectory,
                contentTildeUrl,
                mediaVersion);

            Global = this;
            _UserOptions = new List<BaseString>();
        }

        public static void InitializeApplicationDataStatic(
                string applicationName,
                string baseRepositoryName,
                string version,
                string masterAdministratorUserName,
                bool isDevelopmentVersion,
                string serviceUrl,
                string basePlatformDirectory,
                string contentTildeUrl,
                string mediaVersion)
        {
            ApplicationName = applicationName;
            BaseRepositoryName = baseRepositoryName;
            BaseContentTildeUrl = contentTildeUrl;
            Version = version;
            ServiceUrl = serviceUrl;
            SwitchContentDirectories(contentTildeUrl, basePlatformDirectory);

            // These still can be optionally overridden by the platform.

#if DEBUG
            IsDebugVersion = true;
#else
            IsDebugVersion = false;
#endif

            IsDevelopmentVersion = isDevelopmentVersion;
            IsMobileVersion = false;
            IsTestMobileVersion = false;
            IsEditVersion = true;
            BasePlatformDirectory = basePlatformDirectory;
            MediaVersion = mediaVersion;
            Encoding = new UTF8Encoding(true, true);
            PlatformPathSeparator = @"\";
            ActionUrlPrefix = "/";
            MasterAdministratorUserName = masterAdministratorUserName;
            LanguageTools = new Language.LanguageToolFactory();
            LanguageTools.AddDefaults();

            // These must be set by the client.
            RepositoryName = String.Empty;
            Repositories = null;
            UserRepositories = new List<IMainRepository>();
            Settings = null;
            RemoteClient = null;
            RemoteRepositories = null;
            Translator = null;
            Formats = null;
            Crawlers = null;
            VoiceList = null;

            // Media option defaults.
            CopyRemoteMediaOnDemand = false;
            LimitRemoteMedia = false;
            DownloadedMediaSizeLimit = 0L;
            AutoDeleteOlderData = false;
            TransferTimeoutMsec = 30000;
            TransferRetryLimit = 3;
            CurrentTotalMediaSize = 0;
            IncludeMediaFilesInPackage = true;
        }

        // Call before InitializeApplicationData is called, or set MapToFilePath directly.
        public static void SetUpMapToFile(string basePlatformDirectory)
        {
            BasePlatformDirectory = basePlatformDirectory;
            MapToFilePath = MapToFilePathGeneric;
        }

        public static string MapToFilePathGeneric(string url)
        {
            string filePath = url;

            if (String.IsNullOrEmpty(filePath))
                return filePath;

            if (filePath.StartsWith("~"))
                filePath = filePath.Substring(1);

            if (filePath.StartsWith("/"))
                filePath = filePath.Substring(1);

            filePath = BasePlatformDirectory + "/" + filePath;

            return filePath;
        }

        public static void LoadMediaOptions()
        {
            ServiceUrl = Settings.GetString("ServiceUrl", ServiceUrl);
            CopyRemoteMediaOnDemand = Settings.GetFlag("CopyRemoteMediaOnDemand", CopyRemoteMediaOnDemand);
            LimitRemoteMedia = Settings.GetFlag("LimitRemoteMedia", LimitRemoteMedia);
            DownloadedMediaSizeLimit = Settings.GetLong("DownloadedMediaSizeLimit", DownloadedMediaSizeLimit);
            AutoDeleteOlderData = Settings.GetFlag("AutoDeleteOlderData", AutoDeleteOlderData);
            TransferTimeoutMsec = Settings.GetInteger("TransferTimeoutMsec", TransferTimeoutMsec);
            TransferRetryLimit = Settings.GetInteger("TransferRetryLimit", TransferRetryLimit);
            IncludeMediaFilesInPackage = Settings.GetFlag("IncludeMediaFilesInPackage", IncludeMediaFilesInPackage);
            CurrentTotalMediaSize = Settings.GetLong("CurrentTotalMediaSize", 0L);

        }

        public static void SaveMediaOptions()
        {
            Settings.SetString("ServiceUrl", ServiceUrl);
            Settings.SetFlag("CopyRemoteMediaOnDemand", CopyRemoteMediaOnDemand);
            Settings.SetFlag("LimitRemoteMedia", LimitRemoteMedia);
            Settings.SetLong("DownloadedMediaSizeLimit", DownloadedMediaSizeLimit);
            Settings.SetFlag("AutoDeleteOlderData", AutoDeleteOlderData);
            Settings.SetInteger("TransferTimeoutMsec", TransferTimeoutMsec);
            Settings.SetInteger("TransferRetryLimit", TransferRetryLimit);
            Settings.SetFlag("IncludeMediaFilesInPackage", IncludeMediaFilesInPackage);
        }

        public static void LoadPersistentOptions()
        {
            IsDontInhibitEnglishTranslations = Settings.GetFlag("IsDontInhibitEnglishTranslations", IsDontInhibitEnglishTranslations);
        }

        public static void SavePersistentOptions()
        {
            Settings.SetFlag("IsDontInhibitEnglishTranslations", IsDontInhibitEnglishTranslations);
        }

        public virtual IMainRepository LoadUserRepository(string repositoryName)
        {
            List<string> userRepositoryDefinitions = Settings.GetStringList("UserRepositoryDefinitions");
            int userRepositoryCount = userRepositoryDefinitions.Count() / 2;
            int userRepositoryIndex;

            for (userRepositoryIndex = 0; userRepositoryIndex < userRepositoryCount; userRepositoryIndex++)
            {
                int baseIndex = userRepositoryIndex * 2;
                string theRepositoryName = userRepositoryDefinitions[baseIndex];

                if (theRepositoryName == repositoryName)
                {
                    IMainRepository repository = GetUserRepository(repositoryName);

                    if (repository == null)
                    {
                        string databasePath = userRepositoryDefinitions[baseIndex + 1];
                        string contentPath = userRepositoryDefinitions[baseIndex + 1].Replace(
                            PlatformPathSeparator + "Database", "");

                        repository = CreateUserRepository(repositoryName, databasePath, contentPath);

                        if (repository != null)
                        {
                            AddUserRepository(repository);
                            return repository;
                        }
                    }
                }
            }

            return null;
        }

        public virtual void LoadUserRepositories()
        {
            List<string> userRepositoryDefinitions = Settings.GetStringList("UserRepositoryDefinitions");
            int userRepositoryCount = userRepositoryDefinitions.Count() / 2;
            int userRepositoryIndex;

            for (userRepositoryIndex = 0; userRepositoryIndex < userRepositoryCount; userRepositoryIndex++)
            {
                int baseIndex = userRepositoryIndex * 2;
                string repositoryName = userRepositoryDefinitions[baseIndex];
                string databasePath = userRepositoryDefinitions[baseIndex + 1];
                string contentPath = userRepositoryDefinitions[baseIndex + 1].Replace(
                    PlatformPathSeparator + "Database", "");

                IMainRepository repository = GetUserRepository(repositoryName);

                if (repository == null)
                {
                    repository = CreateUserRepository(repositoryName, databasePath, contentPath);

                    if (repository != null)
                        AddUserRepository(repository);
                }
            }
        }

        public virtual void ResetUserRepositories()
        {
            List<string> userRepositoryDefinitions = Settings.GetStringList("UserRepositoryDefinitions");
            int repositoryCount = UserRepositories.Count();
            int repositoryIndex;

            for (repositoryIndex = repositoryCount - 1; repositoryIndex >= 0; repositoryIndex--)
            {
                IMainRepository repository = UserRepositories[repositoryIndex];
                string repositoryName = repository.KeyString;

                if (repositoryName == BaseRepositoryName)
                    continue;

                if (!userRepositoryDefinitions.Contains(repositoryName))
                {
                    if (RepositoryName == repositoryName)
                        SwitchMainRepository();

                    DeleteUserRepository(repositoryName);
                }
            }

            int userRepositoryCount = userRepositoryDefinitions.Count() / 2;
            int userRepositoryIndex;

            for (userRepositoryIndex = 0; userRepositoryIndex < userRepositoryCount; userRepositoryIndex++)
            {
                int baseIndex = userRepositoryIndex * 2;
                string repositoryName = userRepositoryDefinitions[baseIndex];
                string databasePath = userRepositoryDefinitions[baseIndex + 1];
                string contentPath = userRepositoryDefinitions[baseIndex + 1].Replace(
                    PlatformPathSeparator + "Database", "");

                IMainRepository repository = GetUserRepository(repositoryName);

                if (repository == null)
                {
                    repository = CreateUserRepository(repositoryName, databasePath, contentPath);

                    if (repository != null)
                        AddUserRepository(repository);
                }
            }
        }

        public static List<string> GetUserRepositoryNames()
        {
            List<string> userRepositoryDefinitions = Settings.GetStringList("UserRepositoryDefinitions");
            List<string> userRepositoryNames = new List<string>() { BaseRepositoryName };
            int userRepositoryCount = userRepositoryDefinitions.Count() / 3;
            int userRepositoryIndex;

            for (userRepositoryIndex = 0; userRepositoryIndex < userRepositoryCount; userRepositoryIndex++)
            {
                string repositoryName = userRepositoryDefinitions[userRepositoryIndex * 3];
                userRepositoryNames.Add(repositoryName);
            }

            return userRepositoryNames;
        }

        public virtual IMainRepository CreateUserRepository(
            string repositoryName,
            string databasePath,
            string contentPath)
        {
            return null;
        }

        public virtual void AddUserRepository(IMainRepository repository)
        {
            IMainRepository testRepository = GetUserRepository(repository.KeyString);

            if (testRepository != null)
                UserRepositories.Remove(testRepository);

            UserRepositories.Add(repository);
        }

        public virtual void DeleteUserRepository(string repositoryName)
        {
            IMainRepository testRepository = GetUserRepository(repositoryName);

            if (testRepository != null)
                UserRepositories.Remove(testRepository);
        }

        public static IMainRepository GetUserRepository(string repositoryName)
        {
            IMainRepository repositories = UserRepositories.FirstOrDefault(x => x.KeyString == repositoryName);
            return repositories;
        }

        public static bool SwitchUserRepository(string repositoryName, UserRecord userRecord)
        {
            if (userRecord != null)
                UpdateUserRepositoryUserRecord(userRecord);

            if (Repositories.KeyString != repositoryName)
            {
                IMainRepository repositories = GetUserRepository(repositoryName);

                if (repositories == null)
                    repositories = Global.LoadUserRepository(repositoryName);

                if (repositories != null)
                {
                    Repositories = repositories;
                    MainRepository.Global = repositories;
                    RepositoryName = repositoryName;
                    Settings.SetString("RepositoryName", repositoryName);
                    if (userRecord != null)
                        SyncUserRepositoryUserRecord(userRecord);
                    if (repositoryName == BaseRepositoryName)
                        SwitchContentDirectories(BaseContentTildeUrl, repositories.ContentPath);
                    else
                    {
                        string contentTildeUrl = GetUserRepositoryContentUrl(repositoryName);
                        SwitchContentDirectories(contentTildeUrl, repositories.ContentPath);
                    }
                    return true;
                }
            }
            else
                return true;

            return false;
        }

        public static string GetUserRepositoryContentUrl(string repositoryName)
        {
            List<string> userRepositoryDefinitions = ApplicationData.Settings.GetStringList("UserRepositoryDefinitions");

            for (int index = 0; index < userRepositoryDefinitions.Count(); index += 3)
            {
                if (userRepositoryDefinitions[index] == repositoryName)
                {
                    string url = userRepositoryDefinitions[index + 2];
                    return url;
                }
            }

            return null;
        }

        public virtual string GetAppUrlPrefix()
        {
            return String.Empty;
        }

        public static bool SwitchMainRepository()
        {
            if (Repositories.KeyString != BaseRepositoryName)
            {
                IMainRepository repositories = GetUserRepository(BaseRepositoryName);

                if (repositories != null)
                {
                    Repositories = repositories;
                    MainRepository.Global = repositories;
                    RepositoryName = BaseRepositoryName;
                    Settings.SetString("RepositoryName", BaseRepositoryName);
                    SwitchContentDirectories(BaseContentTildeUrl, repositories.ContentPath);
                    return true;
                }
            }
            else
                return true;

            return false;
        }

        public static void SwitchContentDirectories(string contentTildeUrl, string contentPath)
        {
            if (contentPath.EndsWith(PlatformPathSeparator + "Content"))
                BasePlatformDirectory = contentPath.Substring(0, contentPath.Length - 8);
            else
                BasePlatformDirectory = contentPath;

            ContentTildeUrl = contentTildeUrl;
            CrawlTildeUrl = contentTildeUrl + "/" + "Crawl";
            DatabaseTildeUrl = contentTildeUrl + "/" + "Database";
            ImagesTildeUrl = contentTildeUrl + "/" + "Images";
            LocalDataTildeUrl = contentTildeUrl + "/" + "LocalData";
            MediaTildeUrl = contentTildeUrl + "/" + "Media";
            PicturesTildeUrl = contentTildeUrl + "/" + "Pictures";
            SandboxTildeUrl = contentTildeUrl + "/" + "Sandbox";
            TagsTildeUrl = contentTildeUrl + "/" + "tags";
            TempTildeUrl = contentTildeUrl + "/" + "Temp";
            UserDataTildeUrl = contentTildeUrl + "/" + "UserData";
            ContentPath = MapToFilePath(ContentTildeUrl);
            CrawlPath = MapToFilePath(CrawlTildeUrl);
            DatabasePath = MapToFilePath(DatabaseTildeUrl);
            ImagesPath = MapToFilePath(ImagesTildeUrl);
            LocalDataPath = MapToFilePath(LocalDataTildeUrl);
            MediaPath = MapToFilePath(MediaTildeUrl);
            PicturesPath = MapToFilePath(PicturesTildeUrl);
            SandboxPath = MapToFilePath(SandboxTildeUrl);
            TagsPath = MapToFilePath(TagsTildeUrl);
            TempPath = MapToFilePath(TempTildeUrl);
            UserDataPath = MapToFilePath(UserDataTildeUrl);
        }

        public static bool SyncUserRepositoryUserRecord(UserRecord oldUserRecord)
        {
            bool returnValue = false;

            try
            {
                if (oldUserRecord.IsAnonymous())
                    returnValue = false;
                else
                {
                    UserRecord newUserRecord = Repositories.UserRecords.Get(oldUserRecord.Key);

                    if (newUserRecord == null)
                    {
                        newUserRecord = new UserRecord(oldUserRecord);

                        newUserRecord.ModifiedTime = DateTime.UtcNow;
                        newUserRecord.Modified = false;

                        if (Repositories.UserRecords.Add(newUserRecord))
                            returnValue = true;
                        else
                            returnValue = false;
                    }
                    else
                    {
                        UserProfile oldProfile = oldUserRecord.CurrentUserProfile;
                        UserProfile newProfile = newUserRecord.FindUserProfile(oldProfile.ProfileName);

                        if (newProfile == null)
                        {
                            newProfile = new UserProfile(oldProfile);
                            newUserRecord.AddUserProfile(newProfile);
                            newUserRecord.CurrentUserProfile = newProfile;
                            newUserRecord.ModifiedTime = DateTime.UtcNow;
                            newUserRecord.Modified = false;

                            if (Repositories.UserRecords.Update(newUserRecord))
                                returnValue = true;
                            else
                                returnValue = false;
                        }
                        else if (newUserRecord.CurrentUserProfile != newProfile)
                        {
                            newUserRecord.CurrentUserProfile = newProfile;
                            newUserRecord.ModifiedTime = DateTime.UtcNow;
                            newUserRecord.Modified = false;

                            if (Repositories.UserRecords.Update(newUserRecord))
                                returnValue = true;
                            else
                                returnValue = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                returnValue = false;
            }

            return returnValue;
        }


        public static bool UpdateUserRepositoryUserRecord(UserRecord userRecord)
        {
            bool returnValue = false;

            try
            {
                userRecord.ModifiedTime = DateTime.UtcNow;
                userRecord.Modified = false;

                if (userRecord.IsAnonymous())
                    returnValue = false;
                else
                {
                    if (Repositories.UserRecords.Get(userRecord.Key) != null)
                    {
                        if (Repositories.UserRecords.Update(userRecord))
                            returnValue = true;
                        else
                            returnValue = false;
                    }
                    else
                    {
                        if (Repositories.UserRecords.Add(userRecord))
                            returnValue = true;
                        else
                            returnValue = false;
                    }
                }
            }
            catch (Exception)
            {
                returnValue = false;
            }

            return returnValue;
        }

        public virtual void ServiceUrlChanged()
        {
        }

        public virtual string MakeUrl(string action, string controller, object routeValues)
        {
            string url = UrlUtilities.Action(action, controller, routeValues);
            return url;
        }

        public virtual string UrlEncode(string str)
        {
            return TextUtilities.UrlEncode(str);
        }

        public virtual string UrlDecode(string str)
        {
            return TextUtilities.UrlDecode(str);
        }

        public virtual string HtmlEncode(string str)
        {
            return TextUtilities.HtmlEncode(str);
        }

        public virtual string HtmlDecode(string str)
        {
            return TextUtilities.HtmlDecode(str);
        }

        public virtual string CreateHostUrl(string controller, string action, object routeValues)
        {
            throw new Exception("ApplicationData.CreateHostUrl not overidden.");
        }

        public virtual bool HandleMediaAccess(ref string url, ref MediaStorageState storageState, out bool changed)
        {
            bool returnValue = true;

            changed = false;

            if (String.IsNullOrEmpty(url))
                return false;

            MediaStorageState oldState = storageState;

            switch (storageState)
            {
                case MediaStorageState.Unknown:
                case MediaStorageState.Present:
                case MediaStorageState.Downloaded:
                    if (url.StartsWith("~"))
                    {
                        string path = MapToFilePath(url);
                        if (FileSingleton.Exists(path))
                        {
                            if (storageState == MediaStorageState.Unknown)
                                storageState = MediaStorageState.Present;
                        }
                        else if (CopyRemoteMediaOnDemand)
                        {
                            if (IsConnectedToANetwork())
                            {
                                string remoteUrl = GetRemoteMediaUrlFromTildeUrl(url);
                                string errorMessage = null;

                                if (GetRemoteMediaFile(remoteUrl, path, ref errorMessage))
                                {
                                    storageState = MediaStorageState.Downloaded;
                                    BookkeepMediaFlushCache();
                                }
                                else
                                {
                                    storageState = MediaStorageState.BadLink;
                                    returnValue = false;
                                }
                            }
                            else
                                returnValue = false;
                        }
                        else
                            returnValue = false;
                    }
                    else
                        storageState = MediaStorageState.External;
                    break;
                case MediaStorageState.Absent:
                    break;
                case MediaStorageState.External:
                    break;
                case MediaStorageState.BadLink:
                    returnValue = false;
                    break;
                default:
                    break;
            }

            if (oldState != storageState)
                changed = true;

            return returnValue;
        }

        public bool GetRemoteMediaFilesFromFilePaths(
            List<string> mediaFiles, bool overwrite, LanguageUtilities languageUtilities, out string errorMessage)
        {
            int count = mediaFiles.Count + 1;
            int index = 0;
            List<string> savedFiles = new List<string>(count);
            bool returnValue = true;

            errorMessage = String.Empty;

            if (ApplicationData.Global.IsConnectedToANetwork())
            {
                ProgressOperation_Dispatch(ProgressMode.Start, count, "Downloading media files...");

                foreach (string filePath in mediaFiles)
                {
                    if (ProgressCancelCheck() || CanceledCheck())
                    {
                        errorMessage = errorMessage
                            + languageUtilities.TranslateUIString("Operation cancelled by user.") + "\n";
                        returnValue = false;
                        break;
                    }

                    string remoteUrl = ApplicationData.GetRemoteMediaUrlFromFilePath(filePath);
                    string fileName = MediaUtilities.GetFileName(remoteUrl);

                    if (overwrite || !FileSingleton.Exists(filePath))
                    {
                        ProgressOperation_Dispatch(ProgressMode.Update, index, "Getting: " + fileName);

                        if (ApplicationData.Global.GetRemoteMediaFile(remoteUrl, filePath, ref errorMessage))
                            savedFiles.Add(filePath);
                        else
                            returnValue = false;
                    }

                    index++;
                }

                ProgressOperation_Dispatch(ProgressMode.Update, index++, "Updating file bookkeeping...");
                BookkeepMediaFlushCache();

                ProgressOperation_Dispatch(ProgressMode.Stop, index, null);
                //// Let PostHandleRequest operation stop the progress.
                //ProgressOperation_Dispatch(ProgressMode.Update, index, "Finished server communication.");
            }
            else
            {
                errorMessage = languageUtilities.TranslateUIString(
                    "Sorry, can't download files because there is no network connection.");
                returnValue = false;
            }

            return returnValue;
        }

        public bool CanceledCheck()
        {
            if ((RemoteClient != null) && RemoteClient.CancelFlag)
                return true;
            return false;
        }

        public virtual bool IsConnectedToANetwork()
        {
            return true;
        }

        public virtual bool IsNetworkHostReachable(string host)
        {
            return true;
        }

        public virtual bool GetDiskSpaceInfo(out long totalSpace, out long usedSpace, out long freeSpace)
        {
            totalSpace = 0L;
            usedSpace = 0L;
            freeSpace = 0L;
            return false;
        }

        public virtual bool GetCloudFile(string url, string filePath, ref string errorMessage)
        {
            return false;
        }

        public static string GetRemoteMediaUrlFromTildeUrl(string tildeUrl)
        {
            string returnValue = tildeUrl;

            if (String.IsNullOrEmpty(tildeUrl))
                return returnValue;

            if (tildeUrl.StartsWith("~"))
                returnValue = ServiceUrl + tildeUrl.Substring(1);

            return returnValue;
        }

        public static string GetRemoteMediaUrlFromFilePath(string filePath)
        {
            string returnValue = String.Empty;

            if (String.IsNullOrEmpty(filePath))
                return returnValue;

            string serviceUrl = ServiceUrl;

            if (!serviceUrl.EndsWith("/"))
                serviceUrl = serviceUrl + "/";

            string basePlatformDirectory = BasePlatformDirectory;

            if (basePlatformDirectory.EndsWith("/") || basePlatformDirectory.EndsWith(@"\"))
                basePlatformDirectory = basePlatformDirectory.Substring(0, basePlatformDirectory.Length);

            if (filePath.StartsWith(basePlatformDirectory))
                returnValue = ServiceUrl + filePath.Substring(basePlatformDirectory.Length).Replace(@"\", "/");

            return returnValue;
        }

        public virtual bool MaybeGetRemoteMediaFile(string remoteUrl, string outputFilePath, ref string errorMessage)
        {
            return false;
        }

        public virtual bool GetRemoteMediaFile(string remoteUrl, string outputFilePath, ref string errorMessage)
        {
            return false;
        }

        public virtual bool GetRemoteContentFile(string url, IDataBuffer dataBuffer, string filePath, ref string errorMessage)
        {
            if (RemoteClient == null)
                return false;

            int mediaOfs = url.ToLower().IndexOf("/media/");

            if (mediaOfs == -1)
                return GetFile(url, dataBuffer, filePath, ref errorMessage);

            string partialPath = url.Substring(mediaOfs + 7);

            int userEndOfs = partialPath.IndexOf("/");

            if (userEndOfs == -1)
                return GetFile(url, dataBuffer, filePath, ref errorMessage);

            string userName = partialPath.Substring(0, userEndOfs);

            string mediaPath = partialPath.Substring(userEndOfs + 1);

            string fileType = MediaUtilities.GetFileTypeFromFileName(mediaPath);
            UserID userID = new UserID("LetMeIn");
            string userIDString = userID.StringData;
            byte[] data;

            bool returnValue = RemoteClient.GetContentFile(
                mediaPath,
                fileType,
                userName,
                userIDString,
                out data,
                out errorMessage);

            if (returnValue)
            {
                dataBuffer.SetAllBytes(data);
                long incomingSize = data.Length;
                BookkeepMediaFileAddCached(filePath, incomingSize);
                returnValue = CheckIncomingRemoteFile(filePath, incomingSize);
            }

            return returnValue;
        }

        public virtual bool GetFile(string url, IDataBuffer dataBuffer, string filePath, ref string errorMessage)
        {
            return false;
        }

        public virtual bool GetRemoteMediaDirectoryList(
            string remoteUrl,
            List<string> extensions,
            out List<string> fileNames)
        {
            if (RemoteClient != null)
                return RemoteClient.GetMediaDirectoryList(remoteUrl, extensions, out fileNames);
            else
                fileNames = new List<string>();
            return false;
        }

        public virtual bool CheckIncomingRemoteFile(string filePath, long length)
        {
            if (LimitRemoteMedia)
            {
                long newSize = CurrentTotalMediaSize + length;
                long freedSize = 0;

                if (newSize > DownloadedMediaSizeLimit)
                {
                    if (!AutoDeleteOlderData)
                        return false;

                    long sizeToFree = newSize - DownloadedMediaSizeLimit;
                    freedSize = FreeUpMediaSpace(sizeToFree);

                    UpdateTotalMediaSize(length - freedSize);
                }
            }

            return true;
        }

        public virtual bool GetWebData(
            string url,
            string method,      // "GET" or "POST"
            IDataBuffer dataBuffer,
            ref string errorMessage)
        {
            return false;
        }

        public virtual bool GetWebString(
            string url,
            string method,      // "GET" or "POST"
            out string str,
            ref string errorMessage)
        {
            str = String.Empty;
            return false;
        }

        public virtual bool GetWebJson(
            string url,
            string method,      // "GET" or "POST"
            out object obj,
            ref string errorMessage)
        {
            obj = null;
            return false;
        }

        // Returns false if thread-qued.
        public virtual bool RefreshFiles(bool force)
        {
            return true;
        }

        // Returns false if thread-qued.
        public virtual bool RefreshFile(string filePath, bool force)
        {
            return true;
        }

        public List<string> BookkeptMediaFiles
        {
            get
            {
                return Settings.GetStringList("BookkeptMediaFiles");
            }
            set
            {
                Settings.SetStringList("BookkeptMediaFiles", value);
            }
        }

        public virtual void BookkeepMediaFileRemove(string filePath, long size)
        {
            List<string> mediaFiles = BookkeptMediaFiles;

            if (mediaFiles.Remove(filePath))
                BookkeptMediaFiles = mediaFiles;

            UpdateTotalMediaSize(-size);
        }

        public virtual void BookkeepMediaFileAdd(string filePath, long size)
        {
            if (String.IsNullOrEmpty(filePath))
                return;

            List<string> mediaFiles = BookkeptMediaFiles;

            if (!mediaFiles.Contains(filePath))
                mediaFiles.Add(filePath);

            BookkeptMediaFiles = mediaFiles;

            UpdateTotalMediaSize(size);
        }

        public virtual void BookkeepMediaFileAddCached(string filePath, long size)
        {
            if (String.IsNullOrEmpty(filePath))
                return;

            if (TemporaryCachedRemoteFiles == null)
            {
                TemporaryCachedRemoteFiles = new List<string>();
                TemporaryCachedBookkeptFiles = BookkeptMediaFiles;
                TemporaryCachedRemoteFilesSize = 0;
            }

            if (!TemporaryCachedBookkeptFiles.Contains(filePath))
            {
                TemporaryCachedRemoteFiles.Add(filePath);
                TemporaryCachedBookkeptFiles.Add(filePath);
                TemporaryCachedRemoteFilesSize += size;
            }
        }

        public virtual void BookkeepMediaFlushCache()
        {
            if (TemporaryCachedRemoteFiles == null)
                return;

            BookkeptMediaFiles = TemporaryCachedBookkeptFiles;
            UpdateTotalMediaSize(TemporaryCachedRemoteFilesSize);
            TemporaryCachedRemoteFiles = null;
            TemporaryCachedBookkeptFiles = null;
            TemporaryCachedRemoteFilesSize = 0;
        }

        public virtual long FreeUpMediaSpace(long sizeToFree)
        {
            long freedUpSpace = 0;
            List<string> mediaFiles = BookkeptMediaFiles;
            int index = 0;

            while (index < mediaFiles.Count)
            {
                string filePath = mediaFiles[index];

                long size = FileSingleton.FileSize(filePath);

                try
                {
                    FileSingleton.Delete(filePath);
                    mediaFiles.RemoveAt(0);
                    freedUpSpace += size;

                    if (freedUpSpace >= sizeToFree)
                        break;
                }
                catch (Exception)
                {
                    index++;
                }
            }

            if (freedUpSpace != 0)
                BookkeptMediaFiles = mediaFiles;

            return freedUpSpace;
        }

        public virtual bool UpdateTotalMediaSize(long delta)
        {
            CurrentTotalMediaSize = CurrentTotalMediaSize + delta;
            Settings.SetLong("CurrentTotalMediaSize", CurrentTotalMediaSize);
            return true;
        }

        public virtual bool UpdateDatabaseCheck(bool doTimeCheck, bool force, out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public virtual bool UpdateObjectStoreCheck(string name, LanguageID languageID, bool isConnectedToANetwork,
            bool doTimeCheck, bool force, ref bool neededUpdate, out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public virtual bool UpdateTeachersCheck(List<LanguageID> targetLanguageIDs, out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public virtual bool UpdateTeachers(List<LanguageID> targetLanguageIDs, out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public virtual void RunAsThread(WaitCallback threadOp, WaitCallback continueOp)
        {
            ThreadPool.QueueUserWorkItem(o => SubThread(threadOp, continueOp));
        }

        public void SubThread(WaitCallback threadOp, WaitCallback continueOp)
        {
            threadOp(null);
            continueOp(null);
        }

        public virtual void Sleep(int msec)
        {
        }

        protected bool InProgress = false;
        protected bool InDelayedProgress = false;
        protected bool InPageTransition = false;
        protected static Timer ProgressDelayedTimer = null;
        protected static object ProgressDelayedtimerLock = new object();
        public static int ProgressDelayMsec = 1000;

        public virtual void ProgressOperation_Dispatch(ProgressMode mode, int value, string message)
        {
        }

        public virtual void ProgressOperation(ProgressMode mode, int value, string message)
        {
        }

        // Returns true if cancelled.
        public virtual bool ProgressCancelCheck()
        {
            return false;
        }

        protected void ProgressTimerStart(int msecDelay)
        {
            lock (ProgressDelayedtimerLock)
            {
                if (ProgressDelayedTimer == null)
                {
                    InDelayedProgress = true;
                    ProgressDelayedTimer = new Timer(ProgressTimerCallback, null, msecDelay, Timeout.Infinite);
                }
            }
        }

        protected void ProgressTimerStop()
        {
            lock (ProgressDelayedtimerLock)
            {
                InDelayedProgress = false;

                if (ProgressDelayedTimer != null)
                {
                    ProgressDelayedTimer.Dispose();
                    ProgressDelayedTimer = null;
                }
            }
        }

        private void ProgressTimerCallback(object state)
        {
            lock (ProgressDelayedtimerLock)
            {
                if (ProgressDelayedTimer != null)
                {
                    ProgressDelayedTimer.Dispose();
                    ProgressDelayedTimer = null;
                }
                else
                {
                    InDelayedProgress = false;
                    return;
                }

                if (InDelayedProgress)
                    ProgressOperation_Dispatch(ProgressMode.DelayedShow, 0, null);
            }
        }

        public bool PreHandleRequest(string url)
        {
            if (!InPageTransition)
            {
                InPageTransition = true;
                ProgressTimerStart(ProgressDelayMsec);
            }

            return true;
        }

        public bool PostHandleRequest(string url)
        {
            InPageTransition = false;
            ProgressTimerStop();
            ProgressOperation_Dispatch(ProgressMode.Hide, 0, null);
            return true;
        }

        public UserRecord GetUserRecord(string userName)
        {
            if (String.IsNullOrEmpty(userName))
                return null;

            UserRecord userRecord = null;

            try
            {
                userRecord = Repositories.UserRecords.Get(userName);
            }
            catch (Exception)
            {
            }

            return userRecord;
        }

        public virtual AnonymousUserRecord GetAnonymousUserRecord()
        {
            return null;
        }

        public virtual AnonymousUserRecord CopyAnonymousUserRecord(UserRecord copyFrom)
        {
            if (copyFrom == null)
                return null;
            UserProfile userProfile = copyFrom.CurrentUserProfile;
            AnonymousUserRecord userRecord = new AnonymousUserRecord(
                copyFrom.UserName,
                "(anonymous)",
                "(no email)",
                "student",
                copyFrom.Standing,
                new LanguageDescriptor(userProfile.UILanguageDescriptor),
                userProfile.CloneHostLanguageDescriptors(),
                userProfile.CloneTargetLanguageDescriptors(),
                null,
                null,
                null,
                null,
                copyFrom.IsMinor);
            return userRecord;
        }

        public virtual bool CanGoogleTranslate(LanguageID keyLanguageID, LanguageID resultLanguageID)
        {
            return true;
        }

        public static string GoogleTranslateSourceName = "Google Translate";

        protected static int _GoogleTranslateSourceID = 0;
        public static int GoogleTranslateSourceID
        {
            get
            {
                if (_GoogleTranslateSourceID == 0)
                    _GoogleTranslateSourceID = ApplicationData.DictionarySourcesLazy.Add(GoogleTranslateSourceName);

                return _GoogleTranslateSourceID;
            }
        }

        public static string TranslatorDictionarySourceName = "Translator Dictionary";

        protected static int _TranslatorDictionarySourceID = 0;
        public static int TranslatorDictionarySourceID
        {
            get
            {
                if (_TranslatorDictionarySourceID == 0)
                    _TranslatorDictionarySourceID = ApplicationData.DictionarySourcesLazy.Add(TranslatorDictionarySourceName);

                return _TranslatorDictionarySourceID;
            }
        }

        public static string TranslatorConversionSourceName = "Translator Conversion";

        protected static int _TranslatorConversionSourceID = 0;
        public static int TranslatorConversionSourceID
        {
            get
            {
                if (_TranslatorConversionSourceID == 0)
                    _TranslatorConversionSourceID = ApplicationData.DictionarySourcesLazy.Add(TranslatorConversionSourceName);

                return _TranslatorConversionSourceID;
            }
        }

        public static string TranslatorDatabaseSourceName = "Translator Database";

        protected static int _TranslatorDatabaseSourceID = 0;
        public static int TranslatorDatabaseSourceID
        {
            get
            {
                if (_TranslatorDatabaseSourceID == 0)
                    _TranslatorDatabaseSourceID = ApplicationData.DictionarySourcesLazy.Add(TranslatorDatabaseSourceName);

                return _TranslatorDatabaseSourceID;
            }
        }

        public static string TranslatorDatabaseCacheSourceName = "Translator Database Cache";

        protected static int _TranslatorDatabaseCacheSourceID = 0;
        public static int TranslatorDatabaseCacheSourceID
        {
            get
            {
                if (_TranslatorDatabaseCacheSourceID == 0)
                    _TranslatorDatabaseCacheSourceID = ApplicationData.DictionarySourcesLazy.Add(TranslatorDatabaseCacheSourceName);

                return _TranslatorDatabaseCacheSourceID;
            }
        }

        public static string TranslatorMemoryCacheSourceName = "Translator Memory Cache";

        protected static int _TranslatorMemoryCacheSourceID = 0;
        public static int TranslatorMemoryCacheSourceID
        {
            get
            {
                if (_TranslatorMemoryCacheSourceID == 0)
                    _TranslatorMemoryCacheSourceID = ApplicationData.DictionarySourcesLazy.Add(TranslatorMemoryCacheSourceName);

                return _TranslatorMemoryCacheSourceID;
            }
        }

        public static DictionarySources DictionarySourcesLazy
        {
            get
            {
                if (DictionarySources == null)
                {
                    string dictionarySourcesFilePath = MediaUtilities.ConcatenateFilePath(
                        MediaUtilities.ConcatenateFilePath(LocalDataPath, "Dictionary"),
                        "DictionarySources.xml");
                    DictionarySources = new DictionarySources(dictionarySourcesFilePath);
                }

                return DictionarySources;
            }
        }

        public static PersistentStringMapper InflectionLabelsLazy
        {
            get
            {
                if (InflectionLabels == null)
                {
                    string inflectionLabelsFilePath = MediaUtilities.ConcatenateFilePath(
                        MediaUtilities.ConcatenateFilePath(LocalDataPath, "Dictionary"),
                        "InflectionLabels.xml");
                    InflectionLabels = new PersistentStringMapper("InflectionLabels", inflectionLabelsFilePath);
                }

                return InflectionLabels;
            }
        }

        public string GetUIString(string englishString, LanguageID uiLanguageID)
        {
            if (uiLanguageID.LanguageCode == "en")
                return englishString;

            BaseString stringObject = Repositories.TranslateUIString(englishString, uiLanguageID);

            if (stringObject != null)
                return stringObject.Text;

            return englishString;
        }

        // Get remote tree.
        public virtual BaseObjectNodeTree GetRemoteTree(BaseObjectNodeTree tree, out string errorMessage)
        {
            errorMessage = null;
            return null;
        }

        // Get remote study list.
        public virtual ContentStudyList GetRemoteStudyList(ContentStudyList studyList, out string errorMessage)
        {
            errorMessage = null;
            return null;
        }

        // Update study list from remote.
        public virtual bool UpdateStudyList(ContentStudyList studyList, out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public virtual bool CaptchaCheck(
            string page,
            string captchaValue,
            string userIP,
            ref string errorMessage)
        {
            bool returnValue = true;
            return returnValue;
        }

        public virtual bool SendMessage(
            string subject,
            string messageBody,
            string fromAddress,
            string toAddress,
            string ccAddress,
            out string errorMessage)
        {
            errorMessage = "SendMessage not overidden.";
            return false;
        }

        public static IComparer<T> GetComparer<T>(bool caseInsensitive)
        {
            IComparer<T> returnValue = null;
            string languageCode = String.Empty;

            try
            {
#if CORE
                if (caseInsensitive)
                    returnValue = (IComparer<T>)StringComparer.CurrentCultureIgnoreCase;
                else
                    returnValue = (IComparer<T>)StringComparer.CurrentCulture;
#else
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentUICulture;
                if (cultureInfo != null)
                    returnValue = (IComparer<T>)StringComparer.Create(cultureInfo, caseInsensitive);
#endif
            }
            catch (Exception)
            {
            }

            return returnValue;
        }

        public static IComparer<T> GetLanguageComparer<T>(LanguageID languageID, bool caseInsensitive)
        {
            IComparer<T> returnValue = null;
            string languageCode = String.Empty;

            if (languageID != null)
            {
                languageCode = languageID.LanguageCode;

                if (!String.IsNullOrEmpty(languageCode))
                {
                    if (languageCode.StartsWith("("))
                        languageCode = String.Empty;
                }
            }

            if (String.IsNullOrEmpty(languageCode))
                return returnValue;

            try
            {
#if CORE
                //FIXME:  .Net Core doesn't support creating a string comparer for other languages.
                CultureInfo cultureInfo = null;

                if (CreateCulture != null)
                    cultureInfo = CreateCulture(languageCode);

                if (cultureInfo != null)
                {
                    CultureInfo saveCulture = CultureInfo.DefaultThreadCurrentCulture;
                    CultureInfo saveUICulture = CultureInfo.DefaultThreadCurrentUICulture;
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                    if (caseInsensitive)
                        returnValue = (IComparer<T>)StringComparer.CurrentCultureIgnoreCase;
                    else
                        returnValue = (IComparer<T>)StringComparer.CurrentCulture;
                    CultureInfo.DefaultThreadCurrentCulture = saveCulture;
                    CultureInfo.DefaultThreadCurrentUICulture = saveUICulture;
                }
#else
                CultureInfo cultureInfo = null;

                if (CreateCulture != null)
                    cultureInfo = CreateCulture(languageCode);

                if (cultureInfo != null)
                    returnValue = (IComparer<T>)StringComparer.Create(cultureInfo, caseInsensitive);
#endif
            }
            catch (Exception)
            {
            }

            return returnValue;
        }

        public string GetLocalDataFilePath(string directory, string fileName)
        {
            string filePath = LocalDataPath + PlatformPathSeparator +
                directory + PlatformPathSeparator +
                fileName;
            return filePath;
        }

        public string GetLocalDataLanguageFilePath(string directory, LanguageID languageID, string fileName)
        {
            string filePath = LocalDataPath + PlatformPathSeparator +
                directory + PlatformPathSeparator +
                languageID.SymbolName + PlatformPathSeparator +
                fileName;
            return filePath;
        }

        public string GetSandboxFileUrl(string userName, string fileName)
        {
            string url = SandboxTildeUrl + "/" + userName + "/" + fileName;
            return url;
        }

        public string GetSandboxFileName(string userName, string fileName)
        {
            string filePath = SandboxPath + PlatformPathSeparator + userName + PlatformPathSeparator + fileName;
            return filePath;
        }

        public string GetTagsFileUrl(string fileName)
        {
            string url = TagsTildeUrl + "/" + fileName;
            return url;
        }

        public string GetTagsFileName(string fileName)
        {
            string filePath = TagsPath + PlatformPathSeparator + fileName;
            return filePath;
        }

        public string GetTempFileUrl(string extension)
        {
            string fileName = DateTime.UtcNow.Ticks.ToString() + extension;
            string fileUrl = TempTildeUrl + "/" + fileName;
            return fileUrl;
        }

        public string GetTempFileName(string extension)
        {
            string fileName = DateTime.UtcNow.Ticks.ToString() + extension;
            string filePath = TempPath + PlatformPathSeparator + fileName;
            return filePath;
        }

        public string GetTempFileUrl(string userName, string fileName)
        {
            string fileUrl = TempPath + "/" + userName + "/" + fileName;
            return fileUrl;
        }

        public string GetTempFileName(string userName, string fileName)
        {
            string filePath = TempPath + PlatformPathSeparator + userName + PlatformPathSeparator + fileName;
            return filePath;
        }

        public string GetUserDataFilePath(string userName, string fileName)
        {
            string filePath = UserDataPath + PlatformPathSeparator +
                userName + PlatformPathSeparator +
                fileName;
            return filePath;
        }

        public string GetUserDataNestedFilePath(string userName, string directory, string fileName)
        {
            string filePath = UserDataPath + PlatformPathSeparator +
                userName + PlatformPathSeparator +
                directory + PlatformPathSeparator +
                fileName;
            return filePath;
        }

        public string GetUserDataDoubleNestedFilePath(string userName, string directory, string subDirectory, string fileName)
        {
            string filePath = UserDataPath + PlatformPathSeparator +
                userName + PlatformPathSeparator +
                directory + PlatformPathSeparator +
                subDirectory + PlatformPathSeparator +
                fileName;
            return filePath;
        }

        public string GetUserProfileDataFilePath(string userName, string profileName, string fileName)
        {
            string filePath = UserDataPath + PlatformPathSeparator +
                userName + PlatformPathSeparator +
                profileName + PlatformPathSeparator +
                fileName;
            return filePath;
        }

        public virtual XElement LoadXml(string filePath)
        {
            throw new Exception("ApplicationData.LoadXml: Needs platform override.");
        }

        public virtual void SaveXml(XElement element, string filePath)
        {
            throw new Exception("ApplicationData.SaveXml: Needs platform override.");
        }

        public virtual string RenderPartialViewToString(ControllerUtilities controller, string viewName,
            object model, string controllerGroup, string controllerAction)
        {
            return String.Empty;
        }

        public virtual AutomatedCompiledMarkup GetAutomatedState(
            string userName, int treeKey, int nodeKey, string contentKey)
        {
            return null;
        }

        public virtual bool SetAutomatedState(
            string userName, int treeKey, int nodeKey, string contentKey,
            AutomatedCompiledMarkup compiledMarkup)
        {
            return false;
        }

        public virtual TimeZoneInfo GetLocalTimeZone()
        {
            return null;
        }

        public virtual string GetLocalTimeZoneID()
        {
            return null;
        }

        public virtual TimeSpan GetLocalTimeZoneOffset()
        {
            return TimeSpan.Zero;
        }

        public virtual string GetTimeZoneID(TimeZoneInfo timeZoneInfo)
        {
            return null;
        }

        public virtual string GetValidTimeZoneID(string timeZoneID)
        {
            return timeZoneID;
        }

        public virtual System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
        {
            return null;
        }

        public virtual List<BaseString> GetTimeZoneStrings()
        {
            return null;
        }

        public virtual TimeZoneInfo FindSystemTimeZoneById(string timeZoneID)
        {
            return null;
        }

        public virtual bool AuthenticateUser(UserID userID)
        {
            return true;
        }

        public virtual UserID AuthenticateUser(string userName, string password)
        {
            return null;
        }

        public virtual bool LoginMember(string memberName)
        {
            return true;
        }

        public virtual bool LoginMember(UserRecord memberRecord, bool isPersistent, ref string errorMessage)
        {
            return true;
        }

        public virtual bool LogoffMember(UserRecord memberRecord, bool isPersistent)
        {
            return true;
        }

        public virtual bool RegisterMember(UserRecord memberRecord, bool doLogin, ref string errorMessage)
        {
            try
            {
                if (Repositories.UserRecords.Get(memberRecord.UserName) == null)
                {
                    memberRecord.EnsureGuid();
                    memberRecord.TouchAndClearModified();

                    if (Repositories.UserRecords.Add(memberRecord))
                        return true;

                    if (!String.IsNullOrEmpty(errorMessage))
                        errorMessage = errorMessage + "\r\n";

                    errorMessage += "Member record for " + memberRecord.UserName + " already exists.";
                }
                else
                    return true;
            }
            catch (Exception exception)
            {
                if (!String.IsNullOrEmpty(errorMessage))
                    errorMessage = errorMessage + "\r\n";

                errorMessage += "Exception while registering: " + memberRecord.UserName + ": "
                    + exception.Message;

                if (exception.InnerException != null)
                    errorMessage += ": " + exception.InnerException.Message;
            }

            return false;
        }

        public virtual bool DeleteMember(string memberName, UserRecord memberRecord, ref string errorMessage)
        {
            if (!Repositories.UserRecords.DeleteKey(memberName))
            {
                if (!String.IsNullOrEmpty(errorMessage))
                    errorMessage = errorMessage + "\r\n";

                errorMessage += "Member record for " + memberRecord.UserName + " does not exist.";

                return false;
            }

            return true;
        }

        public virtual ApplicationMenu GetApplicationMenu(IApplicationCookies cookies,
            UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
            LanguageUtilities languageUtilities, bool recreate)
        {
            return null;
        }

        public string TreeArray(NodeUtilities nodeUtilities, BaseObjectNodeTree tree)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[\n");

            if (tree != null)
            {
                string treeType = tree.Label;
                string treeTypePlural = treeType + "s";
                string treeText = nodeUtilities.LanguageUtilities.TranslateUIString(treeTypePlural);
                string action = (treeType == "Course" ? "Courses" : "PlanList");
                string controller = (treeType == "Course" ? "Lessons" : "Plans");
                string treesUrl = ActionUrlPrefix + controller + "/" + action;

                nodeUtilities.ShowEmptyContentItemsInTreeView = nodeUtilities.UserProfile.GetUserOptionFlag("ShowEmptyContentItemsInTreeView", true);

                // Append root.
                sb.Append("	['" + treeText + "', ['javascript:jt_main_body_goto_raw(\"" + treesUrl + "\")'],\n");

                if ((tree != null) && !String.IsNullOrEmpty(tree.KeyString))
                {
                    sb.Append("		[\n");

                    // Append tree.
                    AddTreeNode(sb, nodeUtilities, tree, true);

                    sb.Append("		]\n");
                }

                sb.Append("	]\n");
            }

            sb.Append("]\n");

            string treeData = sb.ToString();

            if (tree.Modified)
                nodeUtilities.UpdateTree(tree, true, false);

            return treeData;
        }

        private void AddTreeNode(StringBuilder sb, NodeUtilities nodeUtilities, BaseObjectNodeTree tree, bool isLast)
        {
            if (tree == null)
                return;

            string treeKey = tree.KeyString;
            string treeSource = tree.Source;
            string treeType = (treeSource == "Courses" ? "course" : "plan");
            string imageName = (treeSource == "Courses" ? "Course" : "Plan");
            string tip = nodeUtilities.LanguageUtilities.TranslateUIString(treeType);
            string treeName = Filter(tree, tree.Title, nodeUtilities, tip);
            string treeAction = (treeSource == "Courses" ? "Course" : "Plan");
            string controller = (treeType == "course" ? "Lessons" : "Plans");
            string treeUrl = ActionUrlPrefix + controller + "/" + treeAction + "?treeKey=" + treeKey;

            sb.Append("					['" + treeName + "', ['javascript:jt_main_body_goto(\"" + treeUrl + "\")', , '" + imageName + "', '(" + treeType + ")'],\n");

            if ((tree != null) && (tree.HasChildren() || tree.HasContent()))
            {
                sb.Append("				        [\n");

                AddNodeChildrenAndContent(sb, nodeUtilities, tree, tree);

                sb.Append("				        ]\n");
            }

            sb.Append("			        ]" + (isLast ? "" : ",") + "\n");
        }

        private void AddNode(StringBuilder sb, NodeUtilities nodeUtilities, BaseObjectNodeTree tree, BaseObjectNode node, bool isLast)
        {
            if (node == null)
                return;
            UserRecord userRecord = nodeUtilities.UserRecord;
            if (!node.IsPublic && (node.Owner != userRecord.UserName) && (node.Owner != userRecord.Team) && !userRecord.IsAdministrator())
                return;
            string treeSource = tree.Source;
            string treeType = (treeSource == "Courses" ? "course" : "plan");
            string treeKey = tree.KeyString;
            string nodeType = (node.HasChildren() ? "group" : "lesson");
            string nodeTypeCapitalized = (node.HasChildren() ? "Group" : "Lesson");
            string imageName = nodeTypeCapitalized;
            string tip = nodeUtilities.LanguageUtilities.TranslateUIString(nodeType);
            string nodeName = Filter(node, node.Title, nodeUtilities, tip);
            string nodeKey = node.KeyString;
            string nodeAction = nodeTypeCapitalized;
            string controller = (treeType == "course" ? "Lessons" : "Plans");
            string nodeUrl = ActionUrlPrefix + controller + "/" + nodeAction + "?treeKey=" + treeKey + "&nodeKey=" + nodeKey;

            sb.Append("					['" + nodeName + "', ['javascript:jt_main_body_goto(\"" + nodeUrl + "\")', , '" + imageName + "', '(" + nodeType + ")'],\n");

            if (node.HasChildren() || node.HasContent())
            {
                sb.Append("				        [\n");

                AddNodeChildrenAndContent(sb, nodeUtilities, tree, node);

                sb.Append("				        ]\n");
            }

            sb.Append("			        ]" + (isLast ? "" : ",") + "\n");
        }

        private void AddNodeChildrenAndContent(StringBuilder sb, NodeUtilities nodeUtilities, BaseObjectNodeTree tree, BaseObjectNode node)
        {
            int childCount = node.ChildCount();
            int childIndex = 1;
            List<BaseObjectNode> nodeChildren = node.Children;
            List<BaseObjectContent> contents = node.ContentChildren;

            if (nodeChildren != null)
            {
                nodeChildren = new List<BaseObjectNode>(nodeChildren);
                FilterNodes(nodeChildren, nodeUtilities);
                childCount = nodeChildren.Count();
            }

            if (contents != null)
            {
                contents = new List<BaseObjectContent>(contents);
                FilterContents(contents, nodeUtilities);
            }

            int contentCount = contents.Count;
            int contentIndex = 1;
            bool hasAutomatedStudy = false;

            if ((contentCount != 0) && (nodeUtilities.AutomatedMarkupTemplateKey > 0))
            {
                if (contents.FirstOrDefault(x =>
                        (x.ContentClass == ContentClassType.MediaItem)
                        && (x.ContentType == "Automated")) == null)
                    hasAutomatedStudy = true;
            }

            if (childCount != 0)
            {
                foreach (BaseObjectNode child in nodeChildren)
                {
                    bool isLastChild = ((childIndex == childCount) && (contentCount == 0) && !hasAutomatedStudy ? true : false);
                    BaseObjectNode childNode = tree.GetNode(child.Key);
                    AddNode(sb, nodeUtilities, tree, childNode, isLastChild);
                    childIndex++;
                }
            }

            if (contentCount != 0)
            {
                NodeMaster master = node.Master;
                bool useMasterMenuInTree = true;

                if ((master != null) && (master.MenuItemsCount() != 0) && useMasterMenuInTree)
                {
                    int menuCount = master.MenuItems.Count;
                    int menuIndex = 1;

                    foreach (MasterMenuItem menuItem in master.MenuItems)
                    {
                        bool isLastMenuItem = (menuIndex == menuCount ? true : false);
                        if (menuItem.ContentType == "Automated")
                        {
                            if (hasAutomatedStudy)
                                AddAutomatedStudy(sb, nodeUtilities, tree, node, isLastMenuItem, menuItem);
                        }
                        else
                        {
                            BaseObjectContent content = node.GetContent(menuItem.KeyString);
                            if (content == null)
                            {
                                MultiLanguageString title = null;
                                MultiLanguageString description = null;
                                string source = null;
                                string label = null;
                                List<LanguageID> targetLanguageIDs = node.TargetLanguageIDs;
                                List<LanguageID> hostLanguageIDs = node.HostLanguageIDs;
                                string owner = node.Owner;
                                string contentType = menuItem.ContentType;
                                string contentSubType = menuItem.ContentSubType;
                                content = new BaseObjectContent(menuItem.KeyString, title, description,
                                    source, null, label, null, menuIndex, true, targetLanguageIDs, hostLanguageIDs, owner,
                                    null, contentType, contentSubType, null, null, null, null, null, null);
                                content.Node = (node != null ? node : tree);
                            }
                            AddContent(sb, nodeUtilities, tree, node, content, isLastMenuItem, menuItem);
                        }
                        menuIndex++;
                    }
                }
                else
                {
                    if (contents != null)
                    {
                        foreach (BaseObjectContent content in contents)
                        {
                            bool isLastContent = ((contentIndex == contentCount) && !hasAutomatedStudy ? true : false);
                            AddContent(sb, nodeUtilities, tree, node, content, isLastContent, null);
                            contentIndex++;
                        }

                        if (hasAutomatedStudy)
                            AddAutomatedStudy(sb, nodeUtilities, tree, node, true, null);
                    }
                }
            }
        }

        private void AddContent(StringBuilder sb, NodeUtilities nodeUtilities,
            BaseObjectNodeTree tree, BaseObjectNode node, BaseObjectContent content, bool isLast,
            MasterMenuItem menuItem)
        {
            if (content == null)
                return;
            UserRecord userRecord = nodeUtilities.UserRecord;
            if (!node.IsPublic && (node.Owner != userRecord.UserName) && (node.Owner != userRecord.Team) && !userRecord.IsAdministrator())
                return;
            BaseObjectNode contentNode = content.Node;
            string treeSource = tree.Source;
            string treeType = (treeSource == "Courses" ? "course" : "plan");
            string treeKey = tree.KeyString;
            string nodeKey = (((contentNode == null) || contentNode.IsTree()) ? "-1" : contentNode.KeyString);
            string nodeLabel = node.Label;
            string contentSource = content.Source;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;
            string lessonContentLabel;
            string nodeContentLabel;
            if (menuItem != null)
            {
                lessonContentLabel = menuItem.Text;
                nodeContentLabel = lessonContentLabel;
            }
            else
            {
                lessonContentLabel = contentType + (!String.IsNullOrEmpty(contentSubType) && (contentType != contentSubType) ? " " + contentSubType : "");
                nodeContentLabel = nodeLabel + " " + contentType;
                if (content.ContentClass != ContentClassType.StudyList)
                {
                    nodeContentLabel += " " + contentSubType;
                    if (!contentSubType.EndsWith("s"))
                        nodeContentLabel += "s";
                }
            }
            bool isTreeOrGroup = node.IsTree() || node.IsGroup();
            string contentLabel = (isTreeOrGroup ? nodeContentLabel : lessonContentLabel);
            string contentKey = content.KeyString;
            string tip = nodeUtilities.LanguageUtilities.TranslateUIString(contentLabel);
            string contentName = (menuItem != null ? contentLabel : Filter(content, content.Title, nodeUtilities, tip));
            if (isTreeOrGroup && (contentName == lessonContentLabel))
                contentName = nodeUtilities.LanguageUtilities.TranslateUIString(nodeContentLabel);
            string imageName = contentType;
            string contentAction = "Content";
            string controller = (treeType == "course" ? "Lessons" : "Plans");
            string contentUrl = ActionUrlPrefix + controller + "/" + contentAction + "?treeKey=" + treeKey + "&nodeKey=" + nodeKey
                 + "&contentKey=" + contentKey;
            string studyUrl = ActionUrlPrefix + controller + "/Study" + "?treeKey=" + treeKey + "&nodeKey=" + nodeKey
                 + "&contentKey=" + contentKey + "&sessionIndex=0";
            string studyUrlStudyList = studyUrl + "&toolsource=StudyList";
            //string studyUrlVocabularyList = studyUrl + "&toolsource=VocabularyList";

            if ((contentNode != null) && (node != contentNode))
            {
                string nodeType = (contentNode.HasChildren() ? "group" : "lesson");
                string nodeTip = nodeUtilities.LanguageUtilities.TranslateUIString(nodeType);
                string nodeName = Filter(contentNode, contentNode.Title, nodeUtilities, nodeTip);
                contentName = nodeName + " " + contentName;
            }

            sb.Append("	                ['" + contentName + "', ['javascript:jt_main_body_goto(\"" + contentUrl + "\")', , '" + imageName + "', '(" + tip + ")'],\n");

            List<BaseObjectContent> contentList = null;
            int subCount = 0;
            int subIndex = 0;

            if (content.ContentCount() != 0)
                contentList = new List<BaseObjectContent>(content.ContentList);

            switch (content.ContentClass)
            {
                case ContentClassType.MediaList:
                    if (content.GetOptionFlag("DescendentMediaPlaceholder", false))
                    {
                        int lastIndex = (contentList != null ? contentList.Count : 0);

                        contentList = content.GetMediaContentDescendents(contentList);

                        for (int index = contentList.Count - 1; index >= lastIndex; index--)
                        {
                            if (contentList[index].GetOptionFlag("DescendentMediaPlaceholder", false))
                                contentList.RemoveAt(index);
                        }
                    }
                    break;
                case ContentClassType.MediaItem:
                    if (content.GetOptionFlag("DescendentMediaPlaceholder", false))
                    {
                        int lastIndex = (contentList != null ? contentList.Count : 0);

                        contentList = content.GetContentDescendents(contentList);

                        for (int index = contentList.Count - 1; index >= lastIndex; index--)
                        {
                            if (contentList[index].GetOptionFlag("DescendentMediaPlaceholder", false))
                                contentList.RemoveAt(index);
                        }
                    }
                    break;
                default:
                    break;
            }

            if (contentList != null)
            {
                FilterContents(contentList, nodeUtilities);
                subCount = contentList.Count;
            }

            if ((content.ContentClass == ContentClassType.StudyList) &&
                    (content.ContentStorageKey > 0) &&
                    ((content.ContentSubType == "Vocabulary") ||
                            (content.ContentSubType == "Text") ||
                            (content.ContentSubType == "Dialog") ||
                            (content.ContentSubType == "Transcript") ||
                            (content.ContentSubType == "Expansion")))
                //subCount += 2;
                subCount += 1;

            if (subCount != 0)
            {
                sb.Append("	                    [\n");

                if ((content.ContentClass == ContentClassType.StudyList) &&
                    (content.ContentStorageKey > 0) &&
                    ((content.ContentSubType == "Vocabulary") ||
                        (content.ContentSubType == "Text") ||
                        (content.ContentSubType == "Dialog") ||
                        (content.ContentSubType == "Transcript") ||
                        (content.ContentSubType == "Expansion")))
                {
                    string studyImageName = "Study";
                    string studyLabel = nodeUtilities.LanguageUtilities.TranslateUIString(content.ContentType + " Study");
                    string studyTip = studyLabel;
                    //string vocabularyStudyLabel = nodeUtilities.LanguageUtilities.TranslateUIString("Vocabulary Study");
                    //string vocabularyStudyTip = vocabularyStudyLabel;
                    sb.Append("	                        ['" + studyLabel + "', ['javascript:jt_main_body_goto(\"" + studyUrlStudyList + "\")', , '" + studyImageName + "', '(" + studyTip + ")']],\n");
                    //sb.Append("	                        ['" + vocabularyStudyLabel + "', ['javascript:jt_main_body_goto(\"" + studyUrlVocabularyList + "\")', , '" + studyImageName + "', '(" + vocabularyStudyTip + ")']]\n");
                    //sb.Append((subCount == 2 ? "" : ",") + "\n");
                    //subIndex += 2;
                    sb.Append((subCount == 1 ? "" : ",") + "\n");
                    subIndex += 1;
                }

                if ((contentList != null) && (contentList.Count != 0))
                {
                    foreach (BaseObjectContent subContent in contentList)
                    {
                        if (subContent.KeyString == content.KeyString)
                            continue;
                        subIndex++;
                        bool isLastItem = (subIndex == subCount ? true : false);
                        AddContent(sb, nodeUtilities, tree, node, subContent, isLastItem, null);
                    }
                }

                sb.Append("	                    ]\n");
            }

            sb.Append("	                ]" + (isLast ? "" : ",") + "\n");
        }

        private void AddSubContent(StringBuilder sb, NodeUtilities nodeUtilities,
            BaseObjectNodeTree tree, BaseObjectNode node, BaseObjectContent parentContent,
            BaseObjectContent content, bool isLast)
        {
            if (content == null)
                return;
            UserRecord userRecord = nodeUtilities.UserRecord;
            if (!node.IsPublic && (node.Owner != userRecord.UserName) && (node.Owner != userRecord.Team) && !userRecord.IsAdministrator())
                return;
            string treeSource = tree.Source;
            string treeType = (treeSource == "Courses" ? "course" : "plan");
            string treeKey = tree.KeyString;
            string nodeType = (node.HasChildren() ? "group" : "lesson");
            string nodeTypeCapitalized = (node.HasChildren() ? "Group" : "Lesson");
            string nodeKey = node.KeyString;
            string contentType = content.ContentType;
            string contentSubType = content.ContentSubType;
            string contentLabel = contentSubType + (!String.IsNullOrEmpty(contentType) ? (!String.IsNullOrEmpty(contentSubType) ? " " : "") + contentType : "");
            string contentKey = parentContent.KeyString;
            string contentSubKey = content.KeyString;
            string tip = nodeUtilities.LanguageUtilities.TranslateUIString(contentLabel);
            string contentName = Filter(content, content.Title, nodeUtilities, tip);
            string imageName = contentType;
            string contentAction = "Content";
            string controller = (treeType == "course" ? "Lessons" : "Plans");
            string contentUrl = ActionUrlPrefix + controller + "/" + contentAction + "?treeKey=" + treeKey + "&nodeKey=" + nodeKey
                 + "&contentKey=" + contentKey + "&contentSubKey=" + contentSubKey;

            sb.Append("					['" + contentName + "', ['javascript:jt_main_body_goto(\"" + contentUrl + "\")', , '" + imageName + "', '(" + tip + ")'],\n");

            sb.Append("			        ]" + (isLast ? "" : ",") + "\n");
        }

        private void AddAutomatedStudy(StringBuilder sb, NodeUtilities nodeUtilities,
            BaseObjectNodeTree tree, BaseObjectNode node, bool isLast,
            MasterMenuItem menuItem)
        {
            string treeSource = tree.Source;
            string treeType = (treeSource == "Courses" ? "course" : "plan");
            string treeKey = tree.KeyString;
            string nodeKey = ((node == null) || node.IsTree() ? "-1" : node.KeyString);
            string controller = (treeType == "course" ? "Lessons" : "Plans");
            string studyUrl = ActionUrlPrefix + controller + "/AutomatedNodeStudy"
                + "?treeKey=" + treeKey + "&nodeKey=" + nodeKey;
            string nodeLabel;
            if (node == null)
                nodeLabel = tree.Label;
            else
                nodeLabel = node.Label;
            string rawStudyLabel = (menuItem != null ? menuItem.Text : nodeLabel + " Study");
            string studyLabel = nodeUtilities.LanguageUtilities.TranslateUIString(rawStudyLabel);
            string studyTip = studyLabel;

            string studyImageName = "StudyAutomated";
            sb.Append("	                        ['" + studyLabel + "', ['javascript:jt_main_body_goto(\"" + studyUrl + "\")', , '" + studyImageName + "', '(" + studyTip + ")']\n");
            sb.Append("	                        ]" + (isLast ? "" : ",") + "\n");
        }

        private void FilterContents(List<BaseObjectContent> contents, NodeUtilities nodeUtilities)
        {
            if (contents != null)
            {
                int count = contents.Count;
                int index;
                for (index = count - 1; index >= 0; index--)
                {
                    BaseObjectContent content = contents[index];
                    if (!nodeUtilities.IsShowContent(content))
                        contents.RemoveAt(index);
                    if (!nodeUtilities.ShowEmptyContentItemsInTreeView)
                    {
                        switch (content.ContentStorageState)
                        {
                            case ContentStorageStateCode.Empty:
                                contents.RemoveAt(index);
                                break;
                            case ContentStorageStateCode.Unknown:
                            case ContentStorageStateCode.NotEmpty:
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void FilterNodes(List<BaseObjectNode> nodes, NodeUtilities nodeUtilities)
        {
            if (nodes != null)
            {
                int count = nodes.Count;
                int index;
                for (index = count - 1; index >= 0; index--)
                {
                    BaseObjectNode node = nodes[index];
                    if (!nodeUtilities.ShowEmptyContentItemsInTreeView)
                    {
                        switch (node.ContentStorageState)
                        {
                            case ContentStorageStateCode.Empty:
                                nodes.RemoveAt(index);
                                break;
                            case ContentStorageStateCode.Unknown:
                            case ContentStorageStateCode.NotEmpty:
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private string Filter(
            BaseObjectLanguages src,
            MultiLanguageString mls,
            NodeUtilities nodeUtilities,
            string fallback)
        {
            if (mls == null)
                return fallback;

            string str = mls.Text(nodeUtilities.UILanguageID);

            if (String.IsNullOrEmpty(str))
            {
                str = mls.TextFuzzy(nodeUtilities.UILanguageID);

                if (String.IsNullOrEmpty(str) && (mls.Count() != 0))
                {
                    if ((src != null) && src.HasLanguage(nodeUtilities.UILanguageID)
                        && (nodeUtilities.Translator != null))
                    {
                        string baseText = mls.Text(LanguageLookup.English);
                        LanguageID baseLanguageID = null;

                        if (!String.IsNullOrEmpty(baseText))
                            baseLanguageID = LanguageLookup.English;
                        else
                        {
                            foreach (LanguageString ls in mls.LanguageStrings)
                            {
                                if (ls.HasText())
                                {
                                    baseText = ls.Text;
                                    baseLanguageID = ls.LanguageID;
                                    break;
                                }
                            }
                        }

                        if (baseLanguageID != null)
                        {
                            LanguageTranslatorSource translatorSource;
                            string errorMessage;

                            if (nodeUtilities.Translator.TranslateString(
                                "UITranslation",
                                null,
                                null,
                                baseText, 
                                baseLanguageID,
                                nodeUtilities.UILanguageID,
                                out str,
                                out translatorSource,
                                out errorMessage))
                            {
                                LanguageString uiLS = mls.LanguageString(nodeUtilities.UILanguageID);

                                if (uiLS == null)
                                {
                                    uiLS = new LanguageString(mls.Key, nodeUtilities.UILanguageID, str);
                                    mls.Add(uiLS);
                                }
                                else
                                    uiLS.Text = str;
                            }
                        }
                    }
                    else
                        str = mls.LanguageStrings.First().Text;
                }
            }

            if (String.IsNullOrEmpty(str))
                return fallback;

            str = TextUtilities.JavascriptEncode(str);

            return str;
        }

        public virtual List<string> FontNames()
        {
            return new List<string>();
        }

        public static MediaStorageState GetStorageStateFromString(string str)
        {
            MediaStorageState storageState;

            switch (str)
            {
                case "Unknown":
                    storageState = MediaStorageState.Unknown;
                    break;
                case "Present":
                    storageState = MediaStorageState.Present;
                    break;
                case "Downloaded":
                    storageState = MediaStorageState.Downloaded;
                    break;
                case "Absent":
                    storageState = MediaStorageState.Absent;
                    break;
                case "External":
                    storageState = MediaStorageState.External;
                    break;
                case "BadLink":
                    storageState = MediaStorageState.BadLink;
                    break;
                default:
                    throw new Exception("ApplicationData.GetStorageStateFromString: Unknown storage state: "
                        + str);
            }

            return storageState;
        }

        public virtual string GetNormalizedString(string str)
        {
            str = TextUtilities.GetGenericNormalizedString(str);
            return str;
        }

        public virtual string GetUnaccentedWord(string str)
        {
            // Can't do this in portable library.
            return str;
        }

        public virtual XElement ParseXhtml(string html)
        {
            return null;
        }

        public virtual MessageBox.MessageResult ShowMessageBox(
            string Title,
            string Message,
            bool SetCancelable = false,
            bool SetInverseBackgroundForced = false,
            MessageBox.MessageResult PositiveButton = MessageBox.MessageResult.OK,
            MessageBox.MessageResult NegativeButton = MessageBox.MessageResult.NONE,
            MessageBox.MessageResult NeutralButton = MessageBox.MessageResult.NONE,
            int IconAttribute = 0)
        {
            return MessageBox.MessageResult.NONE;
        }

        // User options - alive only for app instance.


        public List<BaseString> UserOptions
        {
            get
            {
                return _UserOptions;
            }
            set
            {
                if (value != _UserOptions)
                {
                    if (value != null)
                        _UserOptions = value;
                    else
                        _UserOptions = new List<BaseString>();
                }
            }
        }

        private string ComposeOptionKey(string userName, string optionName)
        {
            string key = userName + "_" + optionName;
            return key;
        }

        public bool HasUserOption(string userName, string optionName)
        {
            string key = ComposeOptionKey(userName, optionName);

            if (_UserOptions != null)
                return _UserOptions.FirstOrDefault(x => x.KeyString == key) != null;

            return false;
        }

        public BaseString GetUserOption(string userName, string optionName)
        {
            string key = ComposeOptionKey(userName, optionName);

            if (_UserOptions != null)
                return _UserOptions.FirstOrDefault(x => x.KeyString == key);

            return null;
        }

        public void SetUserOption(BaseString option)
        {
            if (_UserOptions == null)
                _UserOptions = new List<BaseString>(1) { option };
            else
            {
                BaseString oldUserOption = _UserOptions.FirstOrDefault(x => x.KeyString == option.KeyString);
                if (oldUserOption == null)
                {
                    _UserOptions.Add(option);
                }
                else if (!String.IsNullOrEmpty(option.Text))
                {
                    if (oldUserOption.Text != option.Text)
                    {
                        oldUserOption.Text = option.Text;
                    }
                }
                else
                {
                    _UserOptions.Remove(oldUserOption);
                }
            }
        }

        public string GetUserOptionString(string userName, string optionName, string defaultValue = null)
        {
            BaseString option = GetUserOption(userName, optionName);
            if ((option != null) && (option.Text != null))
                return option.Text;
            if (defaultValue == null)
                return String.Empty;
            return defaultValue;
        }

        public void SetUserOptionString(string userName, string optionName, string value)
        {
            string key = ComposeOptionKey(userName, optionName);

            if (_UserOptions == null)
            {
                _UserOptions = new List<BaseString>(1) { new BaseString(key, value) };
            }
            else
            {
                BaseString oldUserOption = _UserOptions.FirstOrDefault(x => x.KeyString == key);
                if (oldUserOption == null)
                {
                    _UserOptions.Add(new BaseString(key, value));
                }
                else
                {
                    if (oldUserOption.Text != value)
                    {
                        oldUserOption.Text = value;
                    }
                }
            }
        }

        public int GetUserOptionInteger(string userName, string optionName, int defaultValue = 0)
        {
            string stringValue = GetUserOptionString(userName, optionName);
            int integerValue = defaultValue;

            if (ObjectUtilities.IsNumberString(stringValue))
            {
                try
                {
                    integerValue = Convert.ToInt32(stringValue);
                }
                catch
                {
                }
            }

            return integerValue;
        }

        public void SetUserOptionInteger(string userName, string optionName, int value)
        {
            SetUserOptionString(userName, optionName, value.ToString());
        }

        public float GetUserOptionFloat(string userName, string optionName, float defaultValue = 0)
        {
            string stringValue = GetUserOptionString(userName, optionName);
            float floatValue = defaultValue;

            if (ObjectUtilities.IsFloatString(stringValue))
            {
                try
                {
                    floatValue = (float)Convert.ToDouble(stringValue);
                }
                catch
                {
                }
            }

            return floatValue;
        }

        public void SetUserOptionFloat(string userName, string optionName, float value)
        {
            SetUserOptionString(userName, optionName, value.ToString());
        }

        public bool GetUserOptionFlag(string userName, string optionName, bool defaultValue = false)
        {
            switch (GetUserOptionString(userName, optionName).ToLower())
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return defaultValue;
            }
        }

        public void SetUserOptionFlag(string userName, string optionName, bool value)
        {
            SetUserOptionString(userName, optionName, (value ? "true" : "false"));
        }

        public LanguageID GetUserOptionLanguageID(string userName, string optionName, LanguageID defaultValue = null)
        {
            BaseString option = GetUserOption(userName, optionName);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return LanguageLookup.GetLanguageIDNoAdd(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionStringList(string userName, string optionName, List<string> value)
        {
            SetUserOptionString(userName, optionName, (value != null ? TextUtilities.GetStringFromStringList(value) : String.Empty));
        }

        public List<string> GetUserOptionStringList(string userName, string optionName, List<string> defaultValue = null)
        {
            BaseString option = GetUserOption(userName, optionName);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetStringListFromString(oldValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionStringDictionary(string userName, string optionName, Dictionary<string, string> value)
        {
            SetUserOptionString(userName, optionName, (value != null ? TextUtilities.GetStringFromStringDictionary(value) : String.Empty));
        }

        public Dictionary<string, string> GetUserOptionStringDictionary(string userName, string optionName, Dictionary<string, string> defaultValue = null)
        {
            BaseString option = GetUserOption(userName, optionName);
            if (option != null)
            {
                string oldValue = option.Text;
                if (!String.IsNullOrEmpty(oldValue))
                    return TextUtilities.GetStringDictionaryFromString(oldValue, defaultValue);
                return null;
            }
            return defaultValue;
        }

        public void SetUserOptionLanguageID(string userName, string optionName, LanguageID value)
        {
            SetUserOptionString(userName, optionName, (value != null ? value.LanguageCultureExtensionCode : String.Empty));
        }

        public bool DeleteUserOption(string userName, string optionName)
        {
            if (_UserOptions == null)
                return false;

            BaseString option = GetUserOption(userName, optionName);

            if (option == null)
                return false;

            return _UserOptions.Remove(option);
        }

        public void DeleteAllUserOptions()
        {
            _UserOptions = null;
        }

        // Background operation stuff.  Only alive for instance.

        public void SetCanceled(string userName, string operationName, string message)
        {
            SetOperationStatus(userName, operationName, "Canceled", message);
        }

        public void ClearCanceled(string userName, string operationName)
        {
            ClearOperationStatus(userName, operationName);
        }

        public bool IsCanceled(string userName, string operationName)
        {
            string state;
            string statusLabel;
            GetOperationStatus(userName, operationName, out state, out statusLabel);
            if (state == "Canceled")
                return true;
            return false;
        }

        public string GetCanceledMessage(string userName, string operationName)
        {
            return GetOperationStatusLabel(userName, operationName);
        }

        public void GetOperationStatus(string userName, string operationName, out string state, out string statusLabel)
        {
            state = GetUserOptionString(userName, operationName + "OperationStatusState");
            statusLabel = GetUserOptionString(userName, operationName + "OperationStatusLabel");
        }

        public void SetOperationStatus(string userName, string operationName, string state, string statusLabel)
        {
            SetUserOptionString(userName, operationName + "OperationStatusState", state);
            SetUserOptionString(userName, operationName + "OperationStatusLabel", statusLabel);
        }

        public string GetOperationStatusLabel(string userName, string operationName)
        {
            return GetUserOptionString(userName, operationName + "OperationStatusLabel");
        }

        public void SetOperationStatusLabel(string userName, string operationName, string statusLabel)
        {
            SetUserOptionString(userName, operationName + "OperationStatusLabel", statusLabel);
        }

        public string GetOperationStatusState(string userName, string operationName)
        {
            return GetUserOptionString(userName, operationName + "OperationStatusState");
        }

        public void SetOperationStatusState(string userName, string operationName, string state)
        {
            SetUserOptionString(userName, operationName + "OperationStatusState", state);
        }

        public void ClearOperationStatus(string userName, string operationName)
        {
            DeleteUserOption(userName, operationName + "OperationStatusState");
            DeleteUserOption(userName, operationName + "OperationStatusLabel");
        }

        public virtual byte[] ReadPersonalFile(string fileName)
        {
            throw new NotImplementedException("ReadPersonalFile not implemented in ApplicationData derived class.");
        }

        public virtual void WritePersonalFile(string fileName, byte[] data)
        {
            throw new NotImplementedException("WritePersonalFile not implemented in ApplicationData derived class.");
        }

        public virtual string GetStringFromBytesUTF8(byte[] bytes)
        {
            throw new NotImplementedException("GetStringFromBytesUTF8 not implemented in ApplicationData derived class.");
        }

        public virtual byte[] GetBytesFromStringUTF8(string str)
        {
            throw new NotImplementedException("GetBytesFromStringUTF8 not implemented in ApplicationData derived class.");
        }

        public virtual byte[] GetPreambleUTF8()
        {
            throw new NotImplementedException("GetBytesFromStringUTF8 not implemented in ApplicationData derived class.");
        }

        public virtual string GetStringFromUTF32Number(int num)
        {
            throw new NotImplementedException("GetStringFromUTF32Number not implemented in ApplicationData derived class.");
        }

        public virtual void PutConsoleMessage(string message)
        {
        }

        public virtual void PutConsoleErrorMessage(string message)
        {
            PutConsoleMessage(message);
        }

        public virtual void PutLogError(string action, string message)
        {
            PutConsoleMessage(action + " error: " + message);
        }

        public virtual void PutLogMessage(string action, string message)
        {
            PutConsoleMessage(action + ": " + message);
        }
    }
}
