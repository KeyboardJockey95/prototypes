using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
//using JTLanguageModelsPortable.Remote;

namespace JTLanguageModelsPortable.Repository
{
    public class MainRepository : BaseObjectKeyed, IMainRepository
    {
        protected string _ContentPath;
        protected string _ServerUrl;
        private int _RepositoryCount = 21;
        protected LanguageBaseStringRepository _UIStrings;
        protected LanguageBaseStringRepository _UIText;
        protected LanguageDescriptionRepository _LanguageDescriptions;
        protected DictionaryRepository _Dictionary;
        protected DictionaryRepository _DictionaryStems;
        protected DeinflectionRepository _Deinflections;
        protected NodeTreeReferenceRepository _CourseHeaders;
        protected NodeTreeRepository _Courses;
        protected ContentStudyListRepository _StudyLists;
        protected ContentMediaItemRepository _MediaItems;
        protected ContentDocumentItemRepository _DocumentItems;
        protected SandboxRepository _Sandboxes;
        protected UserRunItemRepository _UserRunItems;
        protected ContentStatisticsRepository _ContentStatistics;
        protected MarkupTemplateRepository _MarkupTemplates;
        protected NodeMasterRepository _NodeMasters;
        protected NodeTreeReferenceRepository _PlanHeaders;
        protected NodeTreeRepository _Plans;
        protected ToolProfileRepository _ToolProfiles;
        protected ToolStudyListRepository _ToolStudyLists;
        protected ToolSessionRepository _ToolSessions;
        protected ImageRepository _LessonImages;
        protected ImageRepository _ProfileImages;
        protected AudioRepository _DictionaryAudio;
        protected AudioMultiRepository _DictionaryMultiAudio;
        protected PictureRepository _DictionaryPictures;
        protected UserRecordRepository _UserRecords;
        protected AnonymousUserRecordRepository _AnonymousUserRecords;
        protected ChangeLogItemRepository _ChangeLogItems;
        protected ForumCategoryRepository _ForumCategories;
        protected ForumHeadingRepository _ForumHeadings;
        protected ForumTopicRepository _ForumTopics;
        protected ForumPostingRepository _ForumPostings;
        protected LanguageBaseStringRepository _CourseTreeCache;
        protected LanguageBaseStringRepository _PlanTreeCache;
        protected LanguagePairBaseStringRepository _TranslationCache;
        protected LanguagePairBaseStringsRepository _TranslationWithAlternatesCache;
        public static string CacheDirectory { get; set; }
        private IMainRepository _Mirror;
        private UpdateProgressFunction _UpdateProgress;
        public bool RequestCancel { get; set; }
        public static IMainRepository Global;

        public static Dictionary<string, string> RepositorySignatures = new Dictionary<string, string>()
        {
            { "UIStrings", "SingleLanguage" },
            { "UIText", "SingleLanguage" },
            { "LanguageDescriptions", "NoLanguage" },
            { "Dictionary", "SingleLanguage" },
            { "DictionaryStems", "SingleLanguage" },
            { "Deinflections", "SingleLanguage" },
            { "CourseHeaders", "NoLanguage" },
            { "Courses", "NoLanguage" },
            { "StudyLists", "NoLanguage" },
            { "MediaItems", "NoLanguage" },
            { "DocumentItems", "NoLanguage" },
            { "Sandboxes", "NoLanguage" },
            { "UserRunItems", "SingleLanguage" },
            { "ContentStatistics", "NoLanguage" },
            { "MarkupTemplates", "NoLanguage" },
            { "NodeMasters", "NoLanguage" },
            { "PlanHeaders", "NoLanguage" },
            { "Plans", "NoLanguage" },
            { "ToolProfiles", "NoLanguage" },
            { "ToolStudyLists", "NoLanguage" },
            { "ToolSessions", "NoLanguage" },
            { "LessonImages", "NoLanguage" },
            { "ProfileImages", "NoLanguage" },
            { "DictionaryAudio", "SingleLanguage" },
            { "DictionaryMultiAudio", "SingleLanguage" },
            { "DictionaryPictures", "SingleLanguage" },
            { "UserRecords", "NoLanguage" },
            { "AnonymousUserRecords", "NoLanguage" },
            { "ChangeLogItems", "NoLanguage" },
            { "ForumCategories", "NoLanguage" },
            { "ForumHeadings", "NoLanguage" },
            { "ForumTopics", "NoLanguage" },
            { "ForumPostings", "NoLanguage" },
            { "CourseTreeCache", "SingleLanguage" },
            { "PlanTreeCache", "SingleLanguage" },
            { "TranslationCache", "DoubleLanguage" },
            { "TranslationWithAlternatesCache", "DoubleLanguage" }
        };

        public static Dictionary<string, string> RepositoryBaseNames = new Dictionary<string, string>()
        {
            { "UIStrings", "DatabaseItemBaseString" },
            { "UIText", "DatabaseItemBaseString" },
            { "LanguageDescriptions", "DatabaseItemLanguageDescription" },
            { "Dictionary", "DatabaseItemDictionaryEntry" },
            { "DictionaryStems", "DatabaseItemDictionaryEntry" },
            { "Definflections", "DatabaseItemDeinflection" },
            { "CourseHeaders", "DatabaseItemObjectReferenceNodeTree" },
            { "Courses", "DatabaseItemBaseObjectNodeTree" },
            { "StudyLists", "DatabaseItemContentStudyList" },
            { "MediaItems", "DatabaseItemContentMediaItem" },
            { "DocumentItems", "DatabaseItemContentDocumentItem" },
            { "Sandboxes", "DatabaseItemSandbox" },
            { "UserRunItems", "DatabaseItemUserRunItem" },
            { "ContentStatistics", "DatabaseItemContentStatistics" },
            { "MarkupTemplates", "DatabaseItemMarkupTemplate" },
            { "NodeMasters", "DatabaseItemNodeMaster" },
            { "PlanHeaders", "DatabaseItemObjectReferenceNodeTree" },
            { "Plans", "DatabaseItemBaseObjectNodeTree" },
            { "ToolProfiles", "DatabaseItemToolProfile" },
            { "ToolStudyLists", "DatabaseItemToolStudyList" },
            { "ToolSessions", "DatabaseItemToolSession" },
            { "LessonImages", "DatabaseItemImage" },
            { "ProfileImages", "DatabaseItemImage" },
            { "DictionaryAudio", "DatabaseItemAudioReference" },
            { "DictionaryMultiAudio", "DatabaseItemAudioMultiReference" },
            { "DictionaryPictures", "DatabaseItemPictureReference" },
            { "UserRecords", "DatabaseItemUserRecord" },
            { "AnonymousUserRecords", "DatabaseItemUserRecord" },
            { "ChangeLogItems", "DatabaseItemChangeLogItem" },
            { "ForumCategories", "DatabaseItemForumCategory" },
            { "ForumHeadings", "DatabaseItemForumHeading" },
            { "ForumTopics", "DatabaseItemForumTopic" },
            { "ForumPostings", "DatabaseItemForumPosting" },
            { "CourseTreeCache", "DatabaseItemTreeCache" },
            { "PlanTreeCache", "DatabaseItemTreeCache" },
            { "TranslationCache", "DatabaseItemBaseString" },
            { "TranslationWithAlternatesCache", "DatabaseItemBaseStrings" }
        };

        public static Dictionary<string, string> RepositoryItemClassNames = new Dictionary<string, string>()
        {
            { "UIStrings", "BaseString" },
            { "UIText", "BaseString" },
            { "LanguageDescriptions", "LanguageDescription" },
            { "Dictionary", "DictionaryEntry" },
            { "DictionaryStems", "DictionaryEntry" },
            { "Deinflections", "Deinflection" },
            { "CourseHeaders", "ObjectReferenceNodeTree" },
            { "Courses", "BaseObjectNodeTree" },
            { "StudyLists", "ContentStudyList" },
            { "MediaItems", "ContentMediaItem" },
            { "DocumentItems", "ContentDocumentItem" },
            { "Sandboxes", "Sandbox" },
            { "UserRunItems", "UserRunItem" },
            { "ContentStatistics", "ContentStatistics" },
            { "MarkupTemplates", "MarkupTemplate" },
            { "NodeMasters", "NodeMaster" },
            { "PlanHeaders", "ObjectReferenceNodeTree" },
            { "Plans", "BaseObjectNodeTree" },
            { "ToolProfiles", "ToolProfile" },
            { "ToolStudyLists", "ToolStudyList" },
            { "ToolSessions", "ToolSession" },
            { "LessonImages", "Image" },
            { "ProfileImages", "Image" },
            { "DictionaryAudio", "AudioReference" },
            { "DictionaryMultiAudio", "AudioMultiReference" },
            { "DictionaryPictures", "PictureReference" },
            { "UserRecords", "UserRecord" },
            { "AnonymousUserRecords", "UserRecord" },
            { "ChangeLogItems", "ChangeLogItem" },
            { "ForumCategories", "ForumCategory" },
            { "ForumHeadings", "ForumHeading" },
            { "ForumTopics", "ForumTopic" },
            { "ForumPostings", "ForumPosting" },
            { "CourseTreeCache", "TreeCache" },
            { "PlanTreeCache", "TreeCache" },
            { "TranslationCache", "BaseString" },
            { "TranslationWithAlternatesCache", "BaseStrings" }
        };

        public MainRepository(string repositoryName) : base(repositoryName)
        {
            ClearMainRepository();
        }

        public MainRepository()
        {
            ClearMainRepository();
        }

        public override void Clear()
        {
            base.Clear();
            ClearMainRepository();
        }

        public void ClearMainRepository()
        {
            _ContentPath = null;
            _ServerUrl = null;
            _UIStrings = null;
            _UIText = null;
            _LanguageDescriptions = null;
            _Dictionary = null;
            _DictionaryStems = null;
            _Deinflections = null;
            _CourseHeaders = null;
            _Courses = null;
            _StudyLists = null;
            _MediaItems = null;
            _DocumentItems = null;
            _Sandboxes = null;
            _UserRunItems = null;
            _ContentStatistics = null;
            _MarkupTemplates = null;
            _NodeMasters = null;
            _PlanHeaders = null;
            _Plans = null;
            _ToolProfiles = null;
            _ToolStudyLists = null;
            _ToolSessions = null;
            _LessonImages = null;
            _ProfileImages = null;
            _DictionaryAudio = null;
            _DictionaryMultiAudio = null;
            _DictionaryPictures = null;
            _UserRecords = null;
            _AnonymousUserRecords = null;
            _ChangeLogItems = null;
            _ForumCategories = null;
            _ForumHeadings = null;
            _ForumTopics = null;
            _ForumPostings = null;
            _CourseTreeCache = null;
            _PlanTreeCache = null;
            _TranslationCache = null;
            _TranslationWithAlternatesCache = null;
            _Mirror = null;
        }

        public virtual void Copy(IMainRepository other)
        {
        }

        public virtual void Initialize()
        {
            if (_UIStrings == null)
                _UIStrings = new LanguageBaseStringRepository(new LanguageObjectStore("UIStrings", 50));

            if (_UIText == null)
                _UIText = new LanguageBaseStringRepository(new LanguageObjectStore("UIText", 50));

            if (_LanguageDescriptions == null)
                _LanguageDescriptions = new LanguageDescriptionRepository(new ObjectStore("LanguageDescriptions", null, 50));

            if (_Dictionary == null)
                _Dictionary = new DictionaryRepository(new LanguageObjectStore("Dictionary", 50));

            if (_DictionaryStems == null)
                _DictionaryStems = new DictionaryRepository(new LanguageObjectStore("DictionaryStems", 50));

            if (_Deinflections == null)
                _Deinflections = new DeinflectionRepository(new LanguageObjectStore("Deinflections", 50));

            if (_CourseHeaders == null)
                _CourseHeaders = new NodeTreeReferenceRepository(new ObjectStore("CourseHeaders", null, 50));

            if (_Courses == null)
                _Courses = new NodeTreeRepository(new ObjectStore("Courses", null, 50));

            if (_StudyLists == null)
                _StudyLists = new ContentStudyListRepository(new ObjectStore("StudyLists", null, 50));

            if (_MediaItems == null)
                _MediaItems = new ContentMediaItemRepository(new ObjectStore("MediaItems", null, 50));

            if (_DocumentItems == null)
                _DocumentItems = new ContentDocumentItemRepository(new ObjectStore("DocumentItems", null, 50));

            if (_Sandboxes == null)
                _Sandboxes = new SandboxRepository(new ObjectStore("Sandboxes", null, 50));

            if (_UserRunItems == null)
                _UserRunItems = new UserRunItemRepository(new LanguageObjectStore("UserRunItems", 50));

            if (_ContentStatistics == null)
                _ContentStatistics = new ContentStatisticsRepository(new ObjectStore("ContentStatistics", null, 50));

            if (_MarkupTemplates == null)
                _MarkupTemplates = new MarkupTemplateRepository(new ObjectStore("MarkupTemplates", null, 50));

            if (_NodeMasters == null)
                _NodeMasters = new NodeMasterRepository(new ObjectStore("NodeMasters", null, 50));

            if (_PlanHeaders == null)
                _PlanHeaders = new NodeTreeReferenceRepository(new ObjectStore("PlanHeaders", null, 50));

            if (_Plans == null)
                _Plans = new NodeTreeRepository(new ObjectStore("Plans", null, 50));

            if (_ToolProfiles == null)
                _ToolProfiles = new ToolProfileRepository(new ObjectStore("ToolProfiles", null, 50));

            if (_ToolStudyLists == null)
                _ToolStudyLists = new ToolStudyListRepository(new ObjectStore("ToolStudyLists", null, 50));

            if (_ToolSessions == null)
                _ToolSessions = new ToolSessionRepository(new ObjectStore("ToolSessions", null, 50));

            if (_LessonImages == null)
                _LessonImages = new ImageRepository(new ObjectStore("LessonImages", null, 50));

            if (_ProfileImages == null)
                _ProfileImages = new ImageRepository(new ObjectStore("ProfileImages", null, 50));

            if (_DictionaryAudio == null)
                _DictionaryAudio = new AudioRepository(new LanguageObjectStore("DictionaryAudio", 50));

            if (_DictionaryMultiAudio == null)
                _DictionaryMultiAudio = new AudioMultiRepository(new LanguageObjectStore("DictionaryMultiAudio", 50));

            if (_DictionaryPictures == null)
                _DictionaryPictures = new PictureRepository(new LanguageObjectStore("DictionaryPictures", 50));

            if (_UserRecords == null)
                _UserRecords = new UserRecordRepository(new ObjectStore("UserRecords", null, 50));

            if (_AnonymousUserRecords == null)
                _AnonymousUserRecords = new AnonymousUserRecordRepository(new ObjectStore("AnonymousUserRecords", null, 50));

            if (_ChangeLogItems == null)
                _ChangeLogItems = new ChangeLogItemRepository(new ObjectStore("ChangeLogItems", null, 50));

            if (_ForumCategories == null)
                _ForumCategories = new ForumCategoryRepository(new ObjectStore("ForumCategories", null, 50));

            if (_ForumHeadings == null)
                _ForumHeadings = new ForumHeadingRepository(new ObjectStore("ForumHeadings", null, 50));

            if (_ForumTopics == null)
                _ForumTopics = new ForumTopicRepository(new ObjectStore("ForumTopics", null, 50));

            if (_ForumPostings == null)
                _ForumPostings = new ForumPostingRepository(new ObjectStore("ForumPostings", null, 50));

            if (_CourseTreeCache == null)
                _CourseTreeCache = new LanguageBaseStringRepository(new LanguageObjectStore("CourseTreeCache", 50));

            if (_PlanTreeCache == null)
                _PlanTreeCache = new LanguageBaseStringRepository(new LanguageObjectStore("PlanTreeCache", 50));

            if (_TranslationCache == null)
                _TranslationCache = new LanguagePairBaseStringRepository(new LanguagePairObjectStore("TranslationCache", 50));

            if (_TranslationWithAlternatesCache == null)
                _TranslationWithAlternatesCache = new LanguagePairBaseStringsRepository(new LanguagePairObjectStore("TranslationWithAlternatesCache", 50));
        }

        public virtual void CopyFrom(IMainRepository other)
        {
            if (other != null)
            {
                int index = 0;

                UpdateProgressCheck(index++);

                if (_UIStrings != null)
                    _UIStrings.CopyFrom(other.UIStrings, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_UIText != null)
                    _UIText.CopyFrom(other.UIText, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_LanguageDescriptions != null)
                    _LanguageDescriptions.CopyFrom(other.LanguageDescriptions, 0, -1);

                UpdateProgressCheck(index++);

                if (_Dictionary != null)
                    _Dictionary.CopyFrom(other.Dictionary, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_DictionaryStems != null)
                    _DictionaryStems.CopyFrom(other.DictionaryStems, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_Deinflections != null)
                    _Deinflections.CopyFrom(other.Deinflections, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_CourseHeaders != null)
                    _CourseHeaders.CopyFrom(other.CourseHeaders, 0, -1);

                UpdateProgressCheck(index++);

                if (_Courses != null)
                    _Courses.CopyFrom(other.Courses, 0, -1);

                UpdateProgressCheck(index++);

                if (_StudyLists != null)
                    _StudyLists.CopyFrom(other.StudyLists, 0, -1);

                UpdateProgressCheck(index++);

                if (_MediaItems != null)
                    _MediaItems.CopyFrom(other.MediaItems, 0, -1);

                UpdateProgressCheck(index++);

                if (_DocumentItems != null)
                    _DocumentItems.CopyFrom(other.DocumentItems, 0, -1);

                UpdateProgressCheck(index++);

                if (_Sandboxes != null)
                    _Sandboxes.CopyFrom(other.Sandboxes, 0, -1);

                UpdateProgressCheck(index++);

                if (_UserRunItems != null)
                    _UserRunItems.CopyFrom(other.UserRunItems, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_ContentStatistics != null)
                    _ContentStatistics.CopyFrom(other.ContentStatistics, 0, -1);

                UpdateProgressCheck(index++);

                if (_MarkupTemplates != null)
                    _MarkupTemplates.CopyFrom(other.MarkupTemplates, 0, -1);

                UpdateProgressCheck(index++);

                if (_NodeMasters != null)
                    _NodeMasters.CopyFrom(other.NodeMasters, 0, -1);

                UpdateProgressCheck(index++);

                if (_PlanHeaders != null)
                    _PlanHeaders.CopyFrom(other.PlanHeaders, 0, -1);

                UpdateProgressCheck(index++);

                if (_Plans != null)
                    _Plans.CopyFrom(other.Plans, 0, -1);

                UpdateProgressCheck(index++);

                if (_ToolProfiles != null)
                    _ToolProfiles.CopyFrom(other.ToolProfiles, 0, -1);

                UpdateProgressCheck(index++);

                if (_ToolStudyLists != null)
                    _ToolStudyLists.CopyFrom(other.ToolStudyLists, 0, -1);

                UpdateProgressCheck(index++);

                if (_ToolSessions != null)
                    _ToolSessions.CopyFrom(other.ToolSessions, 0, -1);

                UpdateProgressCheck(index++);

                if (_LessonImages != null)
                    _LessonImages.CopyFrom(other.LessonImages, 0, -1);

                UpdateProgressCheck(index++);

                if (_ProfileImages != null)
                    _ProfileImages.CopyFrom(other.ProfileImages, 0, -1);

                UpdateProgressCheck(index++);

                if (_DictionaryAudio != null)
                    _DictionaryAudio.CopyFrom(other.DictionaryAudio, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_DictionaryMultiAudio != null)
                    _DictionaryMultiAudio.CopyFrom(other.DictionaryMultiAudio, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_DictionaryPictures != null)
                    _DictionaryPictures.CopyFrom(other.DictionaryPictures, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_UserRecords != null)
                    _UserRecords.CopyFrom(other.UserRecords, 0, -1);

                UpdateProgressCheck(index++);

                if (_AnonymousUserRecords != null)
                    _AnonymousUserRecords.CopyFrom(other.AnonymousUserRecords, 0, -1);

                UpdateProgressCheck(index++);

                if (_ChangeLogItems != null)
                    _ChangeLogItems.CopyFrom(other.ChangeLogItems, 0, -1);

                UpdateProgressCheck(index++);

                if (_ForumCategories != null)
                    _ForumCategories.CopyFrom(other.ForumCategories, 0, -1);

                UpdateProgressCheck(index++);

                if (_ForumHeadings != null)
                    _ForumHeadings.CopyFrom(other.ForumHeadings, 0, -1);

                UpdateProgressCheck(index++);

                if (_ForumTopics != null)
                    _ForumTopics.CopyFrom(other.ForumTopics, 0, -1);

                UpdateProgressCheck(index++);

                if (_ForumPostings != null)
                    _ForumPostings.CopyFrom(other.ForumPostings, 0, -1);

                UpdateProgressCheck(index++);

                if (_CourseTreeCache != null)
                    _CourseTreeCache.CopyFrom(other.CourseTreeCache, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_PlanTreeCache != null)
                    _PlanTreeCache.CopyFrom(other.PlanTreeCache, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_TranslationCache != null)
                    _TranslationCache.CopyFrom(other.TranslationCache, null, null, 0, -1);

                UpdateProgressCheck(index++);

                if (_TranslationWithAlternatesCache != null)
                    _TranslationWithAlternatesCache.CopyFrom(other.TranslationWithAlternatesCache, null, null, 0, -1);

                UpdateProgressCheck(index++);
            }
        }

        public virtual void CreateStore()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.CreateStore();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.CreateStore();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.CreateStore();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.CreateStore();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.CreateStore();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.CreateStore();

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                _CourseHeaders.CreateStore();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.CreateStore();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.CreateStore();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.CreateStore();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.CreateStore();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.CreateStore();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.CreateStore();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.CreateStore();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.CreateStore();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.CreateStore();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.CreateStore();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.CreateStore();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.CreateStore();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.CreateStore();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.CreateStore();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.CreateStore();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.CreateStore();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.CreateStore();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.CreateStore();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.CreateStore();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.CreateStore();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.CreateStore();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.CreateStore();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.CreateStore();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.CreateStore();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.CreateStore();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.CreateStore();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.CreateStore();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.CreateStore();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.CreateStore();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.CreateStore();

            UpdateProgressCheck(index++);
        }

        public virtual void CreateStoreCheck()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                _CourseHeaders.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.CreateStoreCheck();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.CreateStoreCheck();

            UpdateProgressCheck(index++);
        }

        public virtual void DeleteStore()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.DeleteStore();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.DeleteStore();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.DeleteStore();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.DeleteStore();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.DeleteStore();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.DeleteStore();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.DeleteStore();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.DeleteStore();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.DeleteStore();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.DeleteStore();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.DeleteStore();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.DeleteStore();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.DeleteStore();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.DeleteStore();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.DeleteStore();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.DeleteStore();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.DeleteStore();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.DeleteStore();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.DeleteStore();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.DeleteStore();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.DeleteStore();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.DeleteStore();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.DeleteStore();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.DeleteStore();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.DeleteStore();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.DeleteStore();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.DeleteStore();

            UpdateProgressCheck(index++);
        }

        public virtual void DeleteStoreCheck()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                _CourseHeaders.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.DeleteStoreCheck();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.DeleteStoreCheck();

            UpdateProgressCheck(index++);
        }

        public virtual void DeleteAll()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.DeleteAll();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.DeleteAll();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.DeleteAll();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.DeleteAll();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.DeleteAll();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.DeleteAll();

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                _CourseHeaders.DeleteAll();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.DeleteAll();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.DeleteAll();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.DeleteAll();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.DeleteAll();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.DeleteAll();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.DeleteAll();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.DeleteAll();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.DeleteAll();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.DeleteAll();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.DeleteAll();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.DeleteAll();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.DeleteAll();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.DeleteAll();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.DeleteAll();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.DeleteAll();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.DeleteAll();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.DeleteAll();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.DeleteAll();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.DeleteAll();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.DeleteAll();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.DeleteAll();

            UpdateProgressCheck(index++);
        }

        public virtual string ContentPath
        {
            get
            {
                return _ContentPath;
            }
            set
            {
                _ContentPath = value;
            }
        }

        public virtual string ServerUrl
        {
            get
            {
                return _ServerUrl;
            }
            set
            {
                _ServerUrl = value;
            }
        }

        public int RepositoryCount
        {
            get { return _RepositoryCount; }
            set { _RepositoryCount = value; }
        }

        public List<string> RepositoryNames
        {
            get
            {
                return new List<string>()
                    {
                        "UIStrings",
                        "UIText",
                        "LanguageDescriptions",
                        "Dictionary",
                        "DictionaryStems",
                        "Deinflections",
                        "CourseHeaders",
                        "Courses",
                        "StudyLists",
                        "MediaItems",
                        "DocumentItems",
                        "Sandboxes",
                        "UserRunItems",
                        "MarkupTemplates",
                        "NodeMasters",
                        "PlanHeaders",
                        "Plans",
                        "ToolProfiles",
                        "ToolStudyLists",
                        "ToolSessions",
                        "LessonImages",
                        "ProfileImages",
                        "DictionaryAudio",
                        "DictionaryMultiAudio",
                        "DictionaryPictures",
                        "UserRecords",
                        "AnonymousUserRecords",
                        "ChangeLogItems",
                        "ForumCategories",
                        "ForumHeadings",
                        "ForumTopics",
                        "ForumPostings",
                        "CourseTreeCache",
                        "PlanTreeCache",
                        "TranslationCache",
                        "TranslationWithAlternatesCache"
                    };
            }
        }

        public LanguageBaseStringRepository UIStrings
        {
            get
            {
                return _UIStrings;
            }
            set
            {
                _UIStrings = value;
            }
        }

        public LanguageBaseStringRepository UIText
        {
            get
            {
                return _UIText;
            }
            set
            {
                _UIText = value;
            }
        }

        public virtual LanguageDescriptionRepository LanguageDescriptions
        {
            get
            {
                return _LanguageDescriptions;
            }
            set
            {
                _LanguageDescriptions = value;
            }
        }

        public virtual DictionaryRepository Dictionary
        {
            get
            {
                return _Dictionary;
            }
            set
            {
                _Dictionary = value;
            }
        }

        public virtual DictionaryRepository DictionaryStems
        {
            get
            {
                return _DictionaryStems;
            }
            set
            {
                _DictionaryStems = value;
            }
        }

        public virtual DeinflectionRepository Deinflections
        {
            get
            {
                return _Deinflections;
            }
            set
            {
                _Deinflections = value;
            }
        }

        public virtual NodeTreeReferenceRepository CourseHeaders
        {
            get
            {
                return _CourseHeaders;
            }
            set
            {
                _CourseHeaders = value;
            }
        }

        public virtual NodeTreeRepository Courses
        {
            get
            {
                return _Courses;
            }
            set
            {
                _Courses = value;
            }
        }

        public virtual ContentStudyListRepository StudyLists
        {
            get
            {
                return _StudyLists;
            }
            set
            {
                _StudyLists = value;
            }
        }

        public virtual ContentMediaItemRepository MediaItems
        {
            get
            {
                return _MediaItems;
            }
            set
            {
                _MediaItems = value;
            }
        }

        public virtual ContentDocumentItemRepository DocumentItems
        {
            get
            {
                return _DocumentItems;
            }
            set
            {
                _DocumentItems = value;
            }
        }

        public virtual SandboxRepository Sandboxes
        {
            get
            {
                return _Sandboxes;
            }
            set
            {
                _Sandboxes = value;
            }
        }

        public virtual UserRunItemRepository UserRunItems
        {
            get
            {
                return _UserRunItems;
            }
            set
            {
                _UserRunItems = value;
            }
        }

        public virtual ContentStatisticsRepository ContentStatistics
        {
            get
            {
                return _ContentStatistics;
            }
            set
            {
                _ContentStatistics = value;
            }
        }

        public virtual MarkupTemplateRepository MarkupTemplates
        {
            get
            {
                return _MarkupTemplates;
            }
            set
            {
                _MarkupTemplates = value;
            }
        }

        public virtual NodeMasterRepository NodeMasters
        {
            get
            {
                return _NodeMasters;
            }
            set
            {
                _NodeMasters = value;
            }
        }

        public virtual NodeTreeReferenceRepository PlanHeaders
        {
            get
            {
                return _PlanHeaders;
            }
            set
            {
                _PlanHeaders = value;
            }
        }

        public virtual NodeTreeRepository Plans
        {
            get
            {
                return _Plans;
            }
            set
            {
                _Plans = value;
            }
        }

        public virtual ToolProfileRepository ToolProfiles
        {
            get
            {
                return _ToolProfiles;
            }
            set
            {
                _ToolProfiles = value;
            }
        }

        public virtual ToolStudyListRepository ToolStudyLists
        {
            get
            {
                return _ToolStudyLists;
            }
            set
            {
                _ToolStudyLists = value;
            }
        }

        public virtual ToolSessionRepository ToolSessions
        {
            get
            {
                return _ToolSessions;
            }
            set
            {
                _ToolSessions = value;
            }
        }

        public virtual ImageRepository LessonImages
        {
            get
            {
                return _LessonImages;
            }
            set
            {
                _LessonImages = value;
            }
        }

        public virtual ImageRepository ProfileImages
        {
            get
            {
                return _ProfileImages;
            }
            set
            {
                _ProfileImages = value;
            }
        }

        public virtual AudioRepository DictionaryAudio
        {
            get
            {
                return _DictionaryAudio;
            }
            set
            {
                _DictionaryAudio = value;
            }
        }

        public virtual AudioMultiRepository DictionaryMultiAudio
        {
            get
            {
                return _DictionaryMultiAudio;
            }
            set
            {
                _DictionaryMultiAudio = value;
            }
        }

        public virtual PictureRepository DictionaryPictures
        {
            get
            {
                return _DictionaryPictures;
            }
            set
            {
                _DictionaryPictures = value;
            }
        }

        public virtual UserRecordRepository UserRecords
        {
            get
            {
                return _UserRecords;
            }
            set
            {
                _UserRecords = value;
            }
        }

        public virtual AnonymousUserRecordRepository AnonymousUserRecords
        {
            get
            {
                return _AnonymousUserRecords;
            }
            set
            {
                _AnonymousUserRecords = value;
            }
        }

        public virtual ChangeLogItemRepository ChangeLogItems
        {
            get
            {
                return _ChangeLogItems;
            }
            set
            {
                _ChangeLogItems = value;
            }
        }

        public virtual ForumCategoryRepository ForumCategories
        {
            get
            {
                return _ForumCategories;
            }
            set
            {
                _ForumCategories = value;
            }
        }

        public virtual ForumHeadingRepository ForumHeadings
        {
            get
            {
                return _ForumHeadings;
            }
            set
            {
                _ForumHeadings = value;
            }
        }

        public virtual ForumTopicRepository ForumTopics
        {
            get
            {
                return _ForumTopics;
            }
            set
            {
                _ForumTopics = value;
            }
        }

        public virtual ForumPostingRepository ForumPostings
        {
            get
            {
                return _ForumPostings;
            }
            set
            {
                _ForumPostings = value;
            }
        }

        public LanguageBaseStringRepository CourseTreeCache
        {
            get
            {
                return _CourseTreeCache;
            }
            set
            {
                _CourseTreeCache = value;
            }
        }

        public LanguageBaseStringRepository PlanTreeCache
        {
            get
            {
                return _PlanTreeCache;
            }
            set
            {
                _PlanTreeCache = value;
            }
        }

        public LanguagePairBaseStringRepository TranslationCache
        {
            get
            {
                return _TranslationCache;
            }
            set
            {
                _TranslationCache = value;
            }
        }

        public LanguagePairBaseStringsRepository TranslationWithAlternatesCache
        {
            get
            {
                return _TranslationWithAlternatesCache;
            }
            set
            {
                _TranslationWithAlternatesCache = value;
            }
        }

        public virtual bool CheckReference(string source, LanguageID languageID, object key)
        {
            bool returnValue = false;

            switch (source)
            {
                case "UIStrings":
                    returnValue = UIStrings.Contains(key, languageID);
                    break;
                case "UIText":
                    returnValue = UIText.Contains(key, languageID);
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.Contains(key);
                    break;
                case "Dictionary":
                    returnValue = Dictionary.Contains(key, languageID);
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.Contains(key, languageID);
                    break;
                case "Deinflections":
                    returnValue = Deinflections.Contains(key, languageID);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.Contains(key);
                    break;
                case "Courses":
                    returnValue = Courses.Contains(key);
                    break;
                case "StudyLists":
                    returnValue = StudyLists.Contains(key);
                    break;
                case "MediaItems":
                    returnValue = MediaItems.Contains(key);
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.Contains(key);
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.Contains(key);
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.Contains(key, languageID);
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.Contains(key);
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.Contains(key);
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.Contains(key);
                    break;
                case "Plans":
                    returnValue = Plans.Contains(key);
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.Contains(key);
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.Contains(key);
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.Contains(key);
                    break;
                case "LessonImages":
                    returnValue = LessonImages.Contains(key);
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.Contains(key);
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.Contains(key, languageID);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.Contains(key, languageID);
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.Contains(key, languageID);
                    break;
                case "UserRecords":
                    returnValue = UserRecords.Contains(key);
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.Contains(key);
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.Contains(key);
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.Contains(key);
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.Contains(key);
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.Contains(key);
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.Contains(key);
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.Contains(key, languageID);
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.Contains(key, languageID);
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.Contains(key, languageID, null);
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.Contains(key, languageID, null);
                    break;
                default:
                    throw new ObjectException("CheckReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public virtual IBaseObjectKeyed ResolveGuidReference(string source, LanguageID languageID, Guid guid)
        {
            IBaseObjectKeyed returnValue = null;

            switch (source)
            {
                case "StudyLists":
                    returnValue = StudyLists.GetGuid(guid);
                    break;
                case "MediaItems":
                    returnValue = MediaItems.GetGuid(guid);
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.GetGuid(guid);
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.GetGuid(guid);
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.GetGuid(guid);
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.GetGuid(guid);
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.GetGuid(guid);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.GetGuid(guid);
                    break;
                default:
                    throw new ObjectException("ResolveGuidReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public virtual bool SaveGuidReference(string source, LanguageID languageID, IBaseObjectKeyed item, bool update)
        {
            IBaseObjectKeyed existingItem = null;

            if (item == null)
                return false;

            Guid guid = item.Guid;

            if (guid != Guid.Empty)
            {
                switch (source)
                {
                    case "StudyLists":
                        existingItem = StudyLists.GetGuid(guid);
                        break;
                    case "MediaItems":
                        existingItem = MediaItems.GetGuid(guid);
                        break;
                    case "DocumentItems":
                        existingItem = DocumentItems.GetGuid(guid);
                        break;
                    case "Sandboxes":
                        existingItem = Sandboxes.GetGuid(guid);
                        break;
                    case "MarkupTemplates":
                        existingItem = MarkupTemplates.GetGuid(guid);
                        break;
                    case "NodeMasters":
                        existingItem = NodeMasters.GetGuid(guid);
                        break;
                    case "PlanHeaders":
                        existingItem = PlanHeaders.GetGuid(guid);
                        break;
                    case "CourseHeaders":
                        existingItem = CourseHeaders.GetGuid(guid);
                        break;
                    default:
                        throw new ObjectException("SaveGuidReference: Unknown source \"" + source + "\".");
                }

                if (existingItem == null)
                {
                    item.Key = 0;
                    item.Modified = false;
                    return SaveReference(source, languageID, item);
                }
                else
                {
                    item.Key = existingItem.Key;
                    item.Modified = false;
                    return UpdateReference(source, languageID, item);
                }
            }
            else
            {
                item.EnsureGuid();
                item.Modified = false;

                if (update)
                    return UpdateReference(source, languageID, item);
                else
                    return SaveReference(source, languageID, item);
            }
        }

        public virtual IBaseObjectKeyed ResolveNamedReference(string source, LanguageID languageID, string owner, string name)
        {
            IBaseObjectKeyed returnValue = null;

            switch (source)
            {
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.GetNamed(owner, name);
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.GetNamed(owner, name);
                    break;
                case "LessonImages":
                    returnValue = LessonImages.GetImage(owner, name);
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.GetAudio(name, languageID);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.GetAudio(name, languageID);
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.GetPicture(name, languageID);
                    break;
                case "Dictionary":
                    returnValue = Dictionary.Get(name, languageID);
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.Get(name, languageID);
                    break;
                case "Deinflections":
                    returnValue = Deinflections.Get(name, languageID);
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.GetNamed(owner, name);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.GetNamed(owner, name);
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.Get(name);
                    break;
                case "UserRecords":
                    returnValue = UserRecords.Get(name);
                    break;
                case "UIText":
                    returnValue = UIText.Get(name, languageID);
                    break;
                case "UIStrings":
                    returnValue = UIStrings.Get(name, languageID);
                    break;
                default:
                    throw new ObjectException("ResolveNamedReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public virtual bool SaveNamedReference(string source, LanguageID languageID, IBaseObjectKeyed item, bool update)
        {
            IBaseObjectKeyed existingItem = null;

            if (item == null)
                return false;

            string owner = item.Owner;
            string name = item.Name;

            switch (source)
            {
                case "MarkupTemplates":
                    existingItem = MarkupTemplates.GetNamed(owner, name);
                    break;
                case "NodeMasters":
                    existingItem = NodeMasters.GetNamed(owner, name);
                    break;
                case "LessonImages":
                    existingItem = LessonImages.GetImage(owner, name);
                    break;
                case "DictionaryAudio":
                    existingItem = DictionaryAudio.GetAudio(name, languageID);
                    break;
                case "DictionaryMultiAudio":
                    existingItem = DictionaryMultiAudio.GetAudio(name, languageID);
                    break;
                case "DictionaryPictures":
                    existingItem = DictionaryPictures.GetPicture(name, languageID);
                    break;
                case "Dictionary":
                    existingItem = Dictionary.Get(name, languageID);
                    break;
                case "DictionaryStems":
                    existingItem = DictionaryStems.Get(name, languageID);
                    break;
                case "Deinflections":
                    existingItem = Deinflections.Get(name, languageID);
                    break;
                case "PlanHeaders":
                    existingItem = PlanHeaders.GetNamed(owner, name);
                    break;
                case "CourseHeaders":
                    existingItem = CourseHeaders.GetNamed(owner, name);
                    break;
                case "ToolProfiles":
                    existingItem = ToolProfiles.Get(name);
                    break;
                case "UserRecords":
                    existingItem = UserRecords.Get(name);
                    break;
                case "UIText":
                    existingItem = UIText.Get(name, languageID);
                    break;
                case "UIStrings":
                    existingItem = UIStrings.Get(name, languageID);
                    break;
                default:
                    throw new ObjectException("SaveNameReference: Unknown source \"" + source + "\".");
            }

            if (existingItem == null)
            {
                item.Key = 0;
                item.Modified = false;
                return SaveReference(source, languageID, item);
            }
            else if (update)
            {
                item.Key = existingItem.Key;
                item.Modified = false;
                return UpdateReference(source, languageID, item);
            }

            return true;
        }

        public virtual IBaseObjectKeyed ResolveReference(string source, LanguageID languageID, object key)
        {
            IBaseObjectKeyed returnValue = null;
 
            switch (source)
            {
                case "UIStrings":
                    returnValue = UIStrings.Get(key, languageID);
                    break;
                case "UIText":
                    returnValue = UIText.Get(key, languageID);
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.Get(key);
                    break;
                case "Dictionary":
                    returnValue = Dictionary.Get(key, languageID);
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.Get(key, languageID);
                    break;
                case "Deinflections":
                    returnValue = Deinflections.Get(key, languageID);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.Get(key);
                    break;
                case "Courses":
                    returnValue = Courses.Get(key);
                    break;
                case "StudyLists":
                    returnValue = StudyLists.Get(key);
                    break;
                case "MediaItems":
                    returnValue = MediaItems.Get(key);
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.Get(key);
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.Get(key);
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.Get(key, languageID);
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.Get(key);
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.Get(key);
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.Get(key);
                    break;
                case "Plans":
                    returnValue = Plans.Get(key);
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.Get(key);
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.Get(key);
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.Get(key);
                    break;
                case "LessonImages":
                    returnValue = LessonImages.Get(key);
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.Get(key);
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.Get(key, languageID);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.Get(key, languageID);
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.Get(key, languageID);
                    break;
                case "UserRecords":
                    returnValue = UserRecords.Get(key);
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.Get(key);
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.Get(key);
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.Get(key);
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.Get(key);
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.Get(key);
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.Get(key);
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.Get(key, languageID);
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.Get(key, languageID);
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.Get(key, languageID, null);
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.Get(key, languageID, null);
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public virtual IBaseObjectKeyed CacheCheckReference(string source, LanguageID languageID, IBaseObjectKeyed item)
        {
            IBaseObjectKeyed returnValue = null;

            switch (source)
            {
                case "UIStrings":
                    returnValue = UIStrings.CacheCheckObject(item, languageID);
                    break;
                case "UIText":
                    returnValue = UIText.CacheCheckObject(item, languageID);
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.CacheCheckObject(item);
                    break;
                case "Dictionary":
                    returnValue = Dictionary.CacheCheckObject(item, languageID);
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.CacheCheckObject(item, languageID);
                    break;
                case "Deinflections":
                    returnValue = Deinflections.CacheCheckObject(item, languageID);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.CacheCheckObject(item);
                    break;
                case "Courses":
                    returnValue = Courses.CacheCheckObject(item);
                    break;
                case "StudyLists":
                    returnValue = StudyLists.CacheCheckObject(item);
                    break;
                case "MediaItems":
                    returnValue = MediaItems.CacheCheckObject(item);
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.CacheCheckObject(item);
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.CacheCheckObject(item);
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.CacheCheckObject(item, languageID);
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.CacheCheckObject(item);
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.CacheCheckObject(item);
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.CacheCheckObject(item);
                    break;
                case "Plans":
                    returnValue = Plans.CacheCheckObject(item);
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.CacheCheckObject(item);
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.CacheCheckObject(item);
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.CacheCheckObject(item);
                    break;
                case "LessonImages":
                    returnValue = LessonImages.CacheCheckObject(item);
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.CacheCheckObject(item);
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.CacheCheckObject(item, languageID);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.CacheCheckObject(item, languageID);
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.CacheCheckObject(item, languageID);
                    break;
                case "UserRecords":
                    returnValue = UserRecords.CacheCheckObject(item);
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.CacheCheckObject(item);
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.CacheCheckObject(item);
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.CacheCheckObject(item);
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.CacheCheckObject(item);
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.CacheCheckObject(item);
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.CacheCheckObject(item);
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.CacheCheckObject(item, languageID);
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.CacheCheckObject(item, languageID);
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.CacheCheckObject(item, languageID, null);
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.CacheCheckObject(item, languageID, null);
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        // Returns true if added an object.
        public virtual bool SaveReference(string source, LanguageID languageID, IBaseObjectKeyed item)
        {
            bool returnValue = true;

            if (item == null)
                return returnValue;

            switch (source)
            {
                case "UIStrings":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = UIStrings.Add(obj, languageID);
                    }
                    break;
                case "UIText":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = UIText.Add(obj, languageID);
                    }
                    break;
                case "LanguageDescriptions":
                    {
                        LanguageDescription obj = item as LanguageDescription;
                        if (obj == null)
                            throw new ObjectException("Not a LanguageDescription");
                        returnValue = LanguageDescriptions.Add(obj);
                    }
                    break;
                case "Dictionary":
                    {
                        DictionaryEntry obj = item as DictionaryEntry;
                        if (obj == null)
                            throw new ObjectException("Not a DictionaryEntry");
                        returnValue = Dictionary.Add(obj, languageID);
                    }
                    break;
                case "DictionaryStems":
                    {
                        DictionaryEntry obj = item as DictionaryEntry;
                        if (obj == null)
                            throw new ObjectException("Not a DictionaryEntry");
                        returnValue = DictionaryStems.Add(obj, languageID);
                    }
                    break;
                case "Deinflections":
                    {
                        Deinflection obj = item as Deinflection;
                        if (obj == null)
                            throw new ObjectException("Not a DictionaryEntry");
                        returnValue = Deinflections.Add(obj, languageID);
                    }
                    break;
                case "CourseHeaders":
                    {
                        ObjectReferenceNodeTree obj = item as ObjectReferenceNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree header");
                        obj.Modified = false;
                        returnValue = CourseHeaders.Add(obj);
                    }
                    break;
                case "Courses":
                    {
                        BaseObjectNodeTree obj = item as BaseObjectNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree");
                        obj.Modified = false;
                        returnValue = Courses.Add(obj);
                    }
                    break;
                case "StudyLists":
                    {
                        ContentStudyList obj = item as ContentStudyList;
                        if (obj == null)
                            throw new ObjectException("Not a ContentStudyList");
                        obj.Modified = false;
                        returnValue = StudyLists.Add(obj);
                    }
                    break;
                case "MediaItems":
                    {
                        ContentMediaItem obj = item as ContentMediaItem;
                        if (obj == null)
                            throw new ObjectException("Not a ContentMediaItem");
                        obj.Modified = false;
                        returnValue = MediaItems.Add(obj);
                    }
                    break;
                case "DocumentItems":
                    {
                        ContentDocumentItem obj = item as ContentDocumentItem;
                        if (obj == null)
                            throw new ObjectException("Not a ContentDocumentItem");
                        obj.Modified = false;
                        returnValue = DocumentItems.Add(obj);
                    }
                    break;
                case "Sandboxes":
                    {
                        Sandbox obj = item as Sandbox;
                        if (obj == null)
                            throw new ObjectException("Not a Sandbox");
                        obj.Modified = false;
                        returnValue = Sandboxes.Add(obj);
                    }
                    break;
                case "UserRunItems":
                    {
                        UserRunItem obj = item as UserRunItem;
                        if (obj == null)
                            throw new ObjectException("Not a UserRunItem");
                        obj.Modified = false;
                        returnValue = UserRunItems.Add(obj, languageID);
                    }
                    break;
                case "MarkupTemplates":
                    {
                        MarkupTemplate obj = item as MarkupTemplate;
                        if (obj == null)
                            throw new ObjectException("Not a MarkupTemplate");
                        obj.Modified = false;
                        returnValue = MarkupTemplates.Add(obj);
                    }
                    break;
                case "NodeMasters":
                    {
                        NodeMaster obj = item as NodeMaster;
                        if (obj == null)
                            throw new ObjectException("Not a NodeMaster");
                        obj.Modified = false;
                        returnValue = NodeMasters.Add(obj);
                    }
                    break;
                case "PlanHeaders":
                    {
                        ObjectReferenceNodeTree obj = item as ObjectReferenceNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree header");
                        obj.Modified = false;
                        returnValue = PlanHeaders.Add(obj);
                    }
                    break;
                case "Plans":
                    {
                        BaseObjectNodeTree obj = item as BaseObjectNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree");
                        obj.Modified = false;
                        returnValue = Plans.Add(obj);
                    }
                    break;
                case "ToolProfiles":
                    {
                        ToolProfile obj = item as ToolProfile;
                        if (obj == null)
                            throw new ObjectException("Not a ToolProfiles");
                        obj.Modified = false;
                        returnValue = ToolProfiles.Add(obj);
                    }
                    break;
                case "ToolStudyLists":
                    {
                        ToolStudyList obj = item as ToolStudyList;
                        if (obj == null)
                            throw new ObjectException("Not a ToolStudyList");
                        obj.Modified = false;
                        returnValue = ToolStudyLists.Add(obj);
                    }
                    break;
                case "ToolSessions":
                    {
                        ToolSession obj = item as ToolSession;
                        if (obj == null)
                            throw new ObjectException("Not a ToolSession");
                        obj.Modified = false;
                        returnValue = ToolSessions.Add(obj);
                    }
                    break;
                case "LessonImages":
                    {
                        Image obj = item as Image;
                        if (obj == null)
                            throw new ObjectException("Not an Image");
                        returnValue = LessonImages.Add(obj);
                    }
                    break;
                case "ProfileImages":
                    {
                        Image obj = item as Image;
                        if (obj == null)
                            throw new ObjectException("Not an Image");
                        returnValue = ProfileImages.Add(obj);
                    }
                    break;
                case "DictionaryAudio":
                    {
                        AudioReference obj = item as AudioReference;
                        if (obj == null)
                            throw new ObjectException("Not an AudioReference object");
                        returnValue = DictionaryAudio.Add(obj, languageID);
                    }
                    break;
                case "DictionaryMultiAudio":
                    {
                        AudioMultiReference obj = item as AudioMultiReference;
                        if (obj == null)
                            throw new ObjectException("Not an AudioMultiReference object");
                        returnValue = DictionaryMultiAudio.Add(obj, languageID);
                    }
                    break;
                case "DictionaryPictures":
                    {
                        PictureReference obj = item as PictureReference;
                        if (obj == null)
                            throw new ObjectException("Not an PictureReference object");
                        returnValue = DictionaryPictures.Add(obj, languageID);
                    }
                    break;
                case "UserRecords":
                    {
                        UserRecord obj = item as UserRecord;
                        if (obj == null)
                            throw new ObjectException("Not a UserRecord");
                        returnValue = UserRecords.Add(obj);
                    }
                    break;
                case "AnonymousUserRecords":
                    {
                        AnonymousUserRecord obj = item as AnonymousUserRecord;
                        if (obj == null)
                            throw new ObjectException("Not a UserRecord");
                        returnValue = AnonymousUserRecords.Add(obj);
                    }
                    break;
                case "ChangeLogItems":
                    {
                        ChangeLogItem obj = item as ChangeLogItem;
                        if (obj == null)
                            throw new ObjectException("Not a ChangeLogItem");
                        returnValue = ChangeLogItems.Add(obj);
                    }
                    break;
                case "ForumCategories":
                    {
                        ForumCategory obj = item as ForumCategory;
                        if (obj == null)
                            throw new ObjectException("Not a ForumCategory");
                        returnValue = ForumCategories.Add(obj);
                    }
                    break;
                case "ForumHeadings":
                    {
                        ForumHeading obj = item as ForumHeading;
                        if (obj == null)
                            throw new ObjectException("Not a ForumHeading");
                        returnValue = ForumHeadings.Add(obj);
                    }
                    break;
                case "ForumTopics":
                    {
                        ForumTopic obj = item as ForumTopic;
                        if (obj == null)
                            throw new ObjectException("Not a ForumTopic");
                        returnValue = ForumTopics.Add(obj);
                    }
                    break;
                case "ForumPostings":
                    {
                        ForumPosting obj = item as ForumPosting;
                        if (obj == null)
                            throw new ObjectException("Not a ForumPosting");
                        returnValue = ForumPostings.Add(obj);
                    }
                    break;
                case "CourseTreeCache":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = CourseTreeCache.Add(obj, languageID);
                    }
                    break;
                case "PlanTreeCache":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = PlanTreeCache.Add(obj, languageID);
                    }
                    break;
                case "TranslationWithAlternatesCache":
                    {
                        BaseStrings obj = item as BaseStrings;
                        if (obj == null)
                            throw new ObjectException("Not a BaseStrings");
                        returnValue = TranslationWithAlternatesCache.Add(obj, languageID, null);
                    }
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public virtual List<IBaseObjectKeyed> ResolveListReference(string source, LanguageID languageID, Matcher matcher)
        {
            List<IBaseObjectKeyed> returnValue = null;

            switch (source)
            {
                case "UIStrings":
                    returnValue = UIStrings.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "UIText":
                    returnValue = UIText.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Dictionary":
                    returnValue = Dictionary.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Deinflections":
                    returnValue = Deinflections.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Courses":
                    returnValue = Courses.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "StudyLists":
                    returnValue = StudyLists.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "MediaItems":
                    returnValue = MediaItems.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Plans":
                    returnValue = Plans.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "LessonImages":
                    returnValue = LessonImages.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "UserRecords":
                    returnValue = UserRecords.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.Query(matcher, languageID, null).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.Query(matcher, languageID, null).Cast<IBaseObjectKeyed>().ToList();
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        // Returns true if successfull.
        public virtual bool UpdateReference(string source, LanguageID languageID, IBaseObjectKeyed item)
        {
            bool returnValue = true;

            if (item == null)
                return returnValue;

            switch (source)
            {
                case "UIStrings":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = UIStrings.Update(obj, languageID);
                    }
                    break;
                case "UIText":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = UIText.Update(obj, languageID);
                    }
                    break;
                case "LanguageDescriptions":
                    {
                        LanguageDescription obj = item as LanguageDescription;
                        if (obj == null)
                            throw new ObjectException("Not a LanguageDescription");
                        returnValue = LanguageDescriptions.Update(obj);
                    }
                    break;
                case "Dictionary":
                    {
                        DictionaryEntry obj = item as DictionaryEntry;
                        if (obj == null)
                            throw new ObjectException("Not a DictionaryEntry");
                        returnValue = Dictionary.Update(obj, languageID);
                    }
                    break;
                case "DictionaryStems":
                    {
                        DictionaryEntry obj = item as DictionaryEntry;
                        if (obj == null)
                            throw new ObjectException("Not a DictionaryEntry");
                        returnValue = DictionaryStems.Update(obj, languageID);
                    }
                    break;
                case "Deinflections":
                    {
                        Deinflection obj = item as Deinflection;
                        if (obj == null)
                            throw new ObjectException("Not a Deinflection");
                        returnValue = Deinflections.Update(obj, languageID);
                    }
                    break;
                case "CourseHeaders":
                    {
                        ObjectReferenceNodeTree obj = item as ObjectReferenceNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree header");
                        obj.Modified = false;
                        returnValue = CourseHeaders.Update(obj);
                    }
                    break;
                case "Courses":
                    {
                        BaseObjectNodeTree obj = item as BaseObjectNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree");
                        obj.Modified = false;
                        returnValue = Courses.Update(obj);
                    }
                    break;
                case "StudyLists":
                    {
                        ContentStudyList obj = item as ContentStudyList;
                        if (obj == null)
                            throw new ObjectException("Not a ContentStudyList");
                        obj.Modified = false;
                        returnValue = StudyLists.Update(obj);
                    }
                    break;
                case "MediaItems":
                    {
                        ContentMediaItem obj = item as ContentMediaItem;
                        if (obj == null)
                            throw new ObjectException("Not a ContentMediaItem");
                        obj.Modified = false;
                        returnValue = MediaItems.Update(obj);
                    }
                    break;
                case "DocumentItems":
                    {
                        ContentDocumentItem obj = item as ContentDocumentItem;
                        if (obj == null)
                            throw new ObjectException("Not a ContentDocumentItem");
                        obj.Modified = false;
                        returnValue = DocumentItems.Update(obj);
                    }
                    break;
                case "Sandboxes":
                    {
                        Sandbox obj = item as Sandbox;
                        if (obj == null)
                            throw new ObjectException("Not a Sandbox");
                        obj.Modified = false;
                        returnValue = Sandboxes.Update(obj);
                    }
                    break;
                case "UserRunItems":
                    {
                        UserRunItem obj = item as UserRunItem;
                        if (obj == null)
                            throw new ObjectException("Not a UserRunItem");
                        obj.Modified = false;
                        returnValue = UserRunItems.Update(obj, languageID);
                    }
                    break;
                case "MarkupTemplates":
                    {
                        MarkupTemplate obj = item as MarkupTemplate;
                        if (obj == null)
                            throw new ObjectException("Not a MarkupTemplate");
                        obj.Modified = false;
                        returnValue = MarkupTemplates.Update(obj);
                    }
                    break;
                case "NodeMasters":
                    {
                        NodeMaster obj = item as NodeMaster;
                        if (obj == null)
                            throw new ObjectException("Not a NodeMaster");
                        obj.Modified = false;
                        returnValue = NodeMasters.Update(obj);
                    }
                    break;
                case "PlanHeaders":
                    {
                        ObjectReferenceNodeTree obj = item as ObjectReferenceNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree header");
                        obj.Modified = false;
                        returnValue = PlanHeaders.Update(obj);
                    }
                    break;
                case "Plans":
                    {
                        BaseObjectNodeTree obj = item as BaseObjectNodeTree;
                        if (obj == null)
                            throw new ObjectException("Not a BaseObjectNodeTree");
                        obj.Modified = false;
                        returnValue = Plans.Update(obj);
                    }
                    break;
                case "ToolProfiles":
                    {
                        ToolProfile obj = item as ToolProfile;
                        if (obj == null)
                            throw new ObjectException("Not a ToolProfiles");
                        obj.Modified = false;
                        returnValue = ToolProfiles.Update(obj);
                    }
                    break;
                case "ToolStudyLists":
                    {
                        ToolStudyList obj = item as ToolStudyList;
                        if (obj == null)
                            throw new ObjectException("Not a ToolStudyList");
                        obj.Modified = false;
                        returnValue = ToolStudyLists.Update(obj);
                    }
                    break;
                case "ToolSessions":
                    {
                        ToolSession obj = item as ToolSession;
                        if (obj == null)
                            throw new ObjectException("Not a ToolSession");
                        obj.Modified = false;
                        returnValue = ToolSessions.Update(obj);
                    }
                    break;
                case "LessonImages":
                    {
                        Image obj = item as Image;
                        if (obj == null)
                            throw new ObjectException("Not an Image");
                        returnValue = LessonImages.Update(obj);
                    }
                    break;
                case "ProfileImages":
                    {
                        Image obj = item as Image;
                        if (obj == null)
                            throw new ObjectException("Not an Image");
                        returnValue = ProfileImages.Update(obj);
                    }
                    break;
                case "DictionaryAudio":
                    {
                        AudioReference obj = item as AudioReference;
                        if (obj == null)
                            throw new ObjectException("Not an AudioReference object");
                        returnValue = DictionaryAudio.Update(obj, languageID);
                    }
                    break;
                case "DictionaryMultiAudio":
                    {
                        AudioMultiReference obj = item as AudioMultiReference;
                        if (obj == null)
                            throw new ObjectException("Not an AudioMultiReference object");
                        returnValue = DictionaryMultiAudio.Update(obj, languageID);
                    }
                    break;
                case "DictionaryPictures":
                    {
                        PictureReference obj = item as PictureReference;
                        if (obj == null)
                            throw new ObjectException("Not an PictureReference object");
                        returnValue = DictionaryPictures.Update(obj, languageID);
                    }
                    break;
                case "UserRecords":
                    {
                        UserRecord obj = item as UserRecord;
                        if (obj == null)
                            throw new ObjectException("Not a UserRecord");
                        returnValue = UserRecords.Update(obj);
                    }
                    break;
                case "AnonymousUserRecords":
                    {
                        AnonymousUserRecord obj = item as AnonymousUserRecord;
                        if (obj == null)
                            throw new ObjectException("Not a UserRecord");
                        returnValue = AnonymousUserRecords.Update(obj);
                    }
                    break;
                case "ChangeLogItems":
                    {
                        ChangeLogItem obj = item as ChangeLogItem;
                        if (obj == null)
                            throw new ObjectException("Not a ChangeLogItem");
                        returnValue = ChangeLogItems.Update(obj);
                    }
                    break;
                case "ForumCategories":
                    {
                        ForumCategory obj = item as ForumCategory;
                        if (obj == null)
                            throw new ObjectException("Not a ForumCategory");
                        returnValue = ForumCategories.Update(obj);
                    }
                    break;
                case "ForumHeadings":
                    {
                        ForumHeading obj = item as ForumHeading;
                        if (obj == null)
                            throw new ObjectException("Not a ForumHeading");
                        returnValue = ForumHeadings.Update(obj);
                    }
                    break;
                case "ForumTopics":
                    {
                        ForumTopic obj = item as ForumTopic;
                        if (obj == null)
                            throw new ObjectException("Not a ForumTopic");
                        returnValue = ForumTopics.Update(obj);
                    }
                    break;
                case "ForumPostings":
                    {
                        ForumPosting obj = item as ForumPosting;
                        if (obj == null)
                            throw new ObjectException("Not a ForumPosting");
                        returnValue = ForumPostings.Update(obj);
                    }
                    break;
                case "CourseTreeCache":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = CourseTreeCache.Update(obj, languageID);
                    }
                    break;
                case "PlanTreeCache":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = PlanTreeCache.Update(obj, languageID);
                    }
                    break;
                case "TranslationCache":
                    {
                        BaseString obj = item as BaseString;
                        if (obj == null)
                            throw new ObjectException("Not a BaseString");
                        returnValue = TranslationCache.Update(obj, languageID, null);
                    }
                    break;
                case "TranslationWithAlternatesCache":
                    {
                        BaseStrings obj = item as BaseStrings;
                        if (obj == null)
                            throw new ObjectException("Not a BaseStrings");
                        returnValue = TranslationWithAlternatesCache.Update(obj, languageID, null);
                    }
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        // Get database file path from repository name and languages.
        public static string GetDatabaseArgumentSignature(string source)
        {
            string signature = String.Empty;

            if (RepositorySignatures.TryGetValue(source, out signature))
                return signature;

            return signature;
        }

        // Returns true if successfull.
        public virtual bool Recreate(
            string source,
            LanguageID targetLanguageID,
            LanguageID hostLanguageID,
            List<IBaseObjectKeyed> items)
        {
            string signature = GetDatabaseArgumentSignature(source);
            bool returnValue = true;

            if (String.IsNullOrEmpty(signature))
                return false;

            if (items == null)
                items = new List<IBaseObjectKeyed>();

            switch (source)
            {
                case "UIStrings":
                    {
                        if (UIStrings.RecreateStoreCheck(targetLanguageID))
                            returnValue = UIStrings.AddList(items.Cast<BaseString>().ToList(), targetLanguageID);
                    }
                    break;
                case "UIText":
                    {
                        if (UIText.RecreateStoreCheck(targetLanguageID))
                            returnValue = UIText.AddList(items.Cast<BaseString>().ToList(), targetLanguageID);
                    }
                    break;
                case "LanguageDescriptions":
                    {
                        if (LanguageDescriptions.RecreateStoreCheck())
                            returnValue = LanguageDescriptions.AddList(items.Cast<LanguageDescription>().ToList());
                    }
                    break;
                case "Dictionary":
                    {
                        if (Dictionary.RecreateStoreCheck(targetLanguageID))
                            returnValue = Dictionary.AddList(items.Cast<DictionaryEntry>().ToList(), targetLanguageID);
                    }
                    break;
                case "DictionaryStems":
                    {
                        if (DictionaryStems.RecreateStoreCheck(targetLanguageID))
                            returnValue = DictionaryStems.AddList(items.Cast<DictionaryEntry>().ToList(), targetLanguageID);
                    }
                    break;
                case "Deinflections":
                    {
                        if (Deinflections.RecreateStoreCheck(targetLanguageID))
                            returnValue = Deinflections.AddList(items.Cast<Deinflection>().ToList(), targetLanguageID);
                    }
                    break;
                case "CourseHeaders":
                    {
                        if (CourseHeaders.RecreateStoreCheck())
                            returnValue = CourseHeaders.AddList(items.Cast<ObjectReferenceNodeTree>().ToList());
                    }
                    break;
                case "Courses":
                    {
                        if (Courses.RecreateStoreCheck())
                            returnValue = Courses.AddList(items.Cast<BaseObjectNodeTree>().ToList());
                    }
                    break;
                case "StudyLists":
                    {
                        if (StudyLists.RecreateStoreCheck())
                            returnValue = StudyLists.AddList(items.Cast<ContentStudyList>().ToList());
                    }
                    break;
                case "MediaItems":
                    {
                        if (MediaItems.RecreateStoreCheck())
                            returnValue = MediaItems.AddList(items.Cast<ContentMediaItem>().ToList());
                    }
                    break;
                case "DocumentItems":
                    {
                        if (DocumentItems.RecreateStoreCheck())
                            returnValue = DocumentItems.AddList(items.Cast<ContentDocumentItem>().ToList());
                    }
                    break;
                case "Sandboxes":
                    {
                        if (Sandboxes.RecreateStoreCheck())
                            returnValue = Sandboxes.AddList(items.Cast<Sandbox>().ToList());
                    }
                    break;
                case "UserRunItems":
                    {
                        if (UserRunItems.RecreateStoreCheck(targetLanguageID))
                            returnValue = UserRunItems.AddList(items.Cast<UserRunItem>().ToList(), targetLanguageID);
                    }
                    break;
                case "MarkupTemplates":
                    {
                        if (MarkupTemplates.RecreateStoreCheck())
                            returnValue = MarkupTemplates.AddList(items.Cast<MarkupTemplate>().ToList());
                    }
                    break;
                case "NodeMasters":
                    {
                        if (NodeMasters.RecreateStoreCheck())
                            returnValue = NodeMasters.AddList(items.Cast<NodeMaster>().ToList());
                    }
                    break;
                case "PlanHeaders":
                    {
                        if (PlanHeaders.RecreateStoreCheck())
                            returnValue = PlanHeaders.AddList(items.Cast<ObjectReferenceNodeTree>().ToList());
                    }
                    break;
                case "Plans":
                    {
                        if (Plans.RecreateStoreCheck())
                            returnValue = Plans.AddList(items.Cast<BaseObjectNodeTree>().ToList());
                    }
                    break;
                case "ToolProfiles":
                    {
                        if (ToolProfiles.RecreateStoreCheck())
                            returnValue = ToolProfiles.AddList(items.Cast<ToolProfile>().ToList());
                    }
                    break;
                case "ToolStudyLists":
                    {
                        if (ToolStudyLists.RecreateStoreCheck())
                            returnValue = ToolStudyLists.AddList(items.Cast<ToolStudyList>().ToList());
                    }
                    break;
                case "ToolSessions":
                    {
                        if (ToolSessions.RecreateStoreCheck())
                            returnValue = ToolSessions.AddList(items.Cast<ToolSession>().ToList());
                    }
                    break;
                case "LessonImages":
                    {
                        if (LessonImages.RecreateStoreCheck())
                            returnValue = LessonImages.AddList(items.Cast<Image>().ToList());
                    }
                    break;
                case "ProfileImages":
                    {
                        if (ProfileImages.RecreateStoreCheck())
                            returnValue = ProfileImages.AddList(items.Cast<Image>().ToList());
                    }
                    break;
                case "DictionaryAudio":
                    {
                        if (DictionaryAudio.RecreateStoreCheck(targetLanguageID))
                            returnValue = DictionaryAudio.AddList(items.Cast<AudioReference>().ToList(), targetLanguageID);
                    }
                    break;
                case "DictionaryMultiAudio":
                    {
                        if (DictionaryMultiAudio.RecreateStoreCheck(targetLanguageID))
                            returnValue = DictionaryMultiAudio.AddList(items.Cast<AudioMultiReference>().ToList(), targetLanguageID);
                    }
                    break;
                case "DictionaryPictures":
                    {
                        if (DictionaryPictures.RecreateStoreCheck(targetLanguageID))
                            returnValue = DictionaryPictures.AddList(items.Cast<PictureReference>().ToList(), targetLanguageID);
                    }
                    break;
                case "UserRecords":
                    {
                        if (UserRecords.RecreateStoreCheck())
                            returnValue = UserRecords.AddList(items.Cast<UserRecord>().ToList());
                    }
                    break;
                case "AnonymousUserRecords":
                    {
                        if (AnonymousUserRecords.RecreateStoreCheck())
                            returnValue = AnonymousUserRecords.AddList(items.Cast<AnonymousUserRecord>().ToList());
                    }
                    break;
                case "ChangeLogItems":
                    {
                        if (ChangeLogItems.RecreateStoreCheck())
                            returnValue = ChangeLogItems.AddList(items.Cast<ChangeLogItem>().ToList());
                    }
                    break;
                case "ForumCategories":
                    {
                        if (ForumCategories.RecreateStoreCheck())
                            returnValue = ForumCategories.AddList(items.Cast<ForumCategory>().ToList());
                    }
                    break;
                case "ForumHeadings":
                    {
                        if (ForumHeadings.RecreateStoreCheck())
                            returnValue = ForumHeadings.AddList(items.Cast<ForumHeading>().ToList());
                    }
                    break;
                case "ForumTopics":
                    {
                        if (ForumTopics.RecreateStoreCheck())
                            returnValue = ForumTopics.AddList(items.Cast<ForumTopic>().ToList());
                    }
                    break;
                case "ForumPostings":
                    {
                        if (ForumPostings.RecreateStoreCheck())
                            returnValue = ForumPostings.AddList(items.Cast<ForumPosting>().ToList());
                    }
                    break;
                case "CourseTreeCache":
                    {
                        if (CourseTreeCache.RecreateStoreCheck(targetLanguageID))
                            returnValue = CourseTreeCache.AddList(items.Cast<BaseString>().ToList(), targetLanguageID);
                    }
                    break;
                case "PlanTreeCache":
                    {
                        if (PlanTreeCache.RecreateStoreCheck(targetLanguageID))
                            returnValue = PlanTreeCache.AddList(items.Cast<BaseString>().ToList(), targetLanguageID);
                    }
                    break;
                case "TranslationCache":
                    {
                        if (TranslationCache.RecreateStoreCheck(targetLanguageID, hostLanguageID))
                            returnValue = TranslationCache.AddList(items.Cast<BaseString>().ToList(), targetLanguageID, hostLanguageID);
                    }
                    break;
                case "TranslationWithAlternatesCache":
                    {
                        if (TranslationWithAlternatesCache.RecreateStoreCheck(targetLanguageID, hostLanguageID))
                            returnValue = TranslationWithAlternatesCache.AddList(items.Cast<BaseStrings>().ToList(), targetLanguageID, hostLanguageID);
                    }
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        // Returns true if deleted successfully.
        public virtual bool DeleteReference(string source, LanguageID languageID, object key)
        {
            bool returnValue = true;

            if (key == null)
                return false;

            switch (source)
            {
                case "UIStrings":
                    returnValue = UIStrings.DeleteKey(key, languageID);
                    break;
                case "UIText":
                    returnValue = UIText.DeleteKey(key, languageID);
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.DeleteKey(key);
                    break;
                case "Dictionary":
                    returnValue = Dictionary.DeleteKey(key, languageID);
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.DeleteKey(key, languageID);
                    break;
                case "Deinflections":
                    returnValue = Deinflections.DeleteKey(key, languageID);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.DeleteKey(key);
                    break;
                case "Courses":
                    returnValue = Courses.DeleteKey(key);
                    break;
                case "StudyLists":
                    returnValue = StudyLists.DeleteKey(key);
                    break;
                case "MediaItems":
                    returnValue = MediaItems.DeleteKey(key);
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.DeleteKey(key);
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.DeleteKey(key);
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.DeleteKey(key, languageID);
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.DeleteKey(key);
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.DeleteKey(key);
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.DeleteKey(key);
                    break;
                case "Plans":
                    returnValue = Plans.DeleteKey(key);
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.DeleteKey(key);
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.DeleteKey(key);
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.DeleteKey(key);
                    break;
                case "LessonImages":
                    returnValue = LessonImages.DeleteKey(key);
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.DeleteKey(key);
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.DeleteKey(key, languageID);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.DeleteKey(key, languageID);
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.DeleteKey(key, languageID);
                    break;
                case "UserRecords":
                    returnValue = UserRecords.DeleteKey(key);
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.DeleteKey(key);
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.DeleteKey(key);
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.DeleteKey(key);
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.DeleteKey(key);
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.DeleteKey(key);
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.DeleteKey(key);
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.DeleteKey(key, languageID);
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.DeleteKey(key, languageID);
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.DeleteKey(key, languageID, null);
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.DeleteKey(key, languageID, null);
                    break;
                default:
                    throw new ObjectException("DeleteReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        // Returns true if deleted successfully.
        public virtual bool IsGenerateKeys(string source)
        {
            bool returnValue = false;

            if (String.IsNullOrEmpty(source))
                return false;

            switch (source)
            {
                case "UIStrings":
                case "UIText":
                case "LanguageDescriptions":
                case "Dictionary":
                case "DictionaryStems":
                case "Deinflections":
                case "CourseHeaders":
                    break;
                case "Courses":
                case "StudyLists":
                case "MediaItems":
                case "DocumentItems":
                    returnValue = true;
                    break;
                case "Sandboxes":
                    break;
                case "UserRunItems":
                    break;
                case "MarkupTemplates":
                case "NodeMasters":
                    returnValue = true;
                    break;
                case "PlanHeaders":
                    break;
                case "Plans":
                    returnValue = true;
                    break;
                case "ToolProfiles":
                case "ToolStudyLists":
                case "ToolSessions":
                case "LessonImages":
                case "ProfileImages":
                case "DictionaryAudio":
                case "DictionaryMultiAudio":
                case "DictionaryPictures":
                case "UserRecords":
                case "AnonymousUserRecords":
                    break;
                case "ChangeLogItems":
                case "ForumCategories":
                case "ForumHeadings":
                case "ForumTopics":
                case "ForumPostings":
                    returnValue = true;
                    break;
                case "CourseTreeCache":
                case "PlanTreeCache":
                case "TranslationCache":
                case "TranslationWithAlternatesCache":
                    break;
                default:
                    throw new ObjectException("IsGenerateKeys: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public IObjectStore FindObjectStore(string key)
        {
            string name = key;
            IObjectStore returnValue = null;
            LanguageID languageID = null;
            LanguageID languageID2 = null;

            if (name.Contains("_"))
            {
                char[] delim = {'_'};
                string[] parts = name.Split(delim);
                name = parts[0];

                if (parts.Length > 1)
                {
                    string languageCode = parts[1];
                    int count = parts.Count();
                    for (int index = 2; index < count; index++)
                        languageCode = languageCode + "-" + parts[index];
                    languageID = LanguageLookup.GetLanguageID(languageCode);
                    if (languageID == null)
                        throw new ObjectException("FindObjectStore: Unknown languageID \"" + parts[1] + "\"");
                }
            }

            returnValue = FindObjectStore(name, languageID, languageID2);

            return returnValue;
        }

        public IObjectStore FindObjectStore(string name, LanguageID languageID, LanguageID languageID2)
        {
            IObjectStore returnValue = null;

            switch (name)
            {
                case "UIStrings":
                    returnValue = UIStrings.ObjectStore.GetObjectStore(languageID);
                    break;
                case "UIText":
                    returnValue = UIText.ObjectStore.GetObjectStore(languageID);
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.ObjectStore;
                    break;
                case "Dictionary":
                    returnValue = Dictionary.ObjectStore.GetObjectStore(languageID);
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.ObjectStore.GetObjectStore(languageID);
                    break;
                case "Deinflections":
                    returnValue = Deinflections.ObjectStore.GetObjectStore(languageID);
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.ObjectStore;
                    break;
                case "Courses":
                    returnValue = Courses.ObjectStore;
                    break;
                case "StudyLists":
                    returnValue = StudyLists.ObjectStore;
                    break;
                case "MediaItems":
                    returnValue = MediaItems.ObjectStore;
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.ObjectStore;
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.ObjectStore;
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.ObjectStore.GetObjectStore(languageID);
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.ObjectStore;
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.ObjectStore;
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.ObjectStore;
                    break;
                case "Plans":
                    returnValue = Plans.ObjectStore;
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.ObjectStore;
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.ObjectStore;
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.ObjectStore;
                    break;
                case "LessonImages":
                    returnValue = LessonImages.ObjectStore;
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.ObjectStore;
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.ObjectStore.GetObjectStore(languageID);
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.ObjectStore.GetObjectStore(languageID);
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.ObjectStore.GetObjectStore(languageID);
                    break;
                case "UserRecords":
                    returnValue = UserRecords.ObjectStore;
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.ObjectStore;
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.ObjectStore;
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.ObjectStore;
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.ObjectStore;
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.ObjectStore;
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.ObjectStore;
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.ObjectStore.GetObjectStore(languageID);
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.ObjectStore.GetObjectStore(languageID);
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.ObjectStore.GetObjectStore(languageID, languageID2);
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.ObjectStore.GetObjectStore(languageID, languageID2);
                    break;
                default:
                    throw new ObjectException("FindObjectStore: Unknown object store \"" + name + "\".");
            }

            return returnValue;
        }

        /*
        public virtual string UpdateFlashEntryListStatus(object planKey, object flashListKey, List<FlashEntry> flashEntries)
        {
            LessonUtilities lessonUtilities = new LessonUtilities();
            lessonUtilities.Repositories = this;
            string message = String.Empty;
            try
            {
                if (lessonUtilities.UpdateFlashEntryListStatus(planKey, flashListKey, flashEntries, out message))
                    message = String.Empty;
            }
            catch (Exception exception)
            {
                message = "Exception: " + exception.Message;
                if (exception.InnerException != null)
                    message += " (" + exception.InnerException.Message + ")";
            }
            return message;
        }
        */

        public virtual bool TransferTo(List<IBaseObjectKeyed> objects, bool add, string source, LanguageID languageID)
        {
            if (objects == null)
                return true;

            bool returnValue = true;

            foreach (IBaseObjectKeyed obj in objects)
            {
                if (add)
                {
                    if (!SaveReference(source, languageID, obj))
                        returnValue = false;
                }
                else
                {
                    if (!UpdateReference(source, languageID, obj))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public virtual List<IBaseObjectKeyed> TransferFrom(string source, LanguageID languageID, Matcher matcher)
        {
            List<IBaseObjectKeyed> returnValue;
 
            switch (source)
            {
                case "UIStrings":
                    returnValue = UIStrings.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "UIText":
                    returnValue = UIText.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "LanguageDescriptions":
                    returnValue = LanguageDescriptions.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Dictionary":
                    returnValue = Dictionary.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryStems":
                    returnValue = DictionaryStems.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Deinflections":
                    returnValue = Deinflections.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "CourseHeaders":
                    returnValue = CourseHeaders.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Courses":
                    returnValue = Courses.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "StudyLists":
                    returnValue = StudyLists.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "MediaItems":
                    returnValue = MediaItems.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DocumentItems":
                    returnValue = DocumentItems.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Sandboxes":
                    returnValue = Sandboxes.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "UserRunItems":
                    returnValue = UserRunItems.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "MarkupTemplates":
                    returnValue = MarkupTemplates.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "NodeMasters":
                    returnValue = NodeMasters.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "PlanHeaders":
                    returnValue = PlanHeaders.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "Plans":
                    returnValue = Plans.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ToolProfiles":
                    returnValue = ToolProfiles.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ToolStudyLists":
                    returnValue = ToolStudyLists.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ToolSessions":
                    returnValue = ToolSessions.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "LessonImages":
                    returnValue = LessonImages.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ProfileImages":
                    returnValue = ProfileImages.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryAudio":
                    returnValue = DictionaryAudio.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryMultiAudio":
                    returnValue = DictionaryMultiAudio.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "DictionaryPictures":
                    returnValue = DictionaryPictures.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "UserRecords":
                    returnValue = UserRecords.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "AnonymousUserRecords":
                    returnValue = AnonymousUserRecords.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ChangeLogItems":
                    returnValue = ChangeLogItems.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumCategories":
                    returnValue = ForumCategories.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumHeadings":
                    returnValue = ForumHeadings.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumTopics":
                    returnValue = ForumTopics.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "ForumPostings":
                    returnValue = ForumPostings.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "CourseTreeCache":
                    returnValue = CourseTreeCache.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "PlanTreeCache":
                    returnValue = PlanTreeCache.Query(matcher, languageID).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "TranslationCache":
                    returnValue = TranslationCache.Query(matcher, languageID, null).Cast<IBaseObjectKeyed>().ToList();
                    break;
                case "TranslationWithAlternatesCache":
                    returnValue = TranslationWithAlternatesCache.Query(matcher, languageID, null).Cast<IBaseObjectKeyed>().ToList();
                    break;
                default:
                    throw new ObjectException("ResolveReference: Unknown source \"" + source + "\".");
            }

            return returnValue;
        }

        public virtual BaseString TranslateUIString(string stringID, LanguageID languageID)
        {
            BaseString baseString = UIStrings.Get(stringID, languageID);
            return baseString;
        }

        public virtual List<BaseString> TranslateUIStrings(List<string> stringIDs, LanguageID languageID)
        {
            List<BaseString> baseStrings = new List<BaseString>(stringIDs.Count());

            foreach (string stringID in stringIDs)
            {
                BaseString baseString = TranslateUIString(stringID, languageID);

                if (baseString != null)
                    baseStrings.Add(baseString);
            }

            return baseStrings;
        }

        public virtual BaseString TranslateUIText(string stringID, string englishString, LanguageID languageID)
        {
            BaseString baseString = UIText.Get(stringID, languageID);
            return baseString;
        }

        public virtual void Log(string errorMessage, string action, UserRecord userRecord)
        {
            BaseString languageString = UIText.Get("Log", LanguageLookup.English);
            bool created = false;

            if (errorMessage != null)
            {
                errorMessage = errorMessage.Replace("<", "&lt;");
                errorMessage = errorMessage.Replace(">", "&gt;");
            }
            else
                errorMessage = "";

            if (languageString == null)
            {
                languageString = new BaseString("Log",
                    "<h1>\r\n"
                    + "JTLanguage Log\r\n"
                    + "</h1>\r\n");
                created = true;
            }

            languageString.Text = languageString.Text +
                "<br/>\r\n"
                + "Action: " + action + "<br/>\r\n"
                + "Time: " + DateTime.UtcNow.ToString() + "<br/>\r\n"
                + "User: " + (userRecord != null ? (userRecord.UserName + " " + userRecord.IPAddress) : "(unknown user)") + "<br/>\r\n"
                + TextUtilities.TextWithHtmlLineBreaks(errorMessage) + "<br/>\r\n";

            if (created)
                UIText.Add(languageString, LanguageLookup.English);
            else
                UIText.Update(languageString, LanguageLookup.English);
        }

        public virtual void ClearLog()
        {
            MultiLanguageStringRepository multiLanguageStringRepository =
                new MultiLanguageStringRepository(UIText, new List<LanguageID>(1) { LanguageLookup.English });
            multiLanguageStringRepository.DeleteKey("Log");
        }

        public virtual string ConvertFile(string fromFilePath, string fromMimeType, string toFilePath, string toMimeType)
        {
            return "Not supported.";
        }

        public virtual MessageBase Dispatch(MessageBase command)
        {
            MessageBase result = null;

            if (!String.IsNullOrEmpty(command.MessageTarget))
            {
                if (command.MessageTarget == "MainRepository")
                {
                    switch (command.MessageName)
                    {
                        case "TransferFrom":
                            {
                                BaseString source = command.GetArgumentIndexed(0) as BaseString;
                                LanguageID languageID = command.GetBaseArgumentIndexed(1) as LanguageID;
                                Matcher matcher = command.GetArgumentIndexed(2) as Matcher;
                                List<IBaseObjectKeyed> objects = TransferFrom(source.Text, languageID, matcher);
                                List<IBaseObject> resultArguments;
                                if (objects != null)
                                    resultArguments = objects.Cast<IBaseObject>().ToList();
                                else
                                    resultArguments = new List<IBaseObject>();
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                            }
                            break;
                        case "TransferTo":
                            {
                                IObjectStore objectStore = command.GetArgumentIndexed(0) as IObjectStore;
                                List<IBaseObjectKeyed> objects = objectStore.GetAll();
                                BaseString addString = command.GetArgumentIndexed(1) as BaseString;
                                bool add = (addString != null ? Convert.ToBoolean(addString.Text) : false);
                                BaseString source = command.GetArgumentIndexed(2) as BaseString;
                                LanguageID languageID = command.GetBaseArgumentIndexed(3) as LanguageID;
                                bool returnValue = TransferTo(objects, add, source.Text, languageID);
                                BaseString resultArgument = new BaseString(returnValue.ToString());
                                List<IBaseObject> resultArguments = new List<IBaseObject>(1) { resultArgument };
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                            }
                            break;
                        case "GetAnonymousUserRecord":
                            {
                                List<IBaseObject> resultArguments = new List<IBaseObject>();
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                            }
                            break;
                        case "TranslateUIString":
                            {
                                List<IBaseObject> resultArguments = new List<IBaseObject>();
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                                IBaseObjectKeyed value = null;
                                if ((value = TranslateUIString(command.GetBaseArgumentIndexed(0).KeyString, command.GetBaseArgumentIndexed(1) as LanguageID)) != null)
                                    resultArguments.Add(value);
                            }
                            break;
                        case "TranslateUIStrings":
                            {
                                StringMatcher matcher = command.GetArgumentIndexed(0) as StringMatcher;
                                List<string> keys = matcher.Patterns;
                                LanguageID languageID = command.GetBaseArgumentIndexed(1) as LanguageID;
                                List<BaseString> strings = TranslateUIStrings(keys, languageID);
                                List<IBaseObject> resultArguments;
                                if (strings != null)
                                    resultArguments = strings.Cast<IBaseObject>().ToList();
                                else
                                    resultArguments = new List<IBaseObject>();
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                            }
                            break;
                        case "TranslateUIText":
                            {
                                List<IBaseObject> resultArguments = new List<IBaseObject>();
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                                IBaseObjectKeyed value = null;
                                if ((value = TranslateUIText(command.GetBaseArgumentIndexed(0).KeyString, command.GetBaseArgumentIndexed(1).KeyString, command.GetBaseArgumentIndexed(2) as LanguageID)) != null)
                                    resultArguments.Add(value);
                            }
                            break;
                        /*
                        case "UpdateFlashEntryListStatus":
                            {
                                string message;
                                LessonUtilities lessonUtilities = new LessonUtilities();
                                lessonUtilities.Repositories = this;
                                ToolStudyList flashList = command.GetArgumentIndexed(0) as ToolStudyList;
                                object planKey = flashList.ParentKey;
                                object flashListKey = flashList.Key;
                                List<FlashEntry> flashEntries = flashList.Entries;
                                try
                                {
                                    if (lessonUtilities.UpdateFlashEntryListStatus(planKey, flashListKey, flashEntries, out message))
                                        message = String.Empty;
                                }
                                catch (Exception exception)
                                {
                                    message = "Exception: " + exception.Message;
                                    if (exception.InnerException != null)
                                        message += " (" + exception.InnerException.Message + ")";
                                }
                                BaseString messageString = new BaseString(message);
                                List<IBaseObject> resultArguments = new List<IBaseObject>() { messageString };
                                result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
                            }
                            break;
                        */
                        default:
                            throw new Exception("MainRepository.Dispatch: Unknown commond message name: " + command.MessageName);
                    }
                }
                else
                {
                    IObjectStore objectStore = FindObjectStore(command.MessageTarget);

                    if (objectStore != null)
                        result = objectStore.Dispatch(command);
                }
            }

            return result;
        }

        public virtual void EnableCache(bool enable)
        {
            if (_UIStrings != null)
                _UIStrings.EnableCache(enable);

            if (_UIText != null)
                _UIText.EnableCache(enable);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.EnableCache(enable);

            if (_Dictionary != null)
                _Dictionary.EnableCache(enable);

            if (_DictionaryStems != null)
                _DictionaryStems.EnableCache(enable);

            if (_Deinflections != null)
                _Deinflections.EnableCache(enable);

            if (_CourseHeaders != null)
                _CourseHeaders.EnableCache(enable);

            if (_Courses != null)
                _Courses.EnableCache(enable);

            if (_StudyLists != null)
                _StudyLists.EnableCache(enable);

            if (_MediaItems != null)
                _MediaItems.EnableCache(enable);

            if (_DocumentItems != null)
                _DocumentItems.EnableCache(enable);

            if (_Sandboxes != null)
                _Sandboxes.EnableCache(enable);

            if (_UserRunItems != null)
                _UserRunItems.EnableCache(enable);

            if (_ContentStatistics != null)
                _ContentStatistics.EnableCache(enable);

            if (_MarkupTemplates != null)
                _MarkupTemplates.EnableCache(enable);

            if (_NodeMasters != null)
                _NodeMasters.EnableCache(enable);

            if (_PlanHeaders != null)
                _PlanHeaders.EnableCache(enable);

            if (_Plans != null)
                _Plans.EnableCache(enable);

            if (_ToolProfiles != null)
                _ToolProfiles.EnableCache(enable);

            if (_ToolStudyLists != null)
                _ToolStudyLists.EnableCache(enable);

            if (_ToolSessions != null)
                _ToolSessions.EnableCache(enable);

            if (_LessonImages != null)
                _LessonImages.EnableCache(enable);

            if (_ProfileImages != null)
                _ProfileImages.EnableCache(enable);

            if (_DictionaryAudio != null)
                _DictionaryAudio.EnableCache(enable);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.EnableCache(enable);

            if (_DictionaryPictures != null)
                _DictionaryPictures.EnableCache(enable);

            if (_UserRecords != null)
                _UserRecords.EnableCache(enable);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.EnableCache(enable);

            if (_ChangeLogItems != null)
                _ChangeLogItems.EnableCache(enable);

            if (_ForumCategories != null)
                _ForumCategories.EnableCache(enable);

            if (_ForumHeadings != null)
                _ForumHeadings.EnableCache(enable);

            if (_ForumTopics != null)
                _ForumTopics.EnableCache(enable);

            if (_ForumPostings != null)
                _ForumPostings.EnableCache(enable);

            if (_CourseTreeCache != null)
                _CourseTreeCache.EnableCache(enable);

            if (_PlanTreeCache != null)
                _PlanTreeCache.EnableCache(enable);

            if (_TranslationCache != null)
                _TranslationCache.EnableCache(enable);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.EnableCache(enable);
        }

        public virtual void LoadCache()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.LoadCache();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.LoadCache();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.LoadCache();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.LoadCache();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.LoadCache();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.LoadCache();

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                _CourseHeaders.LoadCache();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.LoadCache();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.LoadCache();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.LoadCache();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.LoadCache();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.LoadCache();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.LoadCache();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.LoadCache();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.LoadCache();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.LoadCache();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.LoadCache();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.LoadCache();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.LoadCache();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.LoadCache();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.LoadCache();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.LoadCache();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.LoadCache();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.LoadCache();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.LoadCache();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.LoadCache();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.LoadCache();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.LoadCache();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.LoadCache();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.LoadCache();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.LoadCache();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.LoadCache();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.LoadCache();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.LoadCache();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.LoadCache();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.LoadCache();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.LoadCache();

            UpdateProgressCheck(index++);
        }

        public virtual void SaveCache()
        {
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                _UIStrings.SaveCache();

            UpdateProgressCheck(index++);

            if (_UIText != null)
                _UIText.SaveCache();

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.SaveCache();

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                _Dictionary.SaveCache();

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                _DictionaryStems.SaveCache();

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                _Deinflections.SaveCache();

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                _CourseHeaders.SaveCache();

            UpdateProgressCheck(index++);

            if (_Courses != null)
                _Courses.SaveCache();

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                _StudyLists.SaveCache();

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                _MediaItems.SaveCache();

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                _DocumentItems.SaveCache();

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                _Sandboxes.SaveCache();

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                _UserRunItems.SaveCache();

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                _ContentStatistics.SaveCache();

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                _MarkupTemplates.SaveCache();

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                _NodeMasters.SaveCache();

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                _PlanHeaders.SaveCache();

            UpdateProgressCheck(index++);

            if (_Plans != null)
                _Plans.SaveCache();

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                _ToolProfiles.SaveCache();

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                _ToolStudyLists.SaveCache();

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                _ToolSessions.SaveCache();

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                _LessonImages.SaveCache();

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                _ProfileImages.SaveCache();

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                _DictionaryAudio.SaveCache();

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.SaveCache();

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                _DictionaryPictures.SaveCache();

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                _UserRecords.SaveCache();

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.SaveCache();

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                _ChangeLogItems.SaveCache();

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                _ForumCategories.SaveCache();

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                _ForumHeadings.SaveCache();

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                _ForumTopics.SaveCache();

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                _ForumPostings.SaveCache();

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                _CourseTreeCache.SaveCache();

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                _PlanTreeCache.SaveCache();

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                _TranslationCache.SaveCache();

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.SaveCache();

            UpdateProgressCheck(index++);
        }

        public virtual void ClearCache()
        {
            if (_UIStrings != null)
                _UIStrings.ClearCache();

            if (_UIText != null)
                _UIText.ClearCache();

            if (_LanguageDescriptions != null)
                _LanguageDescriptions.ClearCache();

            if (_Dictionary != null)
                _Dictionary.ClearCache();

            if (_DictionaryStems != null)
                _DictionaryStems.ClearCache();

            if (_Deinflections != null)
                _Deinflections.ClearCache();

            if (_CourseHeaders != null)
                _CourseHeaders.ClearCache();

            if (_Courses != null)
                _Courses.ClearCache();

            if (_StudyLists != null)
                _StudyLists.ClearCache();

            if (_MediaItems != null)
                _MediaItems.ClearCache();

            if (_DocumentItems != null)
                _DocumentItems.ClearCache();

            if (_Sandboxes != null)
                _Sandboxes.ClearCache();

            if (_UserRunItems != null)
                _UserRunItems.ClearCache();

            if (_ContentStatistics != null)
                _ContentStatistics.ClearCache();

            if (_MarkupTemplates != null)
                _MarkupTemplates.ClearCache();

            if (_NodeMasters != null)
                _NodeMasters.ClearCache();

            if (_PlanHeaders != null)
                _PlanHeaders.ClearCache();

            if (_Plans != null)
                _Plans.ClearCache();

            if (_ToolProfiles != null)
                _ToolProfiles.ClearCache();

            if (_ToolStudyLists != null)
                _ToolStudyLists.ClearCache();

            if (_ToolSessions != null)
                _ToolSessions.ClearCache();

            if (_LessonImages != null)
                _LessonImages.ClearCache();

            if (_ProfileImages != null)
                _ProfileImages.ClearCache();

            if (_DictionaryAudio != null)
                _DictionaryAudio.ClearCache();

            if (_DictionaryMultiAudio != null)
                _DictionaryMultiAudio.ClearCache();

            if (_DictionaryPictures != null)
                _DictionaryPictures.ClearCache();

            if (_UserRecords != null)
                _UserRecords.ClearCache();

            if (_AnonymousUserRecords != null)
                _AnonymousUserRecords.ClearCache();

            if (_ChangeLogItems != null)
                _ChangeLogItems.ClearCache();

            if (_ForumCategories != null)
                _ForumCategories.ClearCache();

            if (_ForumHeadings != null)
                _ForumHeadings.ClearCache();

            if (_ForumTopics != null)
                _ForumTopics.ClearCache();

            if (_ForumPostings != null)
                _ForumPostings.ClearCache();

            if (_CourseTreeCache != null)
                _CourseTreeCache.ClearCache();

            if (_PlanTreeCache != null)
                _PlanTreeCache.ClearCache();

            if (_TranslationCache != null)
                _TranslationCache.ClearCache();

            if (_TranslationWithAlternatesCache != null)
                _TranslationWithAlternatesCache.ClearCache();
        }

        public virtual IMainRepository Mirror
        {
            get { return _Mirror; }
            set
            {
                if (_Mirror != value)
                {
                    _Mirror = value;

                    if (_Mirror != null)
                    {
                        IsMirror = true;

                        if (_UIStrings != null)
                            _UIStrings.Mirror = _Mirror.UIStrings.ObjectStore;

                        if (_UIText != null)
                            _UIText.Mirror = _Mirror.UIText.ObjectStore;

                        if (_LanguageDescriptions != null)
                            _LanguageDescriptions.Mirror = _Mirror.LanguageDescriptions.ObjectStore;

                        if (_Dictionary != null)
                            _Dictionary.Mirror = _Mirror.Dictionary.ObjectStore;

                        if (_DictionaryStems != null)
                            _DictionaryStems.Mirror = _Mirror.Dictionary.ObjectStore;

                        if (_Deinflections != null)
                            _Deinflections.Mirror = _Mirror.Deinflections.ObjectStore;

                        if (_CourseHeaders != null)
                            _CourseHeaders.Mirror = _Mirror.CourseHeaders.ObjectStore;

                        if (_Courses != null)
                            _Courses.Mirror = _Mirror.Courses.ObjectStore;

                        if (_StudyLists != null)
                            _StudyLists.Mirror = _Mirror.StudyLists.ObjectStore;

                        if (_MediaItems != null)
                            _MediaItems.Mirror = _Mirror.MediaItems.ObjectStore;

                        if (_DocumentItems != null)
                            _DocumentItems.Mirror = _Mirror.DocumentItems.ObjectStore;

                        if (_Sandboxes != null)
                            _Sandboxes.Mirror = _Mirror.Sandboxes.ObjectStore;

                        if (_UserRunItems != null)
                            _UserRunItems.Mirror = _Mirror.UserRunItems.ObjectStore;

                        if (_ContentStatistics != null)
                            _ContentStatistics.Mirror = _Mirror.ContentStatistics.ObjectStore;

                        if (_MarkupTemplates != null)
                            _MarkupTemplates.Mirror = _Mirror.MarkupTemplates.ObjectStore;

                        if (_NodeMasters != null)
                            _NodeMasters.Mirror = _Mirror.NodeMasters.ObjectStore;

                        if (_PlanHeaders != null)
                            _PlanHeaders.Mirror = _Mirror.PlanHeaders.ObjectStore;

                        if (_Plans != null)
                            _Plans.Mirror = _Mirror.Plans.ObjectStore;

                        if (_ToolProfiles != null)
                            _ToolProfiles.Mirror = _Mirror.ToolProfiles.ObjectStore;

                        if (_ToolStudyLists != null)
                            _ToolStudyLists.Mirror = _Mirror.ToolStudyLists.ObjectStore;

                        if (_ToolSessions != null)
                            _ToolSessions.Mirror = _Mirror.ToolSessions.ObjectStore;

                        if (_LessonImages != null)
                            _LessonImages.Mirror = _Mirror.LessonImages.ObjectStore;

                        if (_ProfileImages != null)
                            _ProfileImages.Mirror = _Mirror.ProfileImages.ObjectStore;

                        if (_DictionaryAudio != null)
                            _DictionaryAudio.Mirror = _Mirror.DictionaryAudio.ObjectStore;

                        if (_DictionaryMultiAudio != null)
                            _DictionaryMultiAudio.Mirror = _Mirror.DictionaryMultiAudio.ObjectStore;

                        if (_DictionaryPictures != null)
                            _DictionaryPictures.Mirror = _Mirror.DictionaryPictures.ObjectStore;

                        if (_UserRecords != null)
                            _UserRecords.Mirror = _Mirror.UserRecords.ObjectStore;

                        if (_AnonymousUserRecords != null)
                            _AnonymousUserRecords.Mirror = _Mirror.AnonymousUserRecords.ObjectStore;

                        if (_ChangeLogItems != null)
                            _ChangeLogItems.Mirror = _Mirror.ChangeLogItems.ObjectStore;

                        if (_ForumCategories != null)
                            _ForumCategories.Mirror = _Mirror.ForumCategories.ObjectStore;

                        if (_ForumHeadings != null)
                            _ForumHeadings.Mirror = _Mirror.ForumHeadings.ObjectStore;

                        if (_ForumTopics != null)
                            _ForumTopics.Mirror = _Mirror.ForumTopics.ObjectStore;

                        if (_ForumPostings != null)
                            _ForumPostings.Mirror = _Mirror.ForumPostings.ObjectStore;

                        if (_CourseTreeCache != null)
                            _CourseTreeCache.Mirror = _Mirror.CourseTreeCache.ObjectStore;

                        if (_PlanTreeCache != null)
                            _PlanTreeCache.Mirror = _Mirror.PlanTreeCache.ObjectStore;

                        if (_TranslationCache != null)
                            _TranslationCache.Mirror = _Mirror.TranslationCache.ObjectStore;

                        if (_TranslationWithAlternatesCache != null)
                            _TranslationWithAlternatesCache.Mirror = _Mirror.TranslationWithAlternatesCache.ObjectStore;
                    }
                    else
                    {
                        IsMirror = false;

                        if (_UIStrings != null)
                            _UIStrings.Mirror = null;

                        if (_UIText != null)
                            _UIText.Mirror = null;

                        if (_LanguageDescriptions != null)
                            _LanguageDescriptions.Mirror = null;

                        if (_Dictionary != null)
                            _Dictionary.Mirror = null;

                        if (_DictionaryStems != null)
                            _DictionaryStems.Mirror = null;

                        if (_Deinflections != null)
                            _Deinflections.Mirror = null;

                        if (_CourseHeaders != null)
                            _CourseHeaders.Mirror = null;

                        if (_Courses != null)
                            _Courses.Mirror = null;

                        if (_StudyLists != null)
                            _StudyLists.Mirror = null;

                        if (_MediaItems != null)
                            _MediaItems.Mirror = null;

                        if (_DocumentItems != null)
                            _DocumentItems.Mirror = null;

                        if (_Sandboxes != null)
                            _Sandboxes.Mirror = null;

                        if (_UserRunItems != null)
                            _UserRunItems.Mirror = null;

                        if (_ContentStatistics != null)
                            _ContentStatistics.Mirror = null;

                        if (_MarkupTemplates != null)
                            _MarkupTemplates.Mirror = null;

                        if (_NodeMasters != null)
                            _NodeMasters.Mirror = null;

                        if (_PlanHeaders != null)
                            _PlanHeaders.Mirror = null;

                        if (_Plans != null)
                            _Plans.Mirror = null;

                        if (_ToolProfiles != null)
                            _ToolProfiles.Mirror = null;

                        if (_ToolStudyLists != null)
                            _ToolStudyLists.Mirror = null;

                        if (_ToolSessions != null)
                            _ToolSessions.Mirror = null;

                        if (_LessonImages != null)
                            _LessonImages.Mirror = null;

                        if (_ProfileImages != null)
                            _ProfileImages.Mirror = null;

                        if (_DictionaryAudio != null)
                            _DictionaryAudio.Mirror = null;

                        if (_DictionaryMultiAudio != null)
                            _DictionaryMultiAudio.Mirror = null;

                        if (_DictionaryPictures != null)
                            _DictionaryPictures.Mirror = null;

                        if (_UserRecords != null)
                            _UserRecords.Mirror = null;

                        if (_AnonymousUserRecords != null)
                            _AnonymousUserRecords.Mirror = null;

                        if (_ChangeLogItems != null)
                            _ChangeLogItems.Mirror = null;

                        if (_ForumCategories != null)
                            _ForumCategories.Mirror = null;

                        if (_ForumHeadings != null)
                            _ForumHeadings.Mirror = null;

                        if (_ForumTopics != null)
                            _ForumTopics.Mirror = null;

                        if (_ForumPostings != null)
                            _ForumPostings.Mirror = null;

                        if (_CourseTreeCache != null)
                            _CourseTreeCache.Mirror = null;

                        if (_PlanTreeCache != null)
                            _PlanTreeCache.Mirror = null;

                        if (_TranslationCache != null)
                            _TranslationCache.Mirror = null;

                        if (_TranslationWithAlternatesCache != null)
                            _TranslationWithAlternatesCache.Mirror = null;
                    }
                }
            }
        }

        public bool IsMirror { get; set; }

        public virtual bool Synchronize()
        {
            bool returnValue = true;
            int index = 0;

            UpdateProgressCheck(index++);

            if (_UIStrings != null)
                returnValue = _UIStrings.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_UIText != null)
                returnValue = _UIText.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_LanguageDescriptions != null)
                returnValue = _LanguageDescriptions.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_Dictionary != null)
                returnValue = _Dictionary.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_DictionaryStems != null)
                returnValue = _DictionaryStems.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_Deinflections != null)
                returnValue = _Deinflections.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_CourseHeaders != null)
                returnValue = _CourseHeaders.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_Courses != null)
                returnValue = _Courses.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_StudyLists != null)
                returnValue = _StudyLists.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_MediaItems != null)
                returnValue = _MediaItems.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_DocumentItems != null)
                returnValue = _DocumentItems.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_Sandboxes != null)
                returnValue = _Sandboxes.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_UserRunItems != null)
                returnValue = _UserRunItems.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ContentStatistics != null)
                returnValue = _ContentStatistics.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_MarkupTemplates != null)
                returnValue = _MarkupTemplates.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_NodeMasters != null)
                returnValue = _NodeMasters.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_PlanHeaders != null)
                returnValue = _PlanHeaders.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_Plans != null)
                returnValue = _Plans.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ToolProfiles != null)
                returnValue = _ToolProfiles.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ToolStudyLists != null)
                returnValue = _ToolStudyLists.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ToolSessions != null)
                returnValue = _ToolSessions.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_LessonImages != null)
                returnValue = _LessonImages.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ProfileImages != null)
                returnValue = _ProfileImages.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_DictionaryAudio != null)
                returnValue = _DictionaryAudio.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_DictionaryMultiAudio != null)
                returnValue = _DictionaryMultiAudio.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_DictionaryPictures != null)
                returnValue = _DictionaryPictures.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_UserRecords != null)
                returnValue = _UserRecords.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_AnonymousUserRecords != null)
                returnValue = _AnonymousUserRecords.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ChangeLogItems != null)
                returnValue = _ChangeLogItems.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ForumCategories != null)
                returnValue = _ForumCategories.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ForumHeadings != null)
                returnValue = _ForumHeadings.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ForumTopics != null)
                returnValue = _ForumTopics.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_ForumPostings != null)
                returnValue = _ForumPostings.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_CourseTreeCache != null)
                returnValue = _CourseTreeCache.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_PlanTreeCache != null)
                returnValue = _PlanTreeCache.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_TranslationCache != null)
                returnValue = _TranslationCache.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            if (_TranslationWithAlternatesCache != null)
                returnValue = _TranslationWithAlternatesCache.Synchronize() && returnValue;

            UpdateProgressCheck(index++);

            return returnValue;
        }

        public UpdateProgressFunction UpdateProgress
        {
            get { return _UpdateProgress; }
            set { _UpdateProgress = value; }
        }

        public void UpdateProgressCheck(int index)
        {
            if (index == 0)
                RequestCancel = false;

            if (_UpdateProgress != null)
            {
                if (RequestCancel)
                    throw new ObjectException("Operation cancelled.");
                else
                    _UpdateProgress((double)_RepositoryCount, (double)index);
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(UIStrings.GetElement("UIStrings"));
            element.Add(UIText.GetElement("UIText"));
            element.Add(LanguageDescriptions.GetElement("LanguageDescriptions"));
            element.Add(Dictionary.GetElement("Dictionary"));
            element.Add(DictionaryStems.GetElement("DictionaryStems"));
            element.Add(Deinflections.GetElement("Deinflections"));
            element.Add(CourseHeaders.GetElement("CourseHeaders"));
            element.Add(Courses.GetElement("Courses"));
            element.Add(StudyLists.GetElement("StudyLists"));
            element.Add(MediaItems.GetElement("MediaItems"));
            element.Add(DocumentItems.GetElement("DocumentItems"));
            element.Add(Sandboxes.GetElement("Sandboxes"));
            element.Add(UserRunItems.GetElement("UserRunItems"));
            element.Add(MarkupTemplates.GetElement("MarkupTemplates"));
            element.Add(NodeMasters.GetElement("NodeMasters"));
            element.Add(PlanHeaders.GetElement("PlanHeaders"));
            element.Add(Plans.GetElement("Plans"));
            element.Add(ToolProfiles.GetElement("ToolProfiles"));
            element.Add(ToolStudyLists.GetElement("ToolStudyLists"));
            element.Add(ToolSessions.GetElement("ToolSessions"));
            element.Add(LessonImages.GetElement("LessonImages"));
            element.Add(ProfileImages.GetElement("ProfileImages"));
            element.Add(DictionaryAudio.GetElement("DictionaryAudio"));
            element.Add(DictionaryMultiAudio.GetElement("DictionaryMultiAudio"));
            element.Add(DictionaryPictures.GetElement("DictionaryPictures"));
            element.Add(UserRecords.GetElement("UserRecords"));
            element.Add(AnonymousUserRecords.GetElement("AnonymousUserRecords"));
            element.Add(ChangeLogItems.GetElement("ChangeLogItems"));
            element.Add(ForumCategories.GetElement("ForumCategories"));
            element.Add(ForumHeadings.GetElement("ForumHeadings"));
            element.Add(ForumTopics.GetElement("ForumTopics"));
            element.Add(ForumPostings.GetElement("ForumPostings"));
            element.Add(CourseTreeCache.GetElement("CourseTreeCache"));
            element.Add(PlanTreeCache.GetElement("PlanTreeCache"));
            element.Add(TranslationCache.GetElement("TranslationCache"));
            element.Add(TranslationWithAlternatesCache.GetElement("TranslationWithAlternatesCache"));
            return element;
        }

        public override void OnElement(XElement element)
        {
            foreach (XAttribute attribute in element.Attributes())
            {
                if (!OnAttribute(attribute))
                    throw new ObjectException("MainRepository.OnElement: Unknown attribute " + attribute.Name + " in " + element.Name.ToString() + ".");
            }

            foreach (var childNode in element.Elements())
            {
                XElement childElement = childNode as XElement;

                if (childElement == null)
                    continue;

                if (!OnChildElement(childElement))
                    throw new ObjectException("MainRepository.OnElement: Unknown child element " + childElement.Name + " in " + element.Name.ToString() + ".");
            }
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Version":
                    break;
                default:
                    return false;
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "UIStrings":
                    UIStrings.OnElement(childElement);
                    break;
                case "UIText":
                    UIText.OnElement(childElement);
                    break;
                case "LanguageDescriptions":
                    LanguageDescriptions.OnElement(childElement);
                    break;
                case "Dictionary":
                    Dictionary.OnElement(childElement);
                    break;
                case "DictionaryStems":
                    DictionaryStems.OnElement(childElement);
                    break;
                case "Deinflections":
                    Deinflections.OnElement(childElement);
                    break;
                case "CourseHeaders":
                    CourseHeaders.OnElement(childElement);
                    break;
                case "Courses":
                    Courses.OnElement(childElement);
                    break;
                case "StudyLists":
                    StudyLists.OnElement(childElement);
                    break;
                case "MediaItems":
                    MediaItems.OnElement(childElement);
                    break;
                case "DocumentItems":
                    DocumentItems.OnElement(childElement);
                    break;
                case "Sandboxes":
                    Sandboxes.OnElement(childElement);
                    break;
                case "UserRunItems":
                    UserRunItems.OnElement(childElement);
                    break;
                case "MarkupTemplates":
                    MarkupTemplates.OnElement(childElement);
                    break;
                case "NodeMasters":
                    NodeMasters.OnElement(childElement);
                    break;
                case "PlanHeaders":
                    PlanHeaders.OnElement(childElement);
                    break;
                case "Plans":
                    Plans.OnElement(childElement);
                    break;
                case "ToolProfiles":
                    ToolProfiles.OnElement(childElement);
                    break;
                case "ToolStudyLists":
                    ToolStudyLists.OnElement(childElement);
                    break;
                case "ToolSessions":
                    ToolSessions.OnElement(childElement);
                    break;
                case "LessonImages":
                    LessonImages.OnElement(childElement);
                    break;
                case "ProfileImages":
                    ProfileImages.OnElement(childElement);
                    break;
                case "DictionaryAudio":
                    DictionaryAudio.OnElement(childElement);
                    break;
                case "DictionaryMultiAudio":
                    DictionaryMultiAudio.OnElement(childElement);
                    break;
                case "DictionaryPictures":
                    DictionaryPictures.OnElement(childElement);
                    break;
                case "UserRecords":
                    UserRecords.OnElement(childElement);
                    break;
                case "AnonymousUserRecords":
                    AnonymousUserRecords.OnElement(childElement);
                    break;
                case "ChangeLogItems":
                    ChangeLogItems.OnElement(childElement);
                    break;
                case "ForumCategories":
                    ForumCategories.OnElement(childElement);
                    break;
                case "ForumHeadings":
                    ForumHeadings.OnElement(childElement);
                    break;
                case "ForumTopics":
                    ForumTopics.OnElement(childElement);
                    break;
                case "ForumPostings":
                    ForumPostings.OnElement(childElement);
                    break;
                case "CourseTreeCache":
                    CourseTreeCache.OnElement(childElement);
                    break;
                case "PlanTreeCache":
                    PlanTreeCache.OnElement(childElement);
                    break;
                case "TranslationCache":
                    TranslationCache.OnElement(childElement);
                    break;
                case "TranslationWithAlternatesCache":
                    TranslationWithAlternatesCache.OnElement(childElement);
                    break;
                default:
                    throw new ObjectException("MainRepository.OnChildElement: Unknown child element \"" + childElement.Name.LocalName + "\".");
            }

            return true;
        }
    }
}
