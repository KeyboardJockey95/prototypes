using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Database;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Dictionary;

namespace JTLanguageModelsPortable.Crawlers
{
    public class Crawler : ControllerUtilities
    {
        public string Name { get; set; }                // Crawler name.
        public string Type { get; set; }                // Format type.
        public FormatCrawler Format { get; set; }       // Format object with arguments.
        public Dictionary<string, bool> BadLinks;
        public SentenceFixes SentenceFixes { get; set; }
        public NodeUtilities NodeUtilities { get; set; }
        public bool AllowStudent { get; set; }
        public bool AllowTeacher { get; set; }
        public bool AllowAdministrator { get; set; }
        public List<BaseObjectNodeTree> Courses;        // Courses imported.

        // Possible arguments.

        public string Title { get; set; }
        public string Description { get; set; }
        public string CourseName { get; set; }
        public string CourseNamePrefix { get; set; }
        public string CourseNameSuffix { get; set; }
        public string CourseLevel { get; set; }
        public List<string> LessonPathPatterns { get; set; }
        public bool SplitLevels { get; set; }
        public bool DeleteBeforeImport { get; set; }    // Delete target contents before import.
        public bool DontDeleteMedia { get; set; }       // Don't delete media when deleting target contents.
        public bool OverwriteMedia { get; set; }        // Overwrite media.
        public string LanguageName { get; set; }
        public LanguageID HostLanguageID;
        public LanguageID TargetLanguageID;
        public LanguageID TargetAlternateLanguageID;
        public LanguageID RomanizationLanguageID;
        public LanguageID NonRomanizationLanguageID;
        public List<LanguageID> HostLanguageIDs;
        public List<LanguageID> TargetLanguageIDs;
        public List<LanguageID> RomanizationLanguageIDs;
        public List<LanguageID> TargetRomanizationHostLanguageIDs;
        public List<LanguageID> HostTargetRomanizationLanguageIDs;
        public List<LanguageID> TargetRomanizationLanguageIDs;
        public List<LanguageID> LanguageIDs;
        public List<LanguageDescriptor> LanguageDescriptors;
        public List<LanguageDescription> LanguageDescriptions;
        public string Owner { get; set; }
        public bool IsPublic { get; set; }
        public string Package { get; set; }
        public bool UniqueContent { get; set; }
        public static List<string> MediaModes = new List<string>() { "Local", "Download", "Remote", "None" };
        public string AudioMode { get; set; }
        public string VideoMode { get; set; }
        public string PictureMode { get; set; }
        public string DocumentMode { get; set; }
        public bool IsLookupDictionaryAudio { get; set; }
        public bool IsLookupDictionaryPictures { get; set; }
        public bool IsForceAudio { get; set; }
        public bool IsSynthesizeMissingAudio { get; set; }
        public bool IsTranslateMissingItems { get; set; }
        public bool IsAddNewItemsToDictionary { get; set; }
        public bool IsParagraphsOnly { get; set; }
        public bool IsFallbackToParagraphs { get; set; }
        public bool IsExcludePrior { get; set; }
        public bool IsIgnoreImages { get; set; }
        public bool UsePicturesAsAnnotations { get; set; }
        public bool UseHeadingsAsText { get; set; }
        public bool UseMediaList { get; set; }
        public bool UseGenericMasters { get; set; }
        public bool CacheHtml { get; set; }
        public bool ForceHtmlCacheUpdate { get; set; }
        public bool UpdateHtmlCacheOnly { get; set; }
        public bool Verbose { get; set; }
        public string OutputFilePath { get; set; }
        public string MediaFilePath { get; set; }
        public string MediaManifestFile { get; set; }
        public string WebUserName { get; set; }
        public string WebPassword { get; set; }
        public string TitlePrefix { get; set; }
        public string DefaultContentType { get; set; }
        public string DefaultContentSubType { get; set; }
        public string Label { get; set; }
        public string MasterName { get; set; }
        public bool SubDivide { get; set; }
        public bool SubDivideToStudyListsOnly { get; set; }
        public int StudyItemSubDivideCount { get; set; }
        public int MinorSubDivideCount { get; set; }
        public int MajorSubDivideCount { get; set; }
        public bool ExtractText { get; set; }
        public bool ExtractSentences { get; set; }
        public bool ExtractWords { get; set; }
        public bool ExtractCharacters { get; set; }
        public string ElementXPath { get; set; }
        public string CachePath { get; set; }
        public LanguageID VoiceLanguageID { get; set; }
        public string VoiceName { get; set; }
        public int VoiceSpeed { get; set; }
        public double VoicePauseSeconds { get; set; }
        public bool DoSentenceFixes { get; set; }
        public string SentenceFixesKey { get; set; }
        public bool DoWordFixes { get; set; }
        public string WordFixesKey { get; set; }

        // Site information.

        public string SiteName { get; set; }
        public string SiteDomain { get; set; }
        public string SiteRootUrl { get; set; }
        public string SiteMainUrl { get; set; }
        public string SiteLoginUrl { get; set; }
        public string SiteLoginReferer { get; set; }
        public string SiteLoginPostUrl { get; set; }
        public string SiteAccountName { get; set; }
        public string SiteAccountPassword { get; set; }
        public string SiteLoginPostFormat { get; set; }
        public string PHPSESSID { get; set; }
        public string SiteNode { get; set; }
        public string SiteSubNode { get; set; }

        // Implementation data.

        protected int CourseOrdinal = 0;
        protected int GroupOrdinal = 0;
        protected int LessonOrdinal = 0;
        protected NodeTreeRepository CourseRepository;
        protected BaseObjectNodeTree Course;
        protected ConvertTransliterate CharacterConverter;
        protected ConvertTransliterate RomanizationConverter;
        protected IMainRepository Repository;
        protected LanguageTool Tool;
        protected MultiLanguageTool MultiTool;
        protected int kMaxNodeLength = 50;
        protected int kMaxPathLength = 256;
        public string UserMediaTildeUrl;
        protected List<ObjectReferenceNodeTree> CourseHeaders = null;
        protected Dictionary<string, List<string>> OriginalMediaMap = null;
        protected int CrawlerSectionCount;
        protected int CrawlerSectionIndex;
        protected int CrawlerTargetCount;
        protected int CrawlerProgressCount;
        public string SourceType;
        public NodeMaster Master { get; set; }
        protected BaseObjectNode Group;
        protected BaseObjectNode Lesson;
        protected BaseObjectContent Content;
        protected BaseObjectContent GroupTextContent;
        protected BaseObjectContent TextContent;
        protected BaseObjectContent SentencesContent;
        protected BaseObjectContent WordsContent;
        protected BaseObjectContent CharactersContent;
        protected BaseObjectContent AudioContent;
        protected ContentStudyList GroupTextStudyList;
        protected ContentStudyList TextStudyList;
        protected ContentStudyList SentencesStudyList;
        protected ContentStudyList WordsStudyList;
        protected ContentStudyList CharactersStudyList;
        protected ContentStudyList TargetStudyList;
        protected List<string> TextLines;
        protected ContentMediaItem AudioMediaItem;
        protected LanguageMediaItem AudioLanguageMediaItem;
        protected StudyItemCache PriorStudyItemCache;

        public Crawler(string name, string type, FormatCrawler format)
        {
            ClearCrawler();
            Name = name;
            Type = type;
            Format = format;
        }

        public Crawler(Crawler other)
        {
            CopyCrawler(other);
        }

        public Crawler()
        {
            ClearCrawler();
        }

        public void ClearCrawler()
        {
            Name = null;
            Type = null;
            Format = null;
            BadLinks = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            SentenceFixes = null;
            NodeUtilities = null;
            AllowStudent = true;
            AllowTeacher = true;
            AllowAdministrator = true;
            Courses = new List<BaseObjectNodeTree>();

            // Possible arguments.

            Title = String.Empty;
            Description = String.Empty;
            CourseName = String.Empty;
            CourseNamePrefix = String.Empty;
            CourseNameSuffix = String.Empty;
            CourseLevel = "None";
            LessonPathPatterns = new List<string>();
            SplitLevels = false;
            DeleteBeforeImport = false;
            DontDeleteMedia = false;
            OverwriteMedia = false;
            LanguageName = String.Empty;
            UILanguageID = null;            // Inherited.
            HostLanguageID = null;
            TargetLanguageID = null;
            TargetAlternateLanguageID = null;
            RomanizationLanguageID = null;
            HostLanguageIDs = null;
            TargetLanguageIDs = null;
            RomanizationLanguageIDs = null;
            TargetRomanizationHostLanguageIDs = null;
            HostTargetRomanizationLanguageIDs = null;
            TargetRomanizationLanguageIDs = null;
            LanguageIDs = null;
            LanguageDescriptions = null;
            Owner = String.Empty;
            IsPublic = false;
            Package = String.Empty;
            UniqueContent = false;
            AudioMode = "Local";
            VideoMode = "Local";
            PictureMode = "Local";
            DocumentMode = "Local";
            IsTranslateMissingItems = false;
            IsAddNewItemsToDictionary = false;
            IsLookupDictionaryAudio = false;
            IsLookupDictionaryPictures = false;
            IsForceAudio = false;
            IsSynthesizeMissingAudio = false;
            IsParagraphsOnly = false;
            IsFallbackToParagraphs = false;
            IsExcludePrior = false;
            IsIgnoreImages = false;
            UsePicturesAsAnnotations = false;
            UseHeadingsAsText = true;
            UseMediaList = false;
            UseGenericMasters = true;
            CacheHtml = true;
            ForceHtmlCacheUpdate = false;
            UpdateHtmlCacheOnly = false;
            Verbose = true;
            OutputFilePath = String.Empty;
            LogFilePath = ComposeCrawlerDataFilePath(SiteName, SiteName + "_CrawlerLog.txt");
            MediaFilePath = String.Empty;
            MediaManifestFile = String.Empty;
            WebUserName = String.Empty;
            WebPassword = String.Empty;
            TitlePrefix = "Default";
            DefaultContentType = "Words";
            DefaultContentSubType = "Vocabulary";
            Label = "Words";
            MasterName = "";
            Master = null;
            SubDivide = false;
            SubDivideToStudyListsOnly = true;
            StudyItemSubDivideCount = 20;
            MinorSubDivideCount = 5;
            MajorSubDivideCount = 5;
            ExtractText = false;
            ExtractSentences = false;
            ExtractWords = false;
            ExtractCharacters = false;
            ElementXPath = String.Empty;
            CachePath = null;
            VoiceLanguageID = null;
            VoiceName = null;
            VoiceSpeed = 0;
            VoicePauseSeconds = 1.0;
            DoSentenceFixes = false;
            SentenceFixesKey = null;
            DoWordFixes = false;
            WordFixesKey = null;

            SaveLog = true;     // Inherited.

            // Site information.

            SiteName = String.Empty;
            SiteDomain  = String.Empty;
            SiteRootUrl  = String.Empty;
            SiteMainUrl  = String.Empty;
            SiteLoginUrl  = String.Empty;
            SiteLoginReferer  = String.Empty;
            SiteLoginPostUrl  = String.Empty;
            SiteAccountName  = String.Empty;
            SiteAccountPassword  = String.Empty;
            SiteLoginPostFormat  = String.Empty;
            PHPSESSID = String.Empty;
            SiteNode = String.Empty;
            SiteSubNode = String.Empty;

            // Implementation data.

            CourseOrdinal = 0;
            GroupOrdinal = 0;
            LessonOrdinal = 0;
            CourseRepository = null;
            Course = null;
            CharacterConverter = null;
            RomanizationConverter = null;
            Repository = null;
            Tool = null;
            MultiTool = null;
            kMaxNodeLength = 50;
            kMaxPathLength = 256;
            UserMediaTildeUrl = null;
            CourseHeaders = null;
            OriginalMediaMap = null;
            CrawlerSectionCount = 0;
            CrawlerSectionIndex = 0;
            CrawlerTargetCount = 0;
            CrawlerProgressCount = 0;
            SourceType = null;
            Master = null;
            Group = null;
            Lesson = null;
            Content = null;
            GroupTextContent = null;
            TextContent = null;
            SentencesContent = null;
            WordsContent = null;
            CharactersContent = null;
            AudioContent = null;
            GroupTextStudyList = null;
            TextStudyList = null;
            SentencesStudyList = null;
            WordsStudyList = null;
            CharactersStudyList = null;
            TargetStudyList = null;
            TextLines = null;
            AudioMediaItem = null;
            AudioLanguageMediaItem = null;
            PriorStudyItemCache = null;
        }

        public void CopyCrawler(Crawler other)
        {
            Name = other.Name;
            Type = other.Type;
            Format = other.Format;
            BadLinks = new Dictionary<string, bool>();
            SentenceFixes = null;
            NodeUtilities = null;
            AllowStudent = other.AllowStudent;
            AllowTeacher = other.AllowTeacher;
            AllowAdministrator = other.AllowAdministrator;
            Courses = new List<BaseObjectNodeTree>();

            // Possible arguments.

            Title = other.Title;
            Description = other.Description;
            CourseName = other.CourseName;
            CourseNamePrefix = other.CourseNamePrefix;
            CourseNameSuffix = other.CourseNameSuffix;
            LessonPathPatterns = other.LessonPathPatterns;
            SplitLevels = other.SplitLevels;
            DeleteBeforeImport = other.DeleteBeforeImport;
            DontDeleteMedia = other.DontDeleteMedia;
            OverwriteMedia = other.OverwriteMedia;
            LanguageName = other.LanguageName;
            UILanguageID = other.UILanguageID;            // Inherited.
            HostLanguageID = other.HostLanguageID;
            TargetLanguageID = other.TargetLanguageID;
            TargetAlternateLanguageID = other.TargetAlternateLanguageID;
            RomanizationLanguageID = other.RomanizationLanguageID;
            NonRomanizationLanguageID = other.NonRomanizationLanguageID;
            HostLanguageIDs = LanguageID.CopyList(other.HostLanguageIDs);
            TargetLanguageIDs = LanguageID.CopyList(other.TargetLanguageIDs);
            RomanizationLanguageIDs = LanguageID.CopyList(other.RomanizationLanguageIDs);
            TargetRomanizationHostLanguageIDs = LanguageID.CopyList(other.TargetRomanizationHostLanguageIDs);
            HostTargetRomanizationLanguageIDs = LanguageID.CopyList(other.HostTargetRomanizationLanguageIDs);
            TargetRomanizationLanguageIDs = LanguageID.CopyList(other.TargetRomanizationLanguageIDs);
            LanguageIDs = LanguageID.CopyList(other.LanguageIDs);
            LanguageDescriptions = LanguageDescription.CopyList(other.LanguageDescriptions);
            Owner = other.Owner;
            IsPublic = other.IsPublic;
            Package = other.Package;
            UniqueContent = other.UniqueContent;
            AudioMode = other.AudioMode;
            VideoMode = other.VideoMode;
            PictureMode = other.PictureMode;
            DocumentMode = other.DocumentMode;
            IsTranslateMissingItems = other.IsTranslateMissingItems;
            IsAddNewItemsToDictionary = other.IsAddNewItemsToDictionary;
            IsLookupDictionaryAudio = other.IsLookupDictionaryAudio;
            IsLookupDictionaryPictures = other.IsLookupDictionaryPictures;
            IsForceAudio = other.IsForceAudio;
            IsSynthesizeMissingAudio = other.IsSynthesizeMissingAudio;
            IsParagraphsOnly = other.IsParagraphsOnly;
            IsFallbackToParagraphs = other.IsFallbackToParagraphs;
            IsExcludePrior = other.IsExcludePrior;
            IsIgnoreImages = other.IsIgnoreImages;
            UsePicturesAsAnnotations = other.UsePicturesAsAnnotations;
            UseHeadingsAsText = other.UseHeadingsAsText;
            UseMediaList = other.UseMediaList;
            UseGenericMasters = other.UseGenericMasters;
            CacheHtml = other.CacheHtml;
            ForceHtmlCacheUpdate = other.ForceHtmlCacheUpdate;
            UpdateHtmlCacheOnly = other.UpdateHtmlCacheOnly;
            Verbose = other.Verbose;
            OutputFilePath = other.OutputFilePath;
            MediaFilePath = other.MediaFilePath;
            MediaManifestFile = other.MediaManifestFile;
            WebUserName = other.WebUserName;
            WebPassword = other.WebPassword;
            TitlePrefix = other.TitlePrefix;
            DefaultContentType = other.DefaultContentType;
            DefaultContentSubType = other.DefaultContentSubType;
            Label = other.Label;
            MasterName = other.MasterName;
            Master = other.Master;
            SubDivide = other.SubDivide;
            SubDivideToStudyListsOnly = other.SubDivideToStudyListsOnly;
            StudyItemSubDivideCount = other.StudyItemSubDivideCount;
            MinorSubDivideCount = other.MinorSubDivideCount;
            MajorSubDivideCount = other.MajorSubDivideCount;
            ExtractText = other.ExtractText;
            ExtractSentences = other.ExtractSentences;
            ExtractWords = other.ExtractWords;
            ExtractCharacters = other.ExtractCharacters;
            ElementXPath = other.ElementXPath;
            CachePath = other.CachePath;
            VoiceLanguageID = other.VoiceLanguageID;
            VoiceName = other.VoiceName;
            VoiceSpeed = other.VoiceSpeed;
            VoicePauseSeconds = other.VoicePauseSeconds;
            DoSentenceFixes = other.DoSentenceFixes;
            SentenceFixesKey = other.SentenceFixesKey;
            DoWordFixes = other.DoWordFixes;
            WordFixesKey = other.WordFixesKey;

            // Site information.

            SiteName = other.SiteName;
            SiteDomain = other.SiteDomain;
            SiteRootUrl = other.SiteRootUrl;
            SiteMainUrl = other.SiteMainUrl;
            SiteLoginUrl = other.SiteLoginUrl;
            SiteLoginReferer = other.SiteLoginReferer;
            SiteLoginPostUrl = other.SiteLoginPostUrl;
            SiteAccountName = other.SiteAccountName;
            SiteAccountPassword = other.SiteAccountPassword;
            SiteLoginPostFormat = other.SiteLoginPostFormat;
            PHPSESSID = other.PHPSESSID;
            SiteNode = other.SiteNode;
            SiteSubNode = other.SiteSubNode;

            // Implementation data.

            CourseOrdinal = other.CourseOrdinal;
            GroupOrdinal = other.GroupOrdinal;
            LessonOrdinal = other.LessonOrdinal;
            CourseRepository = other.CourseRepository;
            Course = other.Course;
            CharacterConverter = other.CharacterConverter;
            RomanizationConverter = other.RomanizationConverter;
            Repository = other.Repository;
            kMaxNodeLength = other.kMaxNodeLength;
            kMaxPathLength = other.kMaxPathLength;
            UserMediaTildeUrl = other.UserMediaTildeUrl;
            CourseHeaders = other.CourseHeaders;
            OriginalMediaMap = other.OriginalMediaMap;
            CrawlerSectionCount = other.CrawlerSectionCount;
            CrawlerSectionIndex = other.CrawlerSectionIndex;
            CrawlerTargetCount = other.CrawlerTargetCount;
            CrawlerProgressCount = other.CrawlerProgressCount;
            SourceType = null;
            Master = null;
            Group = null;
            Lesson = null;
            Content = null;
            GroupTextContent = null;
            TextContent = null;
            SentencesContent = null;
            WordsContent = null;
            CharactersContent = null;
            AudioContent = null;
            GroupTextStudyList = null;
            TextStudyList = null;
            SentencesStudyList = null;
            WordsStudyList = null;
            CharactersStudyList = null;
            TargetStudyList = null;
            TextLines = null;
            AudioMediaItem = null;
            AudioLanguageMediaItem = null;
            PriorStudyItemCache = null;
        }

        public virtual Crawler Clone()
        {
            return new Crawler(this);
        }

        public static string ComposeCrawlerDataFilePath(string subDirectory, string fileName)
        {
            string path = ApplicationData.CrawlPath;

            if (!String.IsNullOrEmpty(subDirectory))
                path = MediaUtilities.ConcatenateFilePath(path, subDirectory);

            if (!String.IsNullOrEmpty(fileName))
                path = MediaUtilities.ConcatenateFilePath(path, fileName);

            return path;
        }

        public virtual string GetNodePathFromUrl(string url)
        {
            string nodePath = FileFriendlyName(MediaUtilities.StripHostOrTildeFromUrl(url));
            return nodePath;
        }

        public virtual string GetCachedPath(string nodeName, LanguageID languageID)
        {
            string cachePath = CachePath;
            if (string.IsNullOrEmpty(cachePath))
                cachePath = ComposeCrawlerDataFilePath(SiteName, null);
            string path;
            if (languageID != null)
                path = MediaUtilities.ConcatenateFilePath(cachePath, languageID.LanguageCode);
            else
                path = cachePath;
            return MediaUtilities.ConcatenateFilePath(path, nodeName + ".html");
        }

        public virtual string GetCachedPath(string nodeName, string subNode, string fileExtension, LanguageID languageID)
        {
            string cachePath = CachePath;
            if (string.IsNullOrEmpty(cachePath))
                cachePath = ComposeCrawlerDataFilePath(SiteName, null);
            string path;
            if (languageID != null)
                path = MediaUtilities.ConcatenateFilePath(cachePath, languageID.LanguageCode);
            else
                path = cachePath;
            if (!String.IsNullOrEmpty(subNode))
                path = MediaUtilities.ConcatenateFilePath(path, subNode);
            path = MediaUtilities.ConcatenateFilePath(path, nodeName + fileExtension);
            return path;
        }

        public virtual bool GetPossiblyCachedMediaFile(
            string label,
            string sourceUrl,
            string destFilePath,
            LanguageID languageID,
            string subNode)
        {
            string partialPath = MediaUtilities.RemoveFileExtension(MediaUtilities.StripHostOrTildeFromUrl(sourceUrl));
            string fileName = MediaUtilities.GetBaseFileName(destFilePath);

            int offset = fileName.LastIndexOf('_');

            if (offset != -1)
                fileName = fileName.Substring(0, offset);

            string fileTag = fileName.Replace(".", "_");
            string cacheNode = fileTag + partialPath.Replace("/", "_");
            string fileExtension = MediaUtilities.GetFileExtension(destFilePath);
            string cachePath = GetCachedPath(cacheNode, subNode, fileExtension, languageID);

            if (cachePath.Length > MediaUtilities.MaxMediaFilePathLength)
            {
                cacheNode = partialPath.Replace("/", "_");
                cachePath = GetCachedPath(cacheNode, subNode, fileExtension, languageID);
            }

            if (FileSingleton.Exists(cachePath))
                return CopyMediaFile(label, destFilePath, cachePath);

            bool returnValue = GetMediaFile(label, sourceUrl, destFilePath);

            CopyMediaFile(label, cachePath, destFilePath);

            return returnValue;
        }

        public virtual bool PermissionsCheck(UserRecord userRecord, ref string errorMessage)
        {
            bool returnValue = false;

            switch (userRecord.UserRole)
            {
                case "student":
                    returnValue = AllowStudent;
                    break;
                case "teacher":
                    returnValue = AllowTeacher;
                    break;
                case "administrator":
                    returnValue = AllowAdministrator;
                    break;
                default:
                    returnValue = false;
                    break;
            }

            if (!returnValue)
                errorMessage = "Sorry, you don't have a sufficient role to use this crawler.";

            if (!userRecord.IsAdministrator())
            {
                if (!String.IsNullOrEmpty(Package))
                {
                    if (!userRecord.HavePackage(Package))
                    {
                        errorMessage = "Sorry, you don't have package permission to use this crawler.";
                        returnValue = false;
                    }
                }
            }

            return returnValue;
        }

        public virtual void InitializeSiteInformation()
        {
            LogFilePath = ComposeCrawlerDataFilePath(SiteName, SiteName + "_CrawlerLog.txt");
        }

        public virtual void InitializeFromFormat(FormatCrawler format)
        {
            Format = format;

            Initialize(
                format.Repositories,
                null,                       // Cookies not needed.
                format.UserRecord,
                format.UserProfile,
                null,                       // Translator (will get it from language utilities).
                format.LanguageUtilities);

            TaskName = format.TaskName;
            NodeUtilities = format.NodeUtilities;
            Owner = format.UserRecord.UserName;
            UserMediaTildeUrl = ApplicationData.MediaTildeUrl + "/" + Owner;
            OutputFilePath = ApplicationData.MapToFilePath(UserMediaTildeUrl);
            InitializeLanguagesFromUserProfile();
            InitializeSourceType();
        }

        public void InitializeLanguagesFromUserProfile()
        {
            int index;
            LanguageID languageID;

            UILanguageID = UserProfile.UILanguageID;

            HostLanguageIDs = LanguageID.CopyList(UserProfile.HostLanguageIDs);
            TargetLanguageIDs = LanguageID.CopyList(UserProfile.TargetLanguageIDs);
            RomanizationLanguageIDs = new List<LanguageID>();

            for (index = TargetLanguageIDs.Count() - 1; index >= 0; index--)
            {
                languageID = TargetLanguageIDs[index];

                if (LanguageLookup.IsRomanized(languageID))
                {
                    RomanizationLanguageIDs.Insert(0, languageID);
                    TargetLanguageIDs.RemoveAt(index);
                }
            }

            TargetRomanizationHostLanguageIDs = ObjectUtilities.ListConcatenateUnique<LanguageID>(
                TargetLanguageIDs,
                RomanizationLanguageIDs,
                HostLanguageIDs);

            HostTargetRomanizationLanguageIDs = ObjectUtilities.ListConcatenateUnique<LanguageID>(
                HostLanguageIDs,
                TargetLanguageIDs,
                RomanizationLanguageIDs);

            TargetRomanizationLanguageIDs = ObjectUtilities.ListConcatenateUnique<LanguageID>(
                TargetLanguageIDs,
                RomanizationLanguageIDs);

            InitializeSingleLanguagesFromLanguages();
        }

        public void SwapLanguages()
        {
            int index;
            LanguageID languageID;

            List<LanguageID> targetLanguageIDs = LanguageID.CopyList(HostLanguageIDs);
            HostLanguageIDs = LanguageID.CopyList(TargetLanguageIDs);
            TargetLanguageIDs = targetLanguageIDs;
            RomanizationLanguageIDs = new List<LanguageID>();

            for (index = TargetLanguageIDs.Count() - 1; index >= 0; index--)
            {
                languageID = TargetLanguageIDs[index];

                if (LanguageLookup.IsRomanized(languageID))
                {
                    RomanizationLanguageIDs.Insert(0, languageID);
                    TargetLanguageIDs.RemoveAt(index);
                }
            }

            TargetRomanizationHostLanguageIDs = ObjectUtilities.ListConcatenateUnique<LanguageID>(
                TargetLanguageIDs,
                RomanizationLanguageIDs,
                HostLanguageIDs);

            HostTargetRomanizationLanguageIDs = ObjectUtilities.ListConcatenateUnique<LanguageID>(
                HostLanguageIDs,
                TargetLanguageIDs,
                RomanizationLanguageIDs);

            TargetRomanizationLanguageIDs = ObjectUtilities.ListConcatenateUnique<LanguageID>(
                TargetLanguageIDs,
                RomanizationLanguageIDs);

            InitializeSingleLanguagesFromLanguages();
        }

        public void InitializeSingleLanguagesFromLanguages()
        {
            LanguageIDs = new List<LanguageID>();

            if ((TargetLanguageIDs != null) && (TargetLanguageIDs.Count() != 0))
            {
                LanguageIDs.AddRange(TargetLanguageIDs);
                TargetLanguageID = TargetLanguageIDs[0];

                if (TargetLanguageIDs.Count() > 1)
                    TargetAlternateLanguageID = TargetLanguageIDs[1];
                else
                    TargetAlternateLanguageID = null;

                LanguageName = TargetLanguageID.LanguageName(LanguageLookup.English);

                NonRomanizationLanguageID = TargetLanguageID;

                if ((TargetLanguageIDs.Count() > 1) &&
                        (NonRomanizationLanguageID == LanguageLookup.Japanese) &&
                        (TargetLanguageIDs[1] == LanguageLookup.JapaneseKana))
                    NonRomanizationLanguageID = LanguageLookup.JapaneseKana;

            }
            else
            {
                TargetLanguageID = null;
                TargetAlternateLanguageID = null;
                NonRomanizationLanguageID = null;
            }

            if ((RomanizationLanguageIDs != null) && (RomanizationLanguageIDs.Count() != 0))
            {
                LanguageIDs.AddRange(RomanizationLanguageIDs);

                if (RomanizationLanguageIDs.Count() >= 1)
                {
                    if (RomanizationLanguageIDs.Count >= 2)
                    {
                        RomanizationLanguageID = RomanizationLanguageIDs[1];
                        TargetAlternateLanguageID = RomanizationLanguageIDs[0];
                        RomanizationLanguageIDs.Remove(TargetAlternateLanguageID);
                    }
                    else
                        RomanizationLanguageID = RomanizationLanguageIDs[0];
                }
            }
            else
                RomanizationLanguageID = null;

            if ((HostLanguageIDs != null) && (HostLanguageIDs.Count() != 0))
            {
                LanguageIDs.AddRange(HostLanguageIDs);
                HostLanguageID = HostLanguageIDs[0];

                if (HostLanguageID.ExtensionCode == "def")
                    UILanguageID = TargetLanguageID;
                else
                    UILanguageID = HostLanguageID;
            }
            else
                HostLanguageID = null;

            InitializeLanguageDescriptorsFromSingleLanguages();
            InitializeConverters();
        }

        protected void InitializeLanguageDescriptorsFromSingleLanguages()
        {
            LanguageDescriptors = new List<LanguageDescriptor>();

            if (TargetLanguageID != null)
                LanguageDescriptors.Add(
                    new LanguageDescriptor(
                        "Target",
                        TargetLanguageID,
                        true));

            if (TargetAlternateLanguageID != null)
                LanguageDescriptors.Add(
                    new LanguageDescriptor(
                        "Target",
                        TargetAlternateLanguageID,
                        true));

            if (RomanizationLanguageID != null)
                LanguageDescriptors.Add(
                    new LanguageDescriptor(
                        "Target",
                        RomanizationLanguageID,
                        true));

            if (HostLanguageID != null)
                LanguageDescriptors.Add(
                    new LanguageDescriptor(
                        "Host",
                        HostLanguageID,
                        true));
        }

        public void InitializeConverters()
        {
            if ((TargetLanguageIDs.Count() > 1) && !LanguageLookup.IsRomanized(TargetLanguageIDs[1]))
                CharacterConverter = new ConvertTransliterate(
                    TargetLanguageIDs[0],
                    TargetLanguageIDs[1],
                    '\0',
                    Repositories.Dictionary,
                    true);
            else
                CharacterConverter = null;

            if (RomanizationLanguageIDs.Count() >= 1)
            {
                try
                {
                    RomanizationConverter = new ConvertTransliterate(
                        true,
                        RomanizationLanguageID,
                        NonRomanizationLanguageID,
                        '\0',
                        Repositories.Dictionary,
                        true);
                }
                catch (Exception)
                {
                    RomanizationConverter = null;
                }
            }
            else
                RomanizationConverter = null;
        }

        public virtual void InitializeTool()
        {
            if (TargetLanguageIDs.Count() >= 1)
            {
                LanguageTool hostTool = null;
                if ((HostLanguageIDs != null) && (HostLanguageIDs.Count() != 0))
                {
                    hostTool = ApplicationData.LanguageTools.Create(HostLanguageIDs[0]);
                    if (hostTool == null)
                        hostTool = new GenericLanguageTool(HostLanguageIDs[0], HostLanguageIDs, LanguageIDs);
                }
                if ((TargetLanguageIDs != null) && (TargetLanguageIDs.Count() != 0))
                {
                    Tool = ApplicationData.LanguageTools.Create(TargetLanguageIDs[0]);
                    if (Tool == null)
                        Tool = new GenericLanguageTool(TargetLanguageIDs[0], HostLanguageIDs, LanguageIDs);
                    MultiTool = new MultiLanguageTool(Tool, hostTool);
                    Tool.MultiTool = MultiTool;
                }
            }
            else
            {
                Tool = null;
                MultiTool = null;
            }
        }

        public void SyncLanguages()
        {
            LanguageDescriptions = new List<LanguageDescription>();

            foreach (LanguageID languageID in LanguageIDs)
            {
                LanguageDescription languageDescription;
                string languageCode = languageID.LanguageCultureExtensionCode;

                languageDescription = LanguageLookup.GetLanguageDescription(languageID);

                if (languageDescription != null)
                    LanguageDescriptions.Add(languageDescription);
                else
                    throw new Exception("Missing language description for " + languageCode);
            }

            LanguageLookup.SynchronizeToLanguageDescriptions(LanguageDescriptions);
        }

        protected void InitializeSourceType()
        {
            if (Format.ContentSource != null)
                SourceType = "BaseObjectContent";
            else if (Format.NodeSource != null)
                SourceType = "BaseObjectNode";
            else if (Format.TreeSource != null)
                SourceType = "BaseObjectNodeTree";
            else
                SourceType = "Unknown";
        }

        public void InitializePriorStudyItemCache()
        {
            PriorStudyItemCache = new StudyItemCache();
        }

        public void ClearPriorStudyItemCache()
        {
            PriorStudyItemCache = null;
        }

        public virtual string GetLabledUrl(string label)
        {
            return SiteRootUrl + "/" + label;
        }

        public virtual string GetPathPatternUrl(string pathPattern)
        {
            return SiteRootUrl + "/" + pathPattern;
        }

        public bool DontGetMedia
        {
            get
            {
                if ((AudioMode == "None") && (VideoMode == "None") && (PictureMode == "None") && (DocumentMode == "None"))
                    return true;

                return false;
            }
        }

        public virtual bool CrawlPassage(
            string passageName,
            string passageUrl,
            out string passage,
            out string errorMessage)
        {
            passage = String.Empty;
            errorMessage = null;
            PutError("CrawlPassage not implemented.");
            return false;
        }

        public virtual bool HandleCrawl()
        {
            PutError("HandleCrawl not implemented.");
            return false;
        }

        protected virtual bool DeleteTargetCheck()
        {
            bool returnValue = true;

            if (Format == null)
                return false;

            if (Format.ContentSource != null)
            {
                returnValue = NodeUtilities.DeleteContentContentsHelper(
                    Format.ContentSource, !DontDeleteMedia);

                returnValue = NodeUtilities.UpdateContentHelperNoMessage(Format.ContentSource);
            }
            else if (Format.NodeSource != null)
            {
                returnValue = NodeUtilities.DeleteNodeChildrenAndContentHelper(
                    Format.TreeSource, Format.NodeSource, !DontDeleteMedia);

                if (!NodeUtilities.UpdateTree(Format.TreeSource, false, false))
                    returnValue = false;
            }
            else if (Format.TreeSource != null)
                returnValue = NodeUtilities.DeleteTreeChildrenHelper(
                    Format.TreeSource, !DontDeleteMedia, false);

            if (!returnValue)
                UpdateErrorFromNodeUtilities();

            return returnValue;
        }

        protected bool HandleLanguageMediaItemGenerate(
            BaseObjectContent content,
            ContentMediaItem mediaItem,
            LanguageMediaItem languageMediaItem,
            MarkupTemplate markupTemplate,
            Dictionary<string, string> speakerToVoiceNameMap,
            BaseObjectContent transcriptContent,
            bool useAudio,
            bool usePicture,
            bool isGenerateLocalTranscript,
            bool isGenerateStudyItemsOnly,
            double defaultPauseSeconds,
            int synthesizerSpeed)
        {
            LanguageID targetLanguageID = languageMediaItem.FirstTargetLanguageID;
            LanguageID hostLanguageID = languageMediaItem.FirstHostLanguageID;
            string hostVoiceName = NodeUtilities.GetDefaultVoiceName(hostLanguageID);
            string targetVoiceName = NodeUtilities.GetDefaultVoiceName(targetLanguageID);
            targetLanguageID = LanguageLookup.GetBestVoiceLanguageID(targetLanguageID);
            hostLanguageID = LanguageLookup.GetBestVoiceLanguageID(hostLanguageID);
            List<BaseObjectContent> sourceContentList = mediaItem.SourceContents;
            string generateMediaDirectoryUrl = content.MediaTildeUrl + "/" + "Generate";
            string sharedMediaDirectoryUrl = NodeUtilities.SharedMediaTildeUrl;
            AudioMarkupRenderer markupRenderer = new AudioMarkupRenderer(
                content, Course, Lesson, sourceContentList, markupTemplate,
                UserRecord, UserProfile, targetLanguageID, hostLanguageID, UILanguageID, LanguageUtilities, Repositories,
                hostVoiceName, targetVoiceName, speakerToVoiceNameMap, content, transcriptContent,
                generateMediaDirectoryUrl, sharedMediaDirectoryUrl, useAudio, usePicture,
                isGenerateLocalTranscript, isGenerateStudyItemsOnly);
            markupRenderer.DefaultPauseSeconds = defaultPauseSeconds;
            markupRenderer.DefaultSpeed = synthesizerSpeed;
            bool returnValue = markupRenderer.Generate();
            if (!returnValue)
                Error = Error + markupRenderer.Error + "\n";
            return returnValue;
        }

        protected bool ConvertTextLinesToStudyList(
            ContentStudyList studyList,
            List<string> textLines,
            LanguageID languageID,
            List<LanguageID> languageIDs,
            int studyItemIndex,
            string sectionTitle,
            string sectionNotes,
            bool trimPriorExtra)
        {
            BaseObjectContent content = studyList.Content;
            bool needsSentenceParsing = content.NeedsSentenceParsing;
            bool needsWordParsing = content.NeedsWordParsing;
            int index;
            int count = textLines.Count();
            bool returnValue = true;

            //WriteLog(languageID.LanguageName(UILanguageID) + ": " + studyItemIndex.ToString());

            if (studyItemIndex > studyList.StudyItemCount())
            {
                while (studyList.StudyItemCount() < studyItemIndex)
                {
                    string key = studyList.AllocateStudyItemKey();
                    MultiLanguageItem studyItem = new MultiLanguageItem(key, LanguageDescriptors);
                    studyList.AddStudyItem(studyItem);
                }
            }

            for (index = 0; index < count; index++)
            {
                MultiLanguageItem studyItem = studyList.GetStudyItemIndexed(studyItemIndex + index);
                LanguageItem targetLanguageItem = null;
                string text = textLines[index];

                if (studyItem == null)
                {
                    string key = studyList.AllocateStudyItemKey();
                    studyItem = new MultiLanguageItem(key, LanguageDescriptors);
                    studyList.AddStudyItem(studyItem);
                }
                else
                    ContentUtilities.PrepareMultiLanguageItem(studyItem, String.Empty, LanguageDescriptors);

                //WriteLog(studyItem.KeyString + ": " + "\"" + text + "\"");

                if (!String.IsNullOrEmpty(sectionTitle))
                {
                    Annotation titleAnnotation = studyItem.FindAnnotation("Heading");

                    if (titleAnnotation == null)
                    {
                        titleAnnotation = new Annotation(
                            "Heading",
                            sectionNotes,
                            new MultiLanguageString("Heading", languageID, sectionTitle));
                        studyItem.AddAnnotation(titleAnnotation);
                    }
                    else
                        titleAnnotation.Text.SetText(languageID, sectionTitle);
                }

                sectionTitle = null;

                if (!String.IsNullOrEmpty(sectionNotes))
                {
                    Annotation noteAnnotation = studyItem.FindAnnotation("Note");

                    if (noteAnnotation == null)
                    {
                        noteAnnotation = new Annotation(
                            "Note",
                            sectionNotes,
                            new MultiLanguageString("Note", languageID, sectionNotes));
                        studyItem.AddAnnotation(noteAnnotation);
                    }
                    else
                        noteAnnotation.Text.SetText(languageID, sectionNotes);
                }

                sectionNotes = null;

                targetLanguageItem = studyItem.LanguageItem(languageID);

                if (targetLanguageItem == null)
                    continue;

                List<MediaRun> mediaRuns = null;

                text = PeelAnnotations(studyItem, targetLanguageItem, text, ref mediaRuns);

                targetLanguageItem.Text = text;

                if (IsTranslateMissingItems && (languageID != HostLanguageID))
                {
                    string errorMessage;

                    if (!studyItem.IsEmpty() && !LanguageUtilities.Translator.TranslateMultiLanguageItem(
                            studyItem,
                            LanguageIDs,
                            needsSentenceParsing,
                            needsWordParsing,
                            out errorMessage,
                            false))
                    {
                        PutLogError(errorMessage);
                        returnValue = false;
                    }
                }
                else if (String.IsNullOrEmpty(text) && 
                    (languageID != HostLanguageID) &&
                    (languageIDs.Count() > 1) &&
                    (languageID != TargetLanguageIDs[0]) &&
                    studyItem.HasText(TargetLanguageIDs[0]))
                {
                    string errorMessage;

                    targetLanguageItem.Text = null;

                    if (Tool != null)
                    {
                        if (!Tool.TransliterateLanguageItem(studyItem, targetLanguageItem, false))
                        {
                            PutLogError("Error transliterating study item.");
                            returnValue = false;
                        }
                    }
                    else
                    {
                        if (!LanguageUtilities.Translator.TranslateMultiLanguageItem(
                                studyItem,
                                languageIDs,
                                needsSentenceParsing,
                                needsWordParsing,
                                out errorMessage,
                                false))
                        {
                            PutLogError(errorMessage);
                            returnValue = false;
                        }
                    }
                }
                else if (needsSentenceParsing || needsWordParsing)
                {
                    if ((languageID != HostLanguageID) && (Tool != null))
                        Tool.GetStudyItemLanguageItemSentenceAndWordRuns(studyItem, languageID);
                    else
                        targetLanguageItem.GetSentenceAndWordRuns(Repositories.Dictionary);
                }

                if (IsLookupDictionaryAudio || IsLookupDictionaryPictures || IsSynthesizeMissingAudio || IsForceAudio)
                {
                    bool isLookupDictionaryAudio;
                    switch (content.ContentType)
                    {
                        case "Words":
                        case "Characters":
                            isLookupDictionaryAudio = IsLookupDictionaryAudio;
                            break;
                        default:
                            isLookupDictionaryAudio = false;
                            break;
                    }
                    if (!NodeUtilities.GetMediaForStudyItem(
                            studyItem,
                            languageIDs,
                            AudioMode == "Local",
                            PictureMode == "Local",
                            ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                            isLookupDictionaryAudio,
                            IsSynthesizeMissingAudio,
                            IsForceAudio,
                            IsLookupDictionaryPictures))
                    {
                        //returnValue = false;
                        PutMessage("Ignoring audio generation error.");
                    }
                }

                if (mediaRuns != null)
                {
                    TextRun sentenceRun = targetLanguageItem.GetSentenceRun(0);

                    if (sentenceRun != null)
                        sentenceRun.IntegrateMediaRuns(mediaRuns, true);
                }
            }

            if (trimPriorExtra && (studyList.StudyItemCount() > count + studyItemIndex))
            {
                count += studyItemIndex;

                for (index = studyList.StudyItemCount() - 1; index >= count; index--)
                    studyList.DeleteStudyItemIndexed(index);
            }

            NodeUtilities.UpdateContentHelper(content);

            return returnValue;
        }

        protected void CheckAddTreeNewWordsToDictionary(BaseObjectNodeTree tree)
        {
            NodeUtilities.CheckAddTreeNewWordsToDictionary(
                tree,
                TargetLanguageID,
                TargetRomanizationLanguageIDs,
                HostLanguageIDs,
                UserName,
                true,           // Translate missing dictionary items.
                true);          // Synthesize missing dictionary audio.
        }

        protected void CheckAddNodeNewWordsToDictionary(BaseObjectNode node)
        {
            NodeUtilities.CheckAddNodeNewWordsToDictionary(
                node,
                TargetLanguageID,
                TargetRomanizationLanguageIDs,
                HostLanguageIDs,
                UserName,
                true,           // Translate missing dictionary items.
                true);          // Synthesize missing dictionary audio.
        }

        protected void CheckAddStudyListNewWordsToDictionary(ContentStudyList studyList)
        {
            NodeUtilities.CheckAddStudyListNewWordsToDictionary(
                studyList,
                TargetLanguageID,
                TargetRomanizationLanguageIDs,
                HostLanguageIDs,
                UserName,
                true,           // Translate missing dictionary items.
                true);          // Synthesize missing dictionary audio.
        }

        protected void CheckAddNewWordsToDictionary(List<string> words)
        {
            NodeUtilities.CheckAddNewWordsToDictionary(
                words,
                TargetLanguageID,
                TargetRomanizationLanguageIDs,
                HostLanguageIDs,
                UserName,
                true,           // Translate missing dictionary items.
                true);          // Synthesize missing dictionary audio.
        }

        protected string PeelAnnotations(
            MultiLanguageItem studyItem,
            LanguageItem languageItem,
            string text,
            ref List<MediaRun> mediaRuns)
        {
            string newText = text;
            int startIndex, endIndex;

            while ((startIndex = newText.IndexOf("[<")) != -1)
            {
                endIndex = newText.IndexOf(">]");

                if ((endIndex == -1) || (endIndex <= startIndex))
                    break;

                string elementString = newText.Substring(startIndex + 1, endIndex - startIndex);
                newText = newText.Remove(startIndex, (endIndex - startIndex) + 2);

                PeelAnnotation(studyItem, languageItem, elementString, ref mediaRuns);
            }

            return newText;
        }

        protected bool PeelAnnotation(
            MultiLanguageItem studyItem,
            LanguageItem languageItem,
            string elementString,
            ref List<MediaRun> mediaRuns)
        {
            bool returnValue = true;
            XElement element = ConvertHtmlTextToXElementRaw(elementString);

            if (element != null)
            {
                switch (element.Name.LocalName)
                {
                    case "img":
                        {
                            string value = String.Empty;
                            string normalUrl = String.Empty;
                            string normalFileName = String.Empty;
                            string normalFilePath = String.Empty;
                            string bigUrl = String.Empty;
                            string bigFileName = String.Empty;
                            string bigFilePath = String.Empty;
                            string mediaPath = studyItem.MediaDirectoryPath;
                            MediaRun mediaRun = null;
                            string annotationValue = String.Empty;
                            Annotation annotation = null;
                            XAttribute src = element.Attribute("src");
                            if (src != null)
                                normalUrl = src.Value;
                            XAttribute targetUrl = element.Attribute("targeturl");
                            if (targetUrl != null)
                                bigUrl = targetUrl.Value;
                            if (!String.IsNullOrEmpty(normalUrl))
                            {
                                switch (PictureMode)
                                {
                                    case "Local":
                                    case "Download":
                                        //normalFileName = SiteTextTools.GetFileNameFromPath(normalUrl, kMaxNodeLength);
                                        normalFileName = studyItem.KeyString + MediaUtilities.GetFileExtension(normalUrl);
                                        normalFilePath = SiteTextTools.ConcatenateFilePaths(mediaPath, normalFileName);
                                        if (GetMediaFile("Illustration", normalUrl, normalFilePath))
                                        {
                                            if (!UsePicturesAsAnnotations)
                                            {
                                                mediaRun = new MediaRun("Picture", normalFileName, null, null, Owner, TimeSpan.Zero, TimeSpan.Zero);
                                                if (mediaRuns == null)
                                                    mediaRuns = new List<MediaRun>(1) { mediaRun };
                                                else
                                                    mediaRuns.Add(mediaRun);
                                            }
                                        }
                                        break;
                                    case "Remote":
                                        if (!UsePicturesAsAnnotations)
                                        {
                                            mediaRun = new MediaRun("Picture", normalUrl, null, null, Owner, TimeSpan.Zero, TimeSpan.Zero);
                                            mediaRun.StorageState = MediaStorageState.External;
                                            if (mediaRuns == null)
                                                mediaRuns = new List<MediaRun>(1) { mediaRun };
                                            else
                                                mediaRuns.Add(mediaRun);
                                        }
                                        break;
                                    case "None":
                                    default:
                                        break;
                                }
                            }
                            if (!String.IsNullOrEmpty(bigUrl))
                            {
                                switch (PictureMode)
                                {
                                    case "Local":
                                    case "Download":
                                        //bigFileName = SiteTextTools.GetFileNameFromPath(bigUrl, kMaxNodeLength);
                                        bigFileName = studyItem.KeyString + MediaUtilities.GetFileExtension(bigUrl);
                                        bigFilePath = SiteTextTools.ConcatenateFilePaths(mediaPath, bigFileName);
                                        if (GetMediaFile("Illustration", bigUrl, bigFilePath))
                                        {
                                            if (!UsePicturesAsAnnotations)
                                            {
                                                mediaRun = new MediaRun("Picture", bigFileName, null, null, Owner, TimeSpan.Zero, TimeSpan.Zero);
                                                if (mediaRuns == null)
                                                    mediaRuns = new List<MediaRun>(1) { mediaRun };
                                                else
                                                    mediaRuns.Add(mediaRun);
                                            }
                                        }
                                        break;
                                    case "Remote":
                                        if (!UsePicturesAsAnnotations)
                                        {
                                            mediaRun = new MediaRun("Picture", bigUrl, null, null, Owner, TimeSpan.Zero, TimeSpan.Zero);
                                            mediaRun.StorageState = MediaStorageState.External;
                                            if (mediaRuns == null)
                                                mediaRuns = new List<MediaRun>(1) { mediaRun };
                                            else
                                                mediaRuns.Add(mediaRun);
                                        }
                                        break;
                                    case "None":
                                    default:
                                        break;
                                }
                            }
                            if (UsePicturesAsAnnotations && !String.IsNullOrEmpty(normalUrl))
                            {
                                if (!String.IsNullOrEmpty(bigUrl))
                                {
                                    switch (PictureMode)
                                    {
                                        case "Local":
                                        case "Download":
                                            annotationValue = normalFileName + "," + bigFileName;
                                            break;
                                        case "Remote":
                                            annotationValue = normalUrl + "," + bigUrl;
                                            break;
                                        case "None":
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (PictureMode)
                                    {
                                        case "Local":
                                        case "Download":
                                            annotationValue = normalFileName;
                                            break;
                                        case "Remote":
                                            annotationValue = normalUrl;
                                            break;
                                        case "None":
                                        default:
                                            break;
                                    }
                                }
                                annotation = studyItem.FindAnnotation("Picture");
                                if (annotation == null)
                                {
                                    annotation = new Annotation(
                                        "Picture",
                                        annotationValue);
                                    studyItem.AddAnnotation(annotation);
                                }
                                else
                                    annotation.Value = annotationValue;
                            }
                        }
                        break;
                }
            }

            return returnValue;
        }

        protected bool ExtractSentencesStudyList(
            ContentStudyList sentencesStudyList,
            ContentStudyList textStudyList,
            LanguageID languageID)
        {
            BaseObjectContent sentencesContent = sentencesStudyList.Content;
            string contentKey = sentencesContent.KeyString;
            List<MultiLanguageItem> studyItems = textStudyList.StudyItems;
            string destTildeUrl = sentencesContent.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;
            string title = String.Empty;
            string languageName = languageID.LanguageName(UILanguageID);
            int paragraphCount = studyItems.Count();
            int paragraphIndex = 0;
            bool returnValue = true;

            if (Lesson != null)
                title = Lesson.GetTitleString(UILanguageID);

            UpdateProgressMessageElapsed("Extracting sentences for " + title + " for " + languageName + " ...");

            if (!DontDeleteMedia)
                NodeUtilities.DeleteStudyListMediaHelper(sentencesContent);

            sentencesStudyList.DeleteAllStudyItems();
            sentencesStudyList.StudyItemOrdinal = 0;

            foreach (MultiLanguageItem studyItem in studyItems)
            {
                if (IsCanceled())
                    break;

                if (!studyItem.HasText())
                    continue;

                /*
                PutStatusMessageElapsed(
                    "Extracting sentences from paragraph " +
                        paragraphIndex.ToString() +
                        " of " +
                        paragraphCount.ToString() +
                        " for " +
                        title +
                        " for " +
                        languageName +
                        " ...");
                */

                paragraphIndex++;

                int sentenceCount = studyItem.GetMaxSentenceCount(LanguageIDs);

                if (sentenceCount == 0)
                    continue;

                string sourceTildeUrl = studyItem.MediaTildeUrl + "/";
                string relativePathToSource = MediaUtilities.MakeRelativeUrl(destTildeUrl, sourceTildeUrl);

                for (int sentenceIndex = 0; sentenceIndex < sentenceCount; sentenceIndex++)
                {
                    string sentenceText = studyItem.RunText(languageID, sentenceIndex);

                    if (PriorStudyItemCache != null)
                    {
                        if (PriorStudyItemCache.Find(contentKey, languageID, sentenceText, out oldStudyItem))
                            continue;
                    }
                    else
                    {
                        if (sentencesStudyList.FindStudyItem(sentenceText, languageID) != null)
                            continue;

                        if (NodeUtilities.StringExistsInPriorContentCheck(
                                sentencesContent,
                                sentenceText,
                                languageID,
                                out oldContent,
                                out oldStudyItem))
                            continue;

                        if (IsExcludePrior)
                        {
                            if (NodeUtilities.StringExistsInPriorLessonsCheck(
                                    sentencesContent,
                                    sentenceText,
                                    languageID,
                                    out oldContent,
                                    out oldStudyItem))
                                continue;
                        }
                    }

                    string studyItemKey = sentencesStudyList.AllocateStudyItemKey();
                    MultiLanguageItem targetStudyItem = new MultiLanguageItem(studyItemKey, new List<LanguageItem>());
                    bool haveMediaRuns = false;

                    foreach (LanguageItem sourceLanguageItem in studyItem.LanguageItems)
                    {
                        TextRun sourceSentenceRun = sourceLanguageItem.GetSentenceRun(sentenceIndex);
                        if (sourceSentenceRun == null)
                        {
                            PutStatusMessage(
                                "Missmatched sentences in " +
                                    sourceLanguageItem.Text +
                                    " ...");
                            continue;
                        }
                        string text = sourceLanguageItem.GetRunText(sourceSentenceRun);
                        LanguageID sourceLanguageID = sourceLanguageItem.LanguageID;
                        List<MediaRun> targetMediaRuns = null;
                        if (sourceSentenceRun.HasMediaRunWithKey("Audio"))
                        {
                            targetMediaRuns = sourceSentenceRun.CloneAndRetargetMediaRuns(relativePathToSource);
                            if ((targetMediaRuns != null) && (targetMediaRuns.Count() != 0))
                                haveMediaRuns = true;
                        }
                        TextRun targetSentenceRun = new TextRun(0, text.Length, targetMediaRuns);
                        List<TextRun> wordRuns = null;
                        if (sourceLanguageItem.HasWordRuns())
                            wordRuns = sourceLanguageItem.GetSentenceWordRunsRetargeted(sourceSentenceRun);
                        LanguageItem targetLanguageItem = new LanguageItem(
                            studyItemKey,
                            sourceLanguageID,
                            text,
                            new List<TextRun>(1) { targetSentenceRun },
                            wordRuns);
                        targetStudyItem.Add(targetLanguageItem);
                    }

                    if ((sentenceIndex == 0) && studyItem.HasAnnotation("Ordinal"))
                        targetStudyItem.Annotations = studyItem.CloneAnnotations();

                    sentencesStudyList.AddStudyItem(targetStudyItem);

                    if (PriorStudyItemCache != null)
                        PriorStudyItemCache.Add(contentKey, languageID, targetStudyItem);

                    targetStudyItem.SentenceAndWordRunCheck(Repositories.Dictionary);

                    if (!haveMediaRuns && (IsSynthesizeMissingAudio || IsForceAudio))
                    {
                        if (!NodeUtilities.GetMediaForStudyItem(
                                targetStudyItem,
                                LanguageIDs,
                                (AudioMode == "Local") || (AudioMode == "Download"),
                                (PictureMode == "Local") || (PictureMode == "Download"),
                                ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                                false,
                                IsSynthesizeMissingAudio,
                                IsForceAudio,
                                false))
                        {
                            //returnValue = false;
                            PutMessage("Ignoring audio generation error.");
                        }
                    }
                }
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(sentencesStudyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(sentencesContent);

            return returnValue;
        }

        protected bool ExtractWordsStudyList(
            ContentStudyList wordsStudyList,
            ContentStudyList textStudyList,
            LanguageID languageID)
        {
            BaseObjectContent wordsContent = wordsStudyList.Content;
            string contentKey = wordsContent.KeyString;
            BaseObjectNode node = wordsContent.Node;
            List<MultiLanguageItem> studyItems = textStudyList.StudyItems;
            string destTildeUrl = wordsStudyList.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;
            string title = String.Empty;
            string languageName = languageID.LanguageName(UILanguageID);
            List<LanguageID> languageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageID);
            int targetLanguageIDCount = languageIDs.Count();
            LanguageID firstAlternateLanguageID = null;
            bool returnValue = true;

            if (Lesson != null)
                title = Lesson.GetTitleString(UILanguageID);

            if (targetLanguageIDCount > 1)
                firstAlternateLanguageID = languageIDs[1];

            UpdateProgressMessageElapsed("Extracting words for " + title + " for " + languageName + " ...");

            if (!DontDeleteMedia)
                NodeUtilities.DeleteStudyListMediaHelper(wordsContent);

            wordsStudyList.DeleteAllStudyItems();
            wordsStudyList.StudyItemOrdinal = 0;

            List<MultiLanguageString> words = NodeUtilities.CollectWordInstances(studyItems, languageIDs);
            int wordCount = words.Count();
            int wordIndex;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                MultiLanguageString word = words[wordIndex];
                string targetWord = word.Text(languageID);

                if (ProgressCancelCheck())
                    break;

                UpdateProgressMessageElapsed("Extracting words for " + title + " for " + languageName + " index "
                    + wordIndex.ToString() + " of " + wordCount.ToString() + " ...");

                /*
                PutStatusMessageElapsed(
                    "Extracting word " +
                        wordIndex.ToString() +
                        " of " +
                        wordCount.ToString() +
                        " for " +
                        title +
                        " for " +
                        languageName +
                        " ...");
                */

                string wordHash = word.GetStringListString(languageIDs);

                if (PriorStudyItemCache != null)
                {
                    if (PriorStudyItemCache.Find(contentKey, languageID, wordHash, out oldStudyItem))
                        continue;
                }
                else
                {
                    if (wordsStudyList.FindStudyItemInstance(word, languageIDs) != null)
                        continue;

                    if (NodeUtilities.TextExistsInPriorContentCheck(
                            wordsContent,
                            word,
                            languageIDs,
                            out oldContent,
                            out oldStudyItem))
                        continue;

                    if (IsExcludePrior)
                    {
                        if (NodeUtilities.TextExistsInPriorLessonsCheck(
                                wordsContent,
                                word,
                                languageIDs,
                                out oldContent,
                                out oldStudyItem))
                            continue;
                    }
                }

                string studyItemKey = wordsStudyList.AllocateStudyItemKey();
                MultiLanguageItem targetStudyItem = new MultiLanguageItem(
                    studyItemKey,
                    LanguageIDs,
                    word);

                targetStudyItem.PrimeSentenceRunsForWordItem(languageIDs);
                LanguageItem targetLanguageItem = targetStudyItem.LanguageItem(languageID);

                DictionaryEntry dictionaryEntry = null;
                List<DictionaryEntry> dictionaryEntries;
                bool isInflection = false;

                if (Tool != null)
                {
                    dictionaryEntry = Tool.LookupDictionaryEntry(
                        targetWord,
                        MatchCode.Exact,
                        languageIDs,
                        null,
                        out isInflection);

                    if (dictionaryEntry != null)
                        dictionaryEntries = new List<DictionaryEntry>(1) { dictionaryEntry };
                    else
                        dictionaryEntries = null;
                }
                else
                    dictionaryEntries = Repositories.Dictionary.Lookup(
                        targetWord,
                        MatchCode.Exact,
                        languageID,
                        0,
                        0);

                if (ApplicationData.IsMobileVersion)
                {
                    if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                    {
                        if (ApplicationData.RemoteRepositories != null)
                        {
                            dictionaryEntries = ApplicationData.RemoteRepositories.Dictionary.Lookup(
                                targetWord,
                                JTLanguageModelsPortable.Matchers.MatchCode.Exact,
                                languageID,
                                0,
                                0);

                            if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                            {
                                try
                                {
                                    if (!Repositories.Dictionary.AddList(dictionaryEntries, languageID))
                                        PutError("Error saving local dictionary entry");
                                }
                                catch (Exception exc)
                                {
                                    PutExceptionError("Exception saving local dictionary entry", exc);
                                }
                            }
                        }
                    }
                }

                if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                {
                    dictionaryEntry = dictionaryEntries.First();
                    int senseCount = dictionaryEntry.SenseCount;
                    int senseIndex;
                    int reading = -1;

                    /*
                    if (dictionaryEntry.LanguageID != TargetLanguageID)
                    {
                        String baseText = dictionaryEntry.GetFirstAlternateText(languageID);
                        string suffix = targetLanguageItem.Text;
                        targetLanguageItem.Text = baseText;
                        targetLanguageItem.PrimeSentenceRunsForWordItem();
                        Annotation suffixAnnotation = new Annotation(
                            "Suffix",
                            null,
                            new Object.MultiLanguageString(
                                null,
                                new LanguageString(null, languageID, suffix)));
                        targetStudyItem.AddAnnotation(suffixAnnotation);
                    }
                    */

                    if ((dictionaryEntry.AlternateCount != 0) && (targetLanguageIDCount > 1))
                    {
                        string altText = word.Text(firstAlternateLanguageID);

                        foreach (LanguageString alternate in dictionaryEntry.Alternates)
                        {
                            if ((alternate.Text == altText) && (alternate.LanguageID == firstAlternateLanguageID))
                            {
                                reading = alternate.KeyInt;
                                break;
                            }
                        }
                    }

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                        if ((reading != -1) && (sense.Reading != reading))
                            continue;

                        foreach (LanguageID hostLanguageID in HostLanguageIDs)
                        {
                            LanguageItem hostLanguageItem = targetStudyItem.LanguageItem(hostLanguageID);

                            if (sense.HasLanguage(hostLanguageID))
                            {
                                string definitionString = sense.GetDefinition(hostLanguageID, false, false);

                                definitionString = definitionString.Replace(" / ", ", ");

                                if (!hostLanguageItem.HasText())
                                    hostLanguageItem.Text = definitionString;
                            }
                        }
                    }
                }

                if (IsTranslateMissingItems)
                {
                    string errorMessage;

                    if (!LanguageUtilities.Translator.TranslateMultiLanguageItem(
                            targetStudyItem,
                            LanguageIDs,
                            false,
                            false,
                            out errorMessage,
                            false))
                    {
                        PutLogError(errorMessage);
                        returnValue = false;
                    }
                }

                // Need content set before getting media.
                wordsStudyList.AddStudyItem(targetStudyItem);

                if (PriorStudyItemCache != null)
                    PriorStudyItemCache.Add(contentKey, languageID, wordHash, targetStudyItem);

                if (IsLookupDictionaryAudio || IsLookupDictionaryPictures || IsSynthesizeMissingAudio || IsForceAudio)
                {
                    if (!NodeUtilities.GetMediaForStudyItem(
                            targetStudyItem,
                            LanguageIDs,
                            (AudioMode == "Local") || (AudioMode == "Download"),
                            (PictureMode == "Local") || (PictureMode == "Download"),
                            ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                            IsLookupDictionaryAudio,
                            IsSynthesizeMissingAudio,
                            IsForceAudio,
                            IsLookupDictionaryPictures))
                    {
                        //returnValue = false;
                        PutMessage("GetMediaForStudyItem failed", NodeUtilities.MessageOrError);
                    }
                }

                /*
                {
                    string errorMessage = String.Empty;
                    NodeUtilities.AddStudyItemToDictionary(
                        node,
                        wordsContent,
                        targetStudyItem,
                        LanguageIDs,
                        LanguageIDs,
                        LexicalCategory.Unknown,
                        String.Empty,
                        false,
                        false,
                        ref errorMessage);
                }
                */
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(wordsStudyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(wordsContent);

            return returnValue;
        }

        protected bool ExtractWordsStudyListOld(
            ContentStudyList wordsStudyList,
            ContentStudyList textStudyList,
            LanguageID languageID)
        {
            BaseObjectContent wordsContent = wordsStudyList.Content;
            string contentKey = wordsContent.KeyString;
            BaseObjectNode node = wordsContent.Node;
            List<MultiLanguageItem> studyItems = textStudyList.StudyItems;
            string destTildeUrl = wordsStudyList.MediaTildeUrl + "/";
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;
            string title = String.Empty;
            string languageName = languageID.LanguageName(UILanguageID);
            bool returnValue = true;

            if (Lesson != null)
                title = Lesson.GetTitleString(UILanguageID);

            UpdateProgressMessageElapsed("Extracting words for " + title + " for " + languageName + " ...");

            if (!DontDeleteMedia)
                NodeUtilities.DeleteStudyListMediaHelper(wordsContent);

            wordsStudyList.DeleteAllStudyItems();
            wordsStudyList.StudyItemOrdinal = 0;

            List<string> words = new List<string>();
            HashSet<string> wordsHash = new HashSet<string>();
            
            NodeUtilities.CollectWords(studyItems, languageID, words, wordsHash);

            int wordCount = words.Count();
            int wordIndex;

            for (wordIndex = 0; wordIndex < wordCount; wordIndex++)
            {
                string word = words[wordIndex];

                if (IsCanceled())
                    break;

                /*
                PutStatusMessageElapsed(
                    "Extracting word " +
                        wordIndex.ToString() +
                        " of " +
                        wordCount.ToString() +
                        " for " +
                        title +
                        " for " +
                        languageName +
                        " ...");
                */

                if (PriorStudyItemCache != null)
                {
                    if (PriorStudyItemCache.Find(contentKey, languageID, word, out oldStudyItem))
                        continue;
                }
                else
                {
                    if (wordsStudyList.FindStudyItem(word, languageID) != null)
                        continue;

                    if (NodeUtilities.StringExistsInPriorContentCheck(
                            wordsContent,
                            word,
                            languageID,
                            out oldContent,
                            out oldStudyItem))
                        continue;

                    if (IsExcludePrior)
                    {
                        if (NodeUtilities.StringExistsInPriorLessonsCheck(
                                wordsContent,
                                word,
                                languageID,
                                out oldContent,
                                out oldStudyItem))
                            continue;
                    }
                }

                string studyItemKey = wordsStudyList.AllocateStudyItemKey();
                MultiLanguageItem targetStudyItem = new MultiLanguageItem(
                    studyItemKey,
                    LanguageIDs);

                LanguageItem targetLanguageItem = targetStudyItem.LanguageItem(languageID);
                targetLanguageItem.Text = word;
                TextRun targetRun = new TextRun(0, word.Length, null);
                targetLanguageItem.SentenceRuns = new List<TextRun>() { targetRun };

                DictionaryEntry dictionaryEntry = null;
                List<DictionaryEntry> dictionaryEntries;
                bool isInflection = false;

                if (Tool != null)
                {
                    dictionaryEntry = Tool.LookupDictionaryEntry(
                        word,
                        MatchCode.Exact,
                        TargetLanguageIDs,
                        null,
                        out isInflection);

                    if (dictionaryEntry != null)
                        dictionaryEntries = new List<DictionaryEntry>(1) { dictionaryEntry };
                    else
                        dictionaryEntries = null;
                }
                else
                    dictionaryEntries = Repositories.Dictionary.Lookup(
                        word,
                        MatchCode.Exact,
                        languageID,
                        0,
                        0);

                if (ApplicationData.IsMobileVersion)
                {
                    if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                    {
                        if (ApplicationData.RemoteRepositories != null)
                        {
                            dictionaryEntries = ApplicationData.RemoteRepositories.Dictionary.Lookup(
                                word,
                                JTLanguageModelsPortable.Matchers.MatchCode.Exact,
                                languageID,
                                0,
                                0);

                            if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                            {
                                try
                                {
                                    if (!Repositories.Dictionary.AddList(dictionaryEntries, languageID))
                                        PutError("Error saving local dictionary entry");
                                }
                                catch (Exception exc)
                                {
                                    PutExceptionError("Exception saving local dictionary entry", exc);
                                }
                            }
                        }
                    }
                }

                if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                {
                    dictionaryEntry = dictionaryEntries.First();
                    int senseCount = dictionaryEntry.SenseCount;
                    int senseIndex;

                    if (dictionaryEntry.LanguageID != TargetLanguageID)
                    {
                        LanguageItem alternateTargetLanguageItem = targetStudyItem.LanguageItem(dictionaryEntry.LanguageID);
                        alternateTargetLanguageItem.Text = dictionaryEntry.KeyString;
                        TextRun wordRun = new TextRun(0, alternateTargetLanguageItem.TextLength, null);
                        alternateTargetLanguageItem.SentenceRuns = new List<TextRun>() { wordRun };
                        targetLanguageItem.Text = String.Empty;
                        targetLanguageItem.SentenceRuns = null;
                    }

                    if (dictionaryEntry.AlternateCount != 0)
                    {
                        int altCount = TargetLanguageIDs.Count() - 1;

                        if (altCount != 0)
                        {
                            foreach (LanguageString alternate in dictionaryEntry.Alternates)
                            {
                                LanguageID alternateLanguageID = alternate.LanguageID;

                                if (TargetRomanizationLanguageIDs.Contains(alternateLanguageID))
                                {
                                    LanguageItem alternateLanguageItem = targetStudyItem.LanguageItem(alternateLanguageID);

                                    if (!alternateLanguageItem.HasText())
                                    {
                                        string alternateText = alternate.Text;

                                        if (LanguageLookup.IsAlternatePhonetic(alternateLanguageID))
                                            ConvertPinyinNumeric.ToToneMarks(out alternateText, alternateText);

                                        alternateLanguageItem.Text = alternateText;
                                        TextRun wordRun = new TextRun(0, alternateLanguageItem.TextLength, null);
                                        alternateLanguageItem.SentenceRuns = new List<TextRun>() { wordRun };

                                        if (--altCount == 0)
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                        foreach (LanguageID hostLanguageID in HostLanguageIDs)
                        {
                            LanguageItem hostLanguageItem = targetStudyItem.LanguageItem(hostLanguageID);

                            if (sense.HasLanguage(hostLanguageID))
                            {
                                string definitionString = sense.GetDefinition(hostLanguageID, false, false);

                                definitionString = definitionString.Replace(" / ", ", ");

                                if (!hostLanguageItem.HasText())
                                    hostLanguageItem.Text = definitionString;
                            }
                        }
                    }
                }

                if (IsTranslateMissingItems)
                {
                    string errorMessage;

                    if (!LanguageUtilities.Translator.TranslateMultiLanguageItem(
                            targetStudyItem,
                            LanguageIDs,
                            false,
                            false,
                            out errorMessage,
                            false))
                    {
                        PutLogError(errorMessage);
                        returnValue = false;
                    }
                }

                // Need content set before getting media.
                wordsStudyList.AddStudyItem(targetStudyItem);

                if (PriorStudyItemCache != null)
                    PriorStudyItemCache.Add(contentKey, languageID, targetStudyItem);

                if (IsLookupDictionaryAudio || IsLookupDictionaryPictures || IsSynthesizeMissingAudio || IsForceAudio)
                {
                    if (!NodeUtilities.GetMediaForStudyItem(
                            targetStudyItem,
                            LanguageIDs,
                            (AudioMode == "Local") || (AudioMode == "Download"),
                            (PictureMode == "Local") || (PictureMode == "Download"),
                            ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                            IsLookupDictionaryAudio,
                            IsSynthesizeMissingAudio,
                            IsForceAudio,
                            IsLookupDictionaryPictures))
                    {
                        //returnValue = false;
                        PutMessage("Ignoring audio generation error.");
                    }
                }

                /*
                {
                    string errorMessage = String.Empty;
                    NodeUtilities.AddStudyItemToDictionary(
                        node,
                        wordsContent,
                        targetStudyItem,
                        LanguageIDs,
                        LanguageIDs,
                        LexicalCategory.Unknown,
                        String.Empty,
                        false,
                        false,
                        ref errorMessage);
                }
                */
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(wordsStudyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(wordsContent);

            return returnValue;
        }

        protected bool ExtractCharactersStudyList(
            ContentStudyList charactersStudyList,
            ContentStudyList textStudyList,
            LanguageID languageID)
        {
            BaseObjectContent charactersContent = charactersStudyList.Content;
            string contentKey = charactersContent.KeyString;
            BaseObjectNode node = charactersContent.Node;
            List<MultiLanguageItem> studyItems = textStudyList.StudyItems;
            string destTildeUrl = charactersStudyList.MediaTildeUrl;
            BaseObjectContent oldContent;
            MultiLanguageItem oldStudyItem;
            string title = String.Empty;
            string languageName = languageID.LanguageName(UILanguageID);
            bool returnValue = true;

            if (Lesson != null)
                title = Lesson.GetTitleString(UILanguageID);

            UpdateProgressMessageElapsed("Extracting characters for " + title + " for " + languageName + " ...");

            if (!DontDeleteMedia)
                NodeUtilities.DeleteStudyListMediaHelper(charactersContent);

            charactersStudyList.DeleteAllStudyItems();
            charactersStudyList.StudyItemOrdinal = 0;

            LanguageDescription languageDescription = LanguageLookup.GetLanguageDescription(languageID);

            if ((languageDescription == null) || !languageDescription.CharacterBased)
            {
                PutLogError("Skipping character extraction because the target language is not character-based.");
                return false;
            }

            List<string> characters = NodeUtilities.CollectCharacters(studyItems, languageID);
            int characterCount = characters.Count();
            int characterIndex;

            for (characterIndex = 0; characterIndex < characterCount; characterIndex++)
            {
                string character = characters[characterIndex];

                if (IsCanceled())
                    break;

                /*
                PutStatusMessageElapsed(
                    "Extracting character " +
                        characterIndex.ToString() +
                        " of " +
                        characterCount.ToString() +
                        " for " +
                        title +
                        " for " +
                        languageName +
                        " ...");
                */

                if (PriorStudyItemCache != null)
                {
                    if (PriorStudyItemCache.Find(contentKey, languageID, character, out oldStudyItem))
                        continue;
                }
                else
                {
                    if (charactersStudyList.FindStudyItem(character, languageID) != null)
                        continue;

                    if (NodeUtilities.StringExistsInPriorContentCheck(
                            charactersContent,
                            character,
                            languageID,
                            out oldContent,
                            out oldStudyItem))
                        continue;

                    if (IsExcludePrior)
                    {
                        if (NodeUtilities.StringExistsInPriorLessonsCheck(
                                charactersContent,
                                character,
                                languageID,
                                out oldContent,
                                out oldStudyItem))
                            continue;
                    }
                }

                string studyItemKey = charactersStudyList.AllocateStudyItemKey();
                MultiLanguageItem targetStudyItem = new MultiLanguageItem(
                    studyItemKey,
                    LanguageIDs);

                LanguageItem targetLanguageItem = targetStudyItem.LanguageItem(languageID);
                targetLanguageItem.Text = character;
                TextRun targetRun = new TextRun(0, character.Length, null);
                targetLanguageItem.SentenceRuns = new List<TextRun>() { targetRun };

                DictionaryEntry dictionaryEntry = null;
                List<DictionaryEntry> dictionaryEntries = Repositories.Dictionary.Lookup(
                    character,
                    JTLanguageModelsPortable.Matchers.MatchCode.Exact,
                    languageID,
                    0,
                    0);

                if (ApplicationData.IsMobileVersion)
                {
                    if ((dictionaryEntries == null) || (dictionaryEntries.Count() == 0))
                    {
                        if (ApplicationData.RemoteRepositories != null)
                        {
                            dictionaryEntries = ApplicationData.RemoteRepositories.Dictionary.Lookup(
                                character,
                                JTLanguageModelsPortable.Matchers.MatchCode.Exact,
                                languageID,
                                0,
                                0);

                            if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                            {
                                try
                                {
                                    if (!Repositories.Dictionary.AddList(dictionaryEntries, languageID))
                                        PutError("Error saving local dictionary entry");
                                }
                                catch (Exception exc)
                                {
                                    PutExceptionError("Exception saving local dictionary entry", exc);
                                }
                            }
                        }
                    }
                }

                if ((dictionaryEntries != null) && (dictionaryEntries.Count() != 0))
                {
                    dictionaryEntry = dictionaryEntries.First();
                    int senseCount = dictionaryEntry.SenseCount;
                    int senseIndex;

                    if (dictionaryEntry.AlternateCount != 0)
                    {
                        foreach (LanguageString alternate in dictionaryEntry.Alternates)
                        {
                            LanguageID alternateLanguageID = alternate.LanguageID;

                            if (LanguageIDs.Contains(alternateLanguageID))
                            {
                                LanguageItem alternateLanguageItem = targetStudyItem.LanguageItem(alternateLanguageID);

                                if (!alternateLanguageItem.HasText())
                                {
                                    string alternateText = alternate.Text;

                                    if (LanguageLookup.IsAlternatePhonetic(alternateLanguageID))
                                        ConvertPinyinNumeric.ToToneMarks(out alternateText, alternateText);

                                    alternateLanguageItem.Text = alternateText;
                                }
                            }
                        }
                    }

                    for (senseIndex = 0; senseIndex < senseCount; senseIndex++)
                    {
                        Sense sense = dictionaryEntry.GetSenseIndexed(senseIndex);

                        foreach (LanguageID hostLanguageID in HostLanguageIDs)
                        {
                            LanguageItem hostLanguageItem = targetStudyItem.LanguageItem(hostLanguageID);

                            if (sense.HasLanguage(hostLanguageID))
                            {
                                string definitionString = sense.GetDefinition(hostLanguageID, false, false);

                                definitionString = definitionString.Replace(" / ", ", ");

                                if (!hostLanguageItem.HasText())
                                    hostLanguageItem.Text = definitionString;
                            }
                        }
                    }
                }

                if (IsTranslateMissingItems)
                {
                    string errorMessage;

                    if (!LanguageUtilities.Translator.TranslateMultiLanguageItem(
                            targetStudyItem,
                            LanguageIDs,
                            false,
                            false,
                            out errorMessage,
                            false))
                    {
                        PutLogError(errorMessage);
                        returnValue = false;
                    }
                }

                // Need content set before getting media.
                charactersStudyList.AddStudyItem(targetStudyItem);

                if (PriorStudyItemCache != null)
                    PriorStudyItemCache.Add(contentKey, languageID, targetStudyItem);

                if (IsLookupDictionaryAudio || IsLookupDictionaryPictures || IsSynthesizeMissingAudio || IsForceAudio)
                {
                    if (!NodeUtilities.GetMediaForStudyItem(
                            targetStudyItem,
                            LanguageIDs,
                            (AudioMode == "Local") || (AudioMode == "Download"),
                            (PictureMode == "Local") || (PictureMode == "Download"),
                            ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion,
                            IsLookupDictionaryAudio,
                            IsSynthesizeMissingAudio,
                            IsForceAudio,
                            IsLookupDictionaryPictures))
                    {
                        //returnValue = false;
                        PutMessage("Ignoring audio generation error.");
                    }
                }

                /*
                {
                    string errorMessage = String.Empty;
                    NodeUtilities.AddStudyItemToDictionary(
                        node,
                        charactersContent,
                        targetStudyItem,
                        LanguageIDs,
                        LanguageIDs,
                        LexicalCategory.Unknown,
                        String.Empty,
                        false,
                        false,
                        ref errorMessage);
                }
                */
            }

            if (SubDivide)
                NodeUtilities.SubDivideStudyList(charactersStudyList, TitlePrefix, Master,
                    true, StudyItemSubDivideCount, MinorSubDivideCount, MajorSubDivideCount);

            NodeUtilities.UpdateContentHelper(charactersContent);

            return returnValue;
        }

        // Some common help strings.
        public static string DontDeleteMediaHelp = "Don't delete media when deleting course.";
        public static string OverwriteMediaHelp = "Overwrite media even if it exists.";
        public static string TitleHelp = "Set the title of the target item. Leave blank for a default name.";
        public static string DescriptionHelp = "Set the description of the target item. Leave blank for a default name.";
        public static string CourseNameHelp = "Set the name of the course. Leave blank for a default name.";
        public static string CourseNamePrefixHelp = "Set the prefix of the course name. Leave blank for no prefix.";
        public static string CourseNameSuffixHelp = "Set the suffix of the course name. Leave blank for no prefix.";
        public static string CourseLevelHelp = "Select the level of the course. Choose \"none\" if course can apply to multiple levels. Leave blank for a default level.";
        public static string LessonPathPatternsHelp = "Enter the lesson path patterns for filtering specific lessons or groups."
            + " A lesson path pattern is a three part path in the form \"level/season/lesson\", which translates to \"course/group/lesson\" in JTLanguage."
            + " Use '*' for a wild card, i.e. \"*/*/*\" for all lessons.  Separate multiple paths with a comma or newline.";
        public static string SplitLevelsHelp = "Split levels into separate courses.";
        public static string TargetLanguageIDHelp = "Select the target language.";
        public static string RomanizationLanguageIDHelp = "Select the romanized language.";
        public static string HostLanguageIDHelp = "Select the host language.";
        public static string UILanguageIDHelp = "Select the UI language.";
        public static string TargetLanguageIDsHelp = "Select the target languages.";
        public static string RomanizationLanguageIDsHelp = "Select the romanized languages.";
        public static string HostLanguageIDsHelp = "Select the host languages.";
        public static string OwnerHelp = "Enter owner user ID.";
        public static string IsPublicHelp = "If checked, the course will be marked as public.";
        public static string PackageHelp = "Enter package name. This will restrict access to users with the package name set in their account.";
        public static string UniqueContentHelp = "If checked, trees and nodes names will be made unique.";
        public static string AudioModeHelp = "Select how audio files are handled. Local means they will be downloaded, Remote means a we reference, and None omits the media entirely.";
        public static string VideoModeHelp = "Select how video files are handled. Local means they will be downloaded, Remote means a we reference, and None omits the media entirely.";
        public static string PictureModeHelp = "Select how picture files are handled. Local means they will be downloaded, Remote means a we reference, and None omits the media entirely.";
        public static string DocumentModeHelp = "Select how document files are handled. Local means they will be downloaded, Remote means a we reference, and None omits the media entirely.";
        public static string VoiceSpeedHelp = "Specify synthesised voice speed. Interger value from -10 to 10.";
        public static string VoicePauseSecondsHelp = "Specify delay between synthesised items. Seconds floating point value.";
        public static string UsePicturesAsAnnotationsHelp = "If checked, pictures will be included as annotations.";
        public static string UseHeadingsAsTextHelp = "If checked, include headings as text in study list. Otherwise, include headings as annotations.";
        public static string UseMediaListHelp = "Create media list for media items.";
        public static string UseGenericMastersHelp = "If checked, create generic lesson masters instead of lagnuage specific ones.";
        public static string CacheHtmlHelp = "If checked, crawled HTML files will be cached, such that future crawls to the same page will load instead from the saved file.";
        public static string ForceHtmlCacheUpdateHelp = "If checked, forces update of crawled HTML file caches.";
        public static string UpdateHtmlCacheOnlyHelp = "If checked, only updates the HTML file cache.";
        public static string SaveLogHelp = "Save log.";
        public static string VerboseHelp = "Be verbose about activity.";
        public static string OutputFilePathHelp = "The path to the directory into which to put intermediate output.";
        public static string MediaFilePathHelp = "The path to the directory into which to put the media output.";
        public static string MediaManifestFileHelp = "The path to an optional media manifest file.";
        public static string WebUserNameHelp = "Enter your user name for the web site, if needed.";
        public static string WebPasswordHelp = "Enter your password for the web site, if needed.";
        public static string ExtractTextHelp = "Check this to extract text.";
        public static string ExtractSentencesHelp = "Check this to extract sentences.";
        public static string ExtractWordsHelp = "Check this to extract words.";
        public static string ExtractCharactersHelp = "Check this to extract characters.";
        public static string ElementXPathHelp = "Enter XPath for element to get the text from. For info on XPath, see: https://www.w3schools.com/xml/xpath_syntax.asp";
        public static string CachePathHelp = "Enter path to cache directory, a place to cache page files.";
        public static string DoSentenceFixesHelp = "Check this to do sentence fixups.";
        public static string SentenceFixesKeyHelp = "Enter sentence fixups key.";
        public static string DoWordFixesHelp = "Check this to do word fixups.";
        public static string WordFixesKeyHelp = "Enter word fixups key.";

        public virtual void PrefetchArguments(FormReader formReader)
        {
        }

        public virtual void LoadFromArguments()
        {
            DeleteBeforeImport = Format.GetFlagArgumentDefaulted("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", FormatExtract.DeleteBeforeImportHelp, null, null);

            DontDeleteMedia = Format.GetFlagArgumentDefaulted("DontDeleteMedia", "flag", "r", DontDeleteMedia,
                "Don't delete media", DontDeleteMediaHelp, null, null);

            OverwriteMedia = Format.GetFlagArgumentDefaulted("OverwriteMedia", "flag", "r", OverwriteMedia,
                "Overwrite media", OverwriteMediaHelp, null, null);
        }

        public virtual void SaveToArguments()
        {
            Format.SetFlagArgument("DeleteBeforeImport", "flag", "r", DeleteBeforeImport,
                "Delete before import", FormatExtract.DeleteBeforeImportHelp, null, null);

            Format.SetFlagArgument("DontDeleteMedia", "flag", "r", DontDeleteMedia,
                "Don't delete media", DontDeleteMediaHelp, null, null);

            Format.SetFlagArgument("OverwriteMedia", "flag", "r", OverwriteMedia,
                "Overwrite media", OverwriteMediaHelp, null, null);
        }

        public virtual void DumpArguments(string label)
        {
            if (!String.IsNullOrEmpty(label))
                DumpString(label + ":\n");

            DumpArgument("DeleteBeforeImport", DeleteBeforeImport);
            DumpArgument("DontDeleteMedia", DontDeleteMedia);
            DumpArgument("OverwriteMedia", OverwriteMedia);
        }

        public virtual void DumpArgument(string label, object value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
                msg += label + " = ";

            if (value != null)
                msg += value.ToString();
            else
                msg += "(null)";

            DumpString(msg);
        }

        public virtual void DumpArgumentList<T>(string label, List<T> value)
        {
            string msg = String.Empty;

            if (!String.IsNullOrEmpty(label))
                msg += label + " = ";

            if (value != null)
            {
                int index = 0;

                foreach (object item in value)
                {
                    if (index != 0)
                        msg += ", ";

                    msg += item.ToString();
                    index++;
                }
            }
            else
                msg += "(null)";

            DumpString(msg);
        }

        // Check for supported capability.
        // contentType: class name or GetComponentName value
        // capability: "Supported" for general support,
        //  "UseFlags" for component item select support,
        //  "ContentKeyFlags" for node component select support,
        //  "NodeFlags" for sub-object select support,
        //  "Text" for support of text import.
        //  "File" for support of file import.
        //  "Web" for support of web import.
        public static bool IsSupportedStatic(string contentType, string capability)
        {
            return false;
        }

        public virtual bool IsSupportedVirtual(string contentType, string capability)
        {
            return false;
        }

        public static string TypeStringStatic { get { return "Crawler"; } }

        public virtual string TypeStringVirtual { get { return TypeStringStatic; } }

        public bool StreamTransfer(Stream inStream, Stream outStream)
        {
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            int read;

            while ((read = inStream.Read(buffer, 0, bufferSize)) > 0)
                outStream.Write(buffer, 0, read);

            return true;
        }

        public bool ReadAndIgnoreAllStreamBytes(Stream inStream)
        {
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            int read;

            while ((read = inStream.Read(buffer, 0, bufferSize)) == bufferSize)
                ;

            return true;
        }

        public List<LanguageID> CloneTargetLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(TargetLanguageIDs);
            languageIDs.AddRange(RomanizationLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneHostLanguageIDs()
        {
            return new List<LanguageID>(HostLanguageIDs);
        }

        public List<LanguageID> CloneRomanizationLanguageIDs()
        {
            return new List<LanguageID>(RomanizationLanguageIDs);
        }

        public List<LanguageID> CloneTargetRomanizationHostLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(TargetRomanizationHostLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneHostTargetRomanizationLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(HostTargetRomanizationLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneTargetRomanizationLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(TargetRomanizationLanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> CloneLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(LanguageIDs);
            return languageIDs;
        }

        public List<LanguageID> GetGenericTargetLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(1) { LanguageLookup.Target };
            return languageIDs;
        }

        public List<LanguageID> GetGenericHostLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(1) { LanguageLookup.Host };
            return languageIDs;
        }

        public List<LanguageID> GetGenericLanguageIDs()
        {
            List<LanguageID> languageIDs = new List<LanguageID>(1) { LanguageLookup.My };
            return languageIDs;
        }

        public string MediaAbbreviation
        {
            get
            {
                return TextUtilities.MakeFirstLetterUpperCase(TargetLanguageID.LanguageCode)
                    + TextUtilities.MakeFirstLetterUpperCase(HostLanguageID.LanguageCode);
            }
        }

        public string MediaSlashAbbreviation
        {
            get
            {
                return TextUtilities.MakeFirstLetterUpperCase(TargetLanguageID.LanguageCode)
                    + "/" + TextUtilities.MakeFirstLetterUpperCase(HostLanguageID.LanguageCode);
            }
        }

        public string MediaTargetAndHostString
        {
            get
            {
                return "(in "
                    + TargetLanguageID.LanguageName(UILanguageID)
                    + " and "
                    + HostLanguageID.LanguageName(UILanguageID)
                    + ").";
            }
        }

        public virtual string GetQueryAttribute(string attributeName)
        {
            return UrlUtilities.GetQueryValue(SiteMainUrl, attributeName);
        }

        protected virtual bool GetMediaFile(string label, string sourceUrl, string destFilePath)
        {
            if (String.IsNullOrEmpty(sourceUrl))
            {
                string msg = "GetMediaFile: Source URL empty for: " + label;
                PutLogError(msg);
                throw new Exception(msg);
            }

            if (String.IsNullOrEmpty(destFilePath))
            {
                string msg = "GetMediaFile: Destination path empty for: " + label;
                PutLogError(msg);
                throw new Exception(msg);
            }

            sourceUrl = sourceUrl.Trim();
            destFilePath = destFilePath.Trim();

            if (!sourceUrl.StartsWith("http"))
            {
                string msg = "GetMediaFile: Source URL doesn't start with http: " + sourceUrl;
                PutMessage(msg);
                //PutLogError(msg);
                //throw new Exception(msg);
                return false;
            }

            if (destFilePath.StartsWith("http"))
            {
                string msg = "GetMediaFile: Destination path shouldn't start with http: " + destFilePath;
                PutLogError(msg);
                throw new Exception(msg);
            }

            if (IsBadLink(sourceUrl))
            {
                PutLogError("Skipping bad link for \"" + label + "\" file \"" + sourceUrl + "\" to \"" + destFilePath + "\" ...");
                return true;
            }

            if (destFilePath.Length > 256)
            {
                PutLogError("Skipping too long file: \"" + destFilePath);
                return false;
            }

            IDataBuffer dataBuffer = new FileBuffer(destFilePath);
            bool destExists = dataBuffer.Exists();
            List<string> filePaths = null;
            bool destInFilePaths = false;

            if (OriginalMediaMap != null)
            {
                if (OriginalMediaMap.TryGetValue(sourceUrl, out filePaths))
                {
                    if (filePaths.Contains(destFilePath))
                    {
                        destInFilePaths = true;

                        if (destExists)
                        {
                            if (!OverwriteMedia)
                                return true;
                        }
                    }

                    if (!OverwriteMedia && !destExists)
                    {
                        int pathIndex;
                        int pathCount = filePaths.Count();

                        for (pathIndex = pathCount - 1; pathIndex >= 0; pathIndex--)
                        {
                            string testPath = filePaths[pathIndex];

                            if (FileSingleton.Exists(testPath))
                            {
                                if (CopyMediaFile(label, destFilePath, testPath))
                                {
                                    if (!destInFilePaths)
                                        filePaths.Add(destFilePath);

                                    return true;
                                }
                                else
                                    return false;
                            }
                            else
                                filePaths.RemoveAt(pathIndex);
                        }
                    }

                    if (GetRawMediaFile(label, sourceUrl, destFilePath))
                    {
                        if (!destInFilePaths)
                            filePaths.Add(destFilePath);

                        return true;
                    }

                    return false;
                }
                else
                {
                    if (destExists)
                    {
                        filePaths = new List<string>() { destFilePath };
                        OriginalMediaMap.Add(sourceUrl, filePaths);
                        return true;
                    }
                    else
                    {
                        if (GetRawMediaFile(label, sourceUrl, destFilePath))
                        {
                            filePaths = new List<string>() { destFilePath };
                            OriginalMediaMap.Add(sourceUrl, filePaths);
                            return true;
                        }
                    }
                }

                return true;
            }
            else
            {
                if (destExists)
                    return true;

                if (GetRawMediaFile(label, sourceUrl, destFilePath))
                    return true;
            }

            return true;
        }

        protected virtual bool GetRawMediaFile(string label, string sourceUrl, string destFilePath)
        {
            return false;
        }

        protected bool CopyMediaFile(string label, string destFilePath, string sourceFilePath)
        {
            if (!FileSingleton.Exists(sourceFilePath))
            {
                string msg = "CopyMediaFile: Source path doesn't exist for " + label + ": " + sourceFilePath;
                PutLogError(msg);
                return false;
            }

            if (!FileSingleton.DirectoryExistsCheck(destFilePath))
                return false;

            if (OverwriteMedia || !FileSingleton.Exists(destFilePath))
            {
                if (Verbose)
                    PutStatusMessageElapsed("Copying \"" + label + "\" file \"" + sourceFilePath + "\" to \"" + destFilePath + "\" ...");

                try
                {
                    FileSingleton.Copy(sourceFilePath, destFilePath);
                    return true;
                }
                catch (Exception exc)
                {
                    PutStatusMessageElapsed("Error copying \"" + label + "\" file \"" + sourceFilePath + "\" to \"" + destFilePath + ": " + exc.Message + ".");
                }

                return false;
            }

            return true;
        }

        public void LoadBadLinks()
        {
            string fileName = ComposeCrawlerDataFilePath(SiteName, SiteName + "_BadLinks.txt");

            UpdateProgress("Loading bad links.");

            if (!FileSingleton.Exists(fileName))
                return;

            try
            {
                using (StreamReader sr = FileSingleton.OpenText(fileName))
                {
                    do
                    {
                        string line = sr.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                            AddBadLink(line);
                    }
                    while (sr.Peek() != -1);
                }
            }
            catch (Exception e)
            {
                PutError("The bad links file could not be read:");
                PutError(e.Message);
            }
        }

        public void SaveBadLinks()
        {
            UpdateProgress("Saving bad links.");

            if (BadLinks == null)
                return;

            string fileName = ComposeCrawlerDataFilePath(SiteName, SiteName + "_BadLinks.txt");

            try
            {
                using (StreamWriter sw = FileSingleton.CreateText(fileName))
                {
                    foreach (KeyValuePair<string, bool> kvp in BadLinks)
                        sw.WriteLine(kvp.Key);

                    FileSingleton.Close(sw);
                }
            }
            catch (Exception e)
            {
                PutError("The bad links file could not be written:");
                PutError(e.Message);
            }
        }

        public bool IsBadLink(string url)
        {
            if (BadLinks == null)
                return false;

            bool value = false;

            BadLinks.TryGetValue(url, out value);

            return value;
        }

        public void AddBadLink(string url)
        {
            if (BadLinks == null)
                BadLinks = new Dictionary<string, bool>();

            bool value = false;

            if (!BadLinks.TryGetValue(url, out value))
                BadLinks.Add(url, true);
        }

        public void LoadSentenceFixes(bool doFixes, string label)
        {
            if (doFixes)
            {
                SentenceFixes sentenceFixes;
                string filePath = SentenceFixes.GetFilePath(label, null);

                UpdateProgress("Loading sentence fixes for " + label);

                if (SentenceFixes.CreateAndLoad(filePath, out sentenceFixes))
                    SentenceFixes = sentenceFixes;
                else
                    UpdateProgress("No sentence fixes found for " + label);
            }
            else
            {
                UpdateProgress("Skipping loading of sentence fixes for " + label);
                SentenceFixes = null;
            }
        }

        public void SaveSentenceFixes(string label)
        {
            if (SentenceFixes != null)
            {
                UpdateProgress("Saving sentence fixes for " + label);

                if (!SentenceFixes.Save(Course, TargetLanguageID, UILanguageID))
                    UpdateProgress("Error saving sentence fixes for " + label);
            }
            else
                UpdateProgress("No sentence fixes to save for " + label);
        }

        public bool DeleteCourseCheck(string name, string owner)
        {
            bool someDeleted = false;

            if (DeleteBeforeImport)
            {
                for (;;)
                {
                    ObjectReferenceNodeTree treeHeader =
                        Repositories.ResolveNamedReference("CourseHeaders", null, owner, name) as ObjectReferenceNodeTree;

                    if (treeHeader != null)
                    {
                        BaseObjectNodeTree tree = Repositories.Courses.Get(treeHeader.Key);

                        if (tree != null)
                        {
                            if (NodeUtilities != null)
                                someDeleted = NodeUtilities.DeleteTreeHelper(tree, !DontDeleteMedia) || someDeleted;
                        }
                        else
                            PutError("DeleteCourseCheck: tree not found: " + name);
                    }
                    else
                        break;
                }
            }

            return someDeleted;
        }

        public void UpdateErrorFromNodeUtilities()
        {
            if (NodeUtilities.HasError)
                PutLogError(NodeUtilities.Error);

            NodeUtilities.ClearMessageAndError();
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

                if (lastUpperIndex > maxLength / 3)
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

            name = name.Replace(" ", "");
            name = name.Replace("\r", "");
            name = name.Replace("\n", "");
            name = name.Replace("\t", "");
            name = name.Replace("#", "");
            name = name.Replace("/", "_");
            name = name.Replace("-", "");
            name = name.Replace("–", "");
            name = name.Replace("\\", "");
            name = name.Replace("|", "");
            name = name.Replace("<", "");
            name = name.Replace(">", "");
            name = name.Replace("*", "");
            name = name.Replace("...", "");
            name = name.Replace("&#039;", "");

            return name;
        }

        protected bool IsRightToLeft(LanguageID languageID)
        {
            switch (languageID.LanguageCode)
            {
                case "he":
                case "ar":
                    return true;
                default:
                    return false;
            }
        }

        protected void AddMissingSpeakerTextTranslations(ContentStudyList lessonStudyList)
        {
            List<TextRun> targetTextRuns;
            TextRun targetRun;
            int languageCount = LanguageIDs.Count();

            foreach (MultiLanguageItem speakerTextEntry in lessonStudyList.StudyItems)
            {
                for (int languageIndex = 0; languageIndex < languageCount; languageIndex++)
                {
                    LanguageID testLanguageID = LanguageIDs[languageIndex];
                    LanguageItem testLanguageItem = speakerTextEntry.LanguageItem(testLanguageID);

                    if (testLanguageItem == null)
                    {
                        testLanguageItem = new LanguageItem(speakerTextEntry.Key, testLanguageID, String.Empty);
                        speakerTextEntry.LanguageItems.Insert(languageIndex, testLanguageItem);
                    }

                    if (!String.IsNullOrEmpty(testLanguageItem.Text))
                        continue;

                    if (RomanizationLanguageIDs.Contains(testLanguageID))
                    {
                        List<LanguageID> sourceLanguageIDs = LanguageLookup.GetNonPhoneticLanguageIDs(RomanizationLanguageID);

                        foreach (LanguageID sourceLanguageID in sourceLanguageIDs)
                        {
                            if (!CanRomanize(sourceLanguageID))
                                continue;

                            LanguageItem sourceItem = speakerTextEntry.LanguageItem(sourceLanguageID);

                            if ((sourceItem == null) || String.IsNullOrEmpty(sourceItem.Text))
                                continue;

                            string sourceText = sourceItem.Text;
                            string targetText;

                            if (RomanizationConverter.To(out targetText, sourceText))
                            {
                                targetRun = new TextRun(0, targetText.Length, sourceItem.SentenceRuns[0].MediaRuns);
                                targetTextRuns = new List<TextRun>(1) { targetRun };
                                testLanguageItem.Text = targetText;
                                testLanguageItem.SentenceRuns = targetTextRuns;
                            }
                        }
                    }
                    else if (TargetRomanizationLanguageIDs.Contains(testLanguageID))
                    {
                        if (CanTransliterate(testLanguageID))
                        {
                            List<LanguageID> sourceLanguageIDs = LanguageLookup.GetNonPhoneticLanguageIDs(RomanizationLanguageID);

                            foreach (LanguageID sourceLanguageID in sourceLanguageIDs)
                            {
                                if (sourceLanguageID == testLanguageID)
                                    continue;

                                LanguageItem sourceItem = speakerTextEntry.LanguageItem(sourceLanguageID);

                                if ((sourceItem == null) || String.IsNullOrEmpty(sourceItem.Text))
                                    continue;

                                string sourceText = sourceItem.Text;
                                string targetText;

                                if (ConvertTransliterate.ConvertCharacters(sourceLanguageID, testLanguageID, '\0', sourceText,
                                    out targetText, Repository.Dictionary,
                                    FormatQuickLookup.GetQuickDictionary(sourceLanguageID, testLanguageID)))
                                {
                                    targetRun = new TextRun(0, targetText.Length, sourceItem.SentenceRuns[0].MediaRuns);
                                    targetTextRuns = new List<TextRun>(1) { targetRun };
                                    testLanguageItem.Text = targetText;
                                    testLanguageItem.SentenceRuns = targetTextRuns;
                                }
                            }
                        }
                        else if (CanRomanize(testLanguageID))
                        {
                            List<LanguageID> sourceLanguageIDs = LanguageLookup.GetNonPhoneticLanguageIDs(RomanizationLanguageID);

                            foreach (LanguageID sourceLanguageID in sourceLanguageIDs)
                            {
                                if (sourceLanguageID == testLanguageID)
                                    continue;

                                LanguageItem sourceItem = speakerTextEntry.LanguageItem(sourceLanguageID);

                                if ((sourceItem == null) || String.IsNullOrEmpty(sourceItem.Text))
                                    continue;

                                if (!CanRomanize(sourceLanguageID))
                                    continue;

                                string sourceText = sourceItem.Text;
                                string targetText;

                                if (ConvertTransliterate.ConvertAlternatePhonetic(sourceLanguageID, testLanguageID, '\0', sourceText,
                                    out targetText, Repository.Dictionary,
                                    FormatQuickLookup.GetQuickDictionary(sourceLanguageID, testLanguageID),
                                    Tool))
                                {
                                    targetRun = new TextRun(0, targetText.Length, sourceItem.SentenceRuns[0].MediaRuns);
                                    targetTextRuns = new List<TextRun>(1) { targetRun };
                                    testLanguageItem.Text = targetText;
                                    testLanguageItem.SentenceRuns = targetTextRuns;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected bool CanTransliterate(LanguageID languageID)
        {
            if (LanguageLookup.HasAlternateCharacters(languageID))
                return true;

            return false;
        }

        protected bool CanRomanize(LanguageID languageID)
        {
            if (LanguageLookup.HasAlternatePhonetic(languageID))
                return true;

            return false;
        }

        protected void DeleteEmptyContent()
        {
            if (Verbose)
                PutStatusMessageElapsed("Updating media state for " + Course.GetTitleString(UILanguageID) + "...");

            if (NodeUtilities.UpdateTreeNodeMediaCheck(
                    Course,
                    Course,
                    true,
                    true))
            {
                if (Verbose)
                    PutStatusMessageElapsed("Deleting empty content for " + Course.GetTitleString(UILanguageID) + "...");

                if (!NodeUtilities.DeleteEmptyTreeNodeContentCheck(
                        Course,
                        Course,
                        false,
                        true,
                        true))
                    PutLogError(NodeUtilities.Error);
            }
            else
                PutLogError(NodeUtilities.Error);
        }

        public static List<string> BadRomanization = new List<string>();

        protected void DisplayBadRomanization()
        {
            if (BadRomanization.Count() != 0)
            {
                PutStatusMessageElapsed("Bad romanizations (" + BadRomanization.Count().ToString() + ").");

                foreach (string str in BadRomanization)
                    PutStatusMessageElapsed(str);
            }
            //else
            //    PutStatusMessageElapsed("No bad romanizations.");
        }

        protected List<string> ExtractTextsFromHtmlElement(XElement element, List<LanguageID> languageIDs,
            bool isConvertPhoneticNonRomanization, bool isConvertRomanization)
        {
            List<string> strs = new List<string>(languageIDs.Count());
            int languageIndex = 0;
            foreach (LanguageID languageID in languageIDs)
            {
                string text;
                if (LanguageLookup.IsRomanized(languageID))
                {
                    if (isConvertRomanization)
                    {
                        if ((RomanizationConverter != null) && (languageIndex > 0))
                        {
                            int sourceIndex = languageIDs.IndexOf(NonRomanizationLanguageID);
                            if (sourceIndex == -1)
                                sourceIndex = 0;
                            string lastText = strs[sourceIndex];
                            if (!RomanizationConverter.To(out text, lastText))
                                BadRomanization.Add(lastText + " -> " + text);
                            // hack to deal with space added in conversion.
                            if (lastText.Contains("[<"))
                            {
                                text = text.Replace("[ <", "[<");
                                text = text.Replace("> ]", ">]");
                            }
                            text = FixupTransliteration(
                                text,
                                languageID,
                                lastText,
                                languageIDs[languageIndex - 1],
                                false);
                        }
                        else
                            text = String.Empty;
                    }
                    else
                        text = String.Empty;
                }
                else
                {
                    if (LanguageLookup.IsNonRomanizedAlternatePhonetic(languageID))
                    {
                        if (isConvertPhoneticNonRomanization)
                        {
                            if (CharacterConverter != null)
                            {
                                string chars = strs.Last();
                                if (!String.IsNullOrEmpty(chars))
                                    CharacterConverter.To(out text, chars);
                                else
                                    text = String.Empty;
                            }
                            else
                                text = String.Empty;
                        }
                        else
                        {
                            text = ExtractTextFromHtmlElement(element, languageID);

                            if (String.IsNullOrEmpty(text))
                            {
                                if (CharacterConverter != null)
                                {
                                    string chars = strs.Last();
                                    if (!String.IsNullOrEmpty(chars))
                                        CharacterConverter.To(out text, chars);
                                    else
                                        text = String.Empty;
                                }
                            }
                        }
                    }
                    else
                        text = ExtractTextFromHtmlElement(element, languageID);
                    text = FixupLanguageText(text, languageID);
                }
                strs.Add(text.Trim());
                languageIndex++;
            }
            return strs;
        }

        protected List<string> ExtractTextsFromHtmlElementCanonical(XElement element, List<LanguageID> languageIDs,
            bool isConvertPhoneticNonRomanization, bool isConvertRomanization)
        {
            List<string> strs = new List<string>(languageIDs.Count());
            int languageIndex = 0;
            foreach (LanguageID languageID in languageIDs)
            {
                string text;
                if (LanguageLookup.IsRomanized(languageID))
                {
                    if (isConvertRomanization)
                    {
                        if ((RomanizationConverter != null) && (languageIndex > 0))
                        {
                            int sourceIndex = languageIDs.IndexOf(NonRomanizationLanguageID);
                            if (sourceIndex == -1)
                                sourceIndex = 0;
                            string lastText = strs[sourceIndex];
                            if (!RomanizationConverter.To(out text, lastText))
                                BadRomanization.Add(lastText + " -> " + text);
                            // hack to deal with space added in convertion.
                            if (lastText.Contains("[<"))
                            {
                                text = text.Replace("[ <", "[<");
                                text = text.Replace("> ]", ">]");
                            }
                            text = FixupTransliteration(
                                text,
                                languageID,
                                lastText,
                                languageIDs[languageIndex - 1],
                                false);
                        }
                        else
                            text = String.Empty;
                    }
                    else
                        text = String.Empty;
                }
                else
                {
                    if (LanguageLookup.IsNonRomanizedAlternatePhonetic(languageID))
                    {
                        if (isConvertPhoneticNonRomanization)
                        {
                            if (CharacterConverter != null)
                            {
                                string chars = strs.Last();
                                CharacterConverter.To(out text, chars);
                            }
                            else
                                text = String.Empty;
                        }
                        else
                            text = ExtractTextFromHtmlElement(element, languageID);
                    }
                    else
                        text = ExtractTextFromHtmlElement(element, languageID);
                    text = FixupLanguageText(text, languageID);
                    text = TextUtilities.GetCanonicalText(text, languageID);
                }
                strs.Add(text.Trim());
                languageIndex++;
            }
            return strs;
        }

        protected virtual string FixupTransliteration(
            string transliteration,
            LanguageID transliterationLanguageID,
            string nonTransliteration,
            LanguageID nonTransliterationLanguageID,
            bool isWord)
        {
            return transliteration;
        }

        protected virtual string FixupLanguageText(string text, LanguageID languageID)
        {
            return text;
        }

        protected string ExtractTextFromHtmlElement(XElement element, LanguageID languageID)
        {
            StringBuilder sb = new StringBuilder();
            ExtractTextFromHtmlElementRecurse(element, sb, languageID);
            return TextUtilities.TrimIncludingZeroSpace(sb.ToString());
        }

        protected void ExtractTextFromHtmlElementRecurse(XElement element, StringBuilder sb, LanguageID languageID)
        {
            string elementName = element.Name.LocalName.ToLower();

            switch (elementName)
            {
                // Ignore these elements
                case "input":
                case "sup":
                case "sub":
                case "script":
                    return;
                case "img":
                    if (!IsIgnoreImages)
                    {
                        string annotation = "[" + element.ToString() + "]";
                        sb.Append(annotation);
                    }
                    return;
                case "rb":
                    if (LanguageLookup.IsAlternatePhonetic(languageID))
                        return;
                    ExtractTextFromHtmlElementTrim(element, sb, languageID);
                    return;
                case "rt":
                    if (!LanguageLookup.IsNonRomanizedAlternatePhonetic(languageID))
                        return;
                    ExtractTextFromHtmlElementTrim(element, sb, languageID);
                    return;
                default:
                    break;
            }

            foreach (XNode node in element.Nodes())
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        PreElementInsertionCheck(node as XElement, sb, languageID);
                        ExtractTextFromHtmlElementRecurse(node as XElement, sb, languageID);
                        PostElementInsertionCheck(node as XElement, sb, languageID);
                        break;
                    case XmlNodeType.Text:
                        {
                            XText textNode = node as XText;
                            sb.Append(Normalize(textNode.Value));
                        }
                        break;
                    default:
                        break;
                }
            }

            switch (elementName)
            {
                case "p":
                case "div":
                    sb.AppendLine("");
                    break;
                default:
                    break;
            }
        }

        protected void ExtractTextFromHtmlElementTrim(XElement element, StringBuilder sb, LanguageID languageID)
        {
            foreach (XNode node in element.Nodes())
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        PreElementInsertionCheck(node as XElement, sb, languageID);
                        ExtractTextFromHtmlElementRecurse(node as XElement, sb, languageID);
                        PostElementInsertionCheck(node as XElement, sb, languageID);
                        break;
                    case XmlNodeType.Text:
                        {
                            XText textNode = node as XText;
                            sb.Append(Normalize(textNode.Value.Trim()));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected string Normalize(string str)
        {
            return TextUtilities.GetNormalizedString(str);
        }

        protected virtual void PreElementInsertionCheck(XElement element, StringBuilder sb, LanguageID languageID)
        {
        }

        protected virtual void PostElementInsertionCheck(XElement element, StringBuilder sb, LanguageID languageID)
        {
        }

        protected XElement ConvertHtmlTextToXElement(string htmlString)
        {
            string newString = CloseSomeUnclosedElements(htmlString);
            XElement element = ConvertHtmlTextToXElementRaw(newString);
            return element;
        }

        protected XElement ConvertHtmlTextToXElementRaw(string htmlString)
        {
            XElement element = null;
            try
            {
                element = XElement.Parse(htmlString, LoadOptions.PreserveWhitespace);
            }
            catch (Exception)
            {
                //PutLogExceptionError("Error converting string to XElement", exc);
            }
            return element;
        }

        protected static string[] LikelyUnclosedElementNames =
        {
            "hr",
            "img",
            "col "  // Keep space to differentiate from colgroup.
        };

        protected string CloseSomeUnclosedElements(string htmlString)
        {
            htmlString = htmlString.Replace("<br>", "<br />");
            htmlString = htmlString.Replace("<hr>", "<hr />");
            htmlString = htmlString.Replace("<col>", "<col />");
            htmlString = htmlString.Replace("<>", "");
            htmlString = htmlString.Replace("&", "&amp;");
            htmlString = htmlString.Replace("<p></dd>", "</dd>");

            foreach (string elementName in LikelyUnclosedElementNames)
                htmlString = CloseAnUnclosedElement(htmlString, elementName);

            return htmlString;
        }

        protected string CloseAnUnclosedElement(string html, string elementName)
        {
            int len = html.Length;
            int i;

            for (i = 0; i < len;)
            {
                int ofs = html.IndexOf("<" + elementName, i);

                if (ofs == -1)
                    break;

                int endOfs = html.IndexOf(">", ofs + 1);

                if (endOfs != -1)
                {
                    if (html[endOfs - 1] != '/')
                    {
                        html = html.Insert(endOfs, "/");
                        len = html.Length;
                    }
                    else
                        i = endOfs + 1;
                }
                else
                    i = ofs + 1;
            }

            return html;
        }

        public bool GetLanguageIDFromLabel(string languageGroup, string label, List<LanguageID> languageIDs,
            out LanguageID languageID, out string subLabel)
        {
            bool returnValue = true;
            subLabel = null;
            if (label.StartsWith("Standard "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            if (label.StartsWith("Formal "))
            {
                subLabel = label;
                label = label.Substring(7);
            }
            if (label.StartsWith("Informal "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            if (label.StartsWith("Polite "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            if (label.StartsWith("Casual "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == label);
            if (languageID == null)
            {
                switch (languageGroup)
                {
                    case "Target":
                        switch (label)
                        {
                            case "Simplified Chinese":
                                languageID = LanguageLookup.ChineseSimplified;
                                break;
                            case "Traditional Chinese":
                                languageID = LanguageLookup.ChineseTraditional;
                                break;
                            case "Kanji":
                                languageID = LanguageLookup.Japanese;
                                break;
                            case "Hiragana":
                            case "Katagana":
                                languageID = LanguageLookup.JapaneseKana;
                                break;
                            case "Egyptian Arabic":
                            case "Moroccan Arabic":
                                languageID = LanguageLookup.Arabic;
                                break;
                            case "Vowelled Arabic":
                            case "Vowelled Egyptian Arabic":
                            case "Vowelled Standard Arabic":
                            case "Vowelled Moroccan Arabic":
                                languageID = LanguageLookup.ArabicVowelled;
                                break;
                            default:
                                if (label.Contains(" "))
                                {
                                    string[] parts = label.Split(SpaceCharacterArray);
                                    string firstName = parts[0];
                                    string lastName = parts.Last();
                                    languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == lastName);
                                    if (languageID == null)
                                    {
                                        languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == firstName);
                                        if (languageID == null)
                                            returnValue = false;
                                    }
                                    if (String.IsNullOrEmpty(subLabel))
                                        subLabel = label;
                                }
                                else
                                    returnValue = false;
                                break;
                        }
                        break;
                    case "Romanization":
                        switch (label)
                        {
                            case "Pinyin":
                                languageID = LanguageLookup.ChinesePinyin;
                                break;
                            case "Romanization":
                                languageID = RomanizationLanguageID;
                                break;
                            case "Rōmaji":
                            case "RÅmaji":
                            case "Romaji":
                                languageID = LanguageLookup.JapaneseRomaji;
                                break;
                            case "Moroccan Romanization":
                            case "Egyptian Romanization":
                                languageID = RomanizationLanguageID;
                                break;
                            default:
                                if (label.Contains("Romanization"))
                                    languageID = RomanizationLanguageID;
                                else
                                    returnValue = false;
                                break;
                        }
                        break;
                    case "Host":
                    default:
                        returnValue = false;
                        break;
                }
                if (!returnValue)
                {
                    foreach (LanguageID testLanguageID in languageIDs)
                    {
                        string languageName = testLanguageID.LanguageName(HostLanguageID);
                        if (label.Contains(languageName))
                        {
                            languageID = testLanguageID;
                            return true;
                        }
                    }
                }
            }
            return returnValue;
        }

        private static char[] SpaceCharacterArray = { ' ' };

        public bool GetLanguageIDFromLabel(string label, List<LanguageID> languageIDs, out LanguageID languageID, out string subLabel)
        {
            bool returnValue = true;
            subLabel = null;
            if (label.StartsWith("Standard "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            if (label.StartsWith("Formal "))
            {
                subLabel = label;
                label = label.Substring(7);
            }
            if (label.StartsWith("Informal "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            if (label.StartsWith("Polite "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            if (label.StartsWith("Casual "))
            {
                subLabel = label;
                label = label.Substring(9);
            }
            languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == label);
            if (languageID == null)
            {
                switch (label)
                {
                    case "Main":
                        languageID = TargetLanguageID;
                        break;
                    case "Romanization":
                        languageID = RomanizationLanguageID;
                        break;
                    case "Simplified Chinese":
                        languageID = LanguageLookup.ChineseSimplified;
                        break;
                    case "Traditional Chinese":
                        languageID = LanguageLookup.ChineseTraditional;
                        break;
                    case "Kanji":
                        languageID = LanguageLookup.Japanese;
                        break;
                    case "Hiragana":
                    case "Katagana":
                        languageID = LanguageLookup.JapaneseKana;
                        break;
                    case "Pinyin":
                        languageID = LanguageLookup.ChinesePinyin;
                        break;
                    //case "Romanization":
                    case "Rōmaji":
                    case "RÅmaji":
                    case "Romaji":
                    case "romaji":
                        languageID = LanguageLookup.JapaneseRomaji;
                        break;
                    case "english":
                    case "English":
                        languageID = LanguageLookup.English;
                        break;
                    case "Formal French":
                    case "Informal French":
                        languageID = LanguageLookup.French;
                        break;
                    case "Egyptian Arabic":
                    case "Moroccan Arabic":
                        languageID = LanguageLookup.Arabic;
                        break;
                    case "Vowelled Arabic":
                    case "Vowelled Egyptian Arabic":
                    case "Vowelled Standard Arabic":
                    case "Vowelled Moroccan Arabic":
                        languageID = LanguageLookup.ArabicVowelled;
                        break;
                    case "Vowelled":
                    case "Voweled":
                        languageID = languageIDs.FirstOrDefault(x => x.ExtensionCode == "vw");
                        if (languageID == null)
                            returnValue = false;
                        break;
                    case "ill-askjh1":
                    case "term":
                        languageID = TargetLanguageID;
                        break;
                    case "ill-askjh2":
                        languageID = HostLanguageID;
                        break;
                    case "Moroccan Romanization":
                    case "Egyptian Romanization":
                        languageID = RomanizationLanguageID;
                        break;
                    default:
                        if (label.Contains("Romanization"))
                            languageID = RomanizationLanguageID;
                        else if (label.StartsWith("Vowelled"))
                        {
                            string[] parts = label.Split(SpaceCharacterArray);
                            string vowelledName = parts[1] + "-" + parts[0];
                            languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == vowelledName);
                            if (languageID == null)
                                returnValue = false;
                        }
                        else if (label.Contains(" "))
                        {
                            string[] parts = label.Split(SpaceCharacterArray);
                            string firstName = parts[0];
                            string lastName = parts.Last();
                            languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == lastName);
                            if (languageID == null)
                            {
                                languageID = languageIDs.FirstOrDefault(x => x.LanguageName(HostLanguageID) == firstName);
                                if (languageID == null)
                                    returnValue = false;
                            }
                            if (String.IsNullOrEmpty(subLabel))
                                subLabel = label;
                        }
                        else
                            returnValue = false;
                        break;
                }
            }
            return returnValue;
        }

        public void PutLogError(string message)
        {
            SetOperationStatusLabel(message);
            PutError(message);
            WriteLog(message);
        }

        public void PutLogError(string message, string argument)
        {
            string msg = message + ": " + argument;
            SetOperationStatusLabel(msg);
            PutErrorArgument(message, argument);
            WriteLog(msg);
        }

        public void PutLogErrorArgument(string message, string argument)
        {
            string msg = message + ": " + argument;
            SetOperationStatusLabel(msg);
            PutErrorArgument(message, argument);
            WriteLog(msg);
        }

        public void PutLogExceptionError(Exception exc)
        {
            string message = exc.Message;

            if (exc.InnerException != null)
                message = message + ": " + exc.InnerException.Message;

            PutError(message);
            WriteLog(message);
        }

        public void PutLogExceptionError(string message, Exception exc)
        {
            string fullMessage = S(message) + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            PutError(fullMessage);
            WriteLog(fullMessage);
        }

        public void PutLogExceptionError(string message, string argument, Exception exc)
        {
            string fullMessage = S(message) + ": " + argument + ": " + exc.Message;

            if (exc.InnerException != null)
                fullMessage = fullMessage + ": " + exc.InnerException.Message;

            PutError(fullMessage);
            WriteLog(fullMessage);
        }
    }
}
