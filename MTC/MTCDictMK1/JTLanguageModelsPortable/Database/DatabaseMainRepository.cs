using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseMainRepository : MainRepository
    {
        public static string DefaultDatabaseDirectory = "JTLanguage";
        protected string _DatabaseDirectory;

        public DatabaseMainRepository()
        {
            ClearDatabaseMainRepository();
        }

        public DatabaseMainRepository(
            string repositoryName,
            string databaseDirectory,
            string contentPath) : base(repositoryName)
        {
            ClearDatabaseMainRepository();
            _DatabaseDirectory = databaseDirectory;
            _ContentPath = contentPath;
            Initialize();
        }

        public override void Clear()
        {
            base.Clear();
            ClearDatabaseMainRepository();
        }

        public void ClearDatabaseMainRepository()
        {
            _DatabaseDirectory = null;
            CacheDirectory = "Content\\Cache\\" + KeyString + "\\";
        }

        public override void Copy(IMainRepository other)
        {
            DatabaseMainRepository otherMain = other as DatabaseMainRepository;

            if (otherMain != null)
            {
                _DatabaseDirectory = otherMain.DatabaseDirectory;
                _ContentPath = otherMain.ContentPath;
                _ServerUrl = otherMain.ServerUrl;
            }
        }

        public override void Initialize()
        {
            CacheOptions optionsCacheNone = new CacheOptions(0, false, false);
            CacheOptions optionsCacheAll = new CacheOptions(-1, false, false);
            CacheOptions optionsCacheAllPreload = new CacheOptions(-1, true, false);
            CacheOptions optionsCache64 = new CacheOptions(64, false, false);
            CacheOptions optionsCache128 = new CacheOptions(128, false, false);
            CacheOptions optionsCache256 = new CacheOptions(256, false, false);
            CacheOptions optionsCache1024 = new CacheOptions(1024, false, false);
            CacheOptions optionsCache65536 = new CacheOptions(65536, false, false);

#if true  // Set to false to turn off some caching so Admin->Fixup (DeleteOrpans) won't fail.
            CacheOptions treeOptions = optionsCache64;
            CacheOptions contentOptions = optionsCache256;
            CacheOptions templateOptions = optionsCache256;
#else
            CacheOptions treeOptions = optionsCacheNone;
            CacheOptions contentOptions = optionsCacheNone;
            CacheOptions templateOptions = optionsCacheNone;
#endif

            if (_DatabaseDirectory == null)
                _DatabaseDirectory = DefaultDatabaseDirectory;

            if (_UIStrings == null)
                _UIStrings = new LanguageBaseStringRepository(
                    new DatabaseLanguageObjectStore(
                        "UIStrings",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemBaseString>>(
                            _DatabaseDirectory, false, false),
                        optionsCacheAllPreload));

            if (_UIText == null)
                _UIText = new LanguageBaseStringRepository(
                    new DatabaseLanguageObjectStore(
                        "UIText",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemBaseString>>(
                            _DatabaseDirectory, false, false),
                        optionsCacheAll));

            if (_LanguageDescriptions == null)
                _LanguageDescriptions = new LanguageDescriptionRepository(
                    new DatabaseObjectStore(
                        "LanguageDescriptions",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemLanguageDescription>(
                            "LanguageDescriptions", _DatabaseDirectory, false, null, false),
                        optionsCacheAll));

            if (_Dictionary == null)
                _Dictionary = new DictionaryRepository(
                    new DatabaseLanguageObjectStore(
                        "Dictionary",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemDictionaryEntry>>(
                            _DatabaseDirectory, false, true),
                        optionsCacheNone));

            if (_DictionaryStems == null)
                _DictionaryStems = new DictionaryRepository(
                    new DatabaseLanguageObjectStore(
                        "DictionaryStems",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemDictionaryEntry>>(
                            _DatabaseDirectory, false, true),
                        optionsCacheNone));

            if (_Deinflections == null)
                _Deinflections = new DeinflectionRepository(
                    new DatabaseLanguageObjectStore(
                        "Deinflections",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemDeinflection>>(
                            _DatabaseDirectory, false, true),
                        optionsCacheNone));

            if (_CourseHeaders == null)
                _CourseHeaders = new NodeTreeReferenceRepository(
                    new DatabaseObjectStore(
                        "CourseHeaders",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemObjectReferenceNodeTree>(
                            "CourseHeaders", _DatabaseDirectory, false, null, false),
                        optionsCacheAll));

            if (_Courses == null)
                _Courses = new NodeTreeRepository(
                    new DatabaseObjectStore(
                        "Courses",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemBaseObjectNodeTree>(
                            "Courses", _DatabaseDirectory, true, null, false),
                        treeOptions));

            if (_StudyLists == null)
                _StudyLists = new ContentStudyListRepository(
                    new DatabaseObjectStore(
                        "StudyLists",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemContentStudyList>(
                            "StudyLists", _DatabaseDirectory, true, null, false),
                        contentOptions));

            if (_MediaItems == null)
                _MediaItems = new ContentMediaItemRepository(
                    new DatabaseObjectStore(
                        "MediaItems",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemContentMediaItem>(
                            "MediaItems", _DatabaseDirectory, true, null, false),
                        contentOptions));

            if (_DocumentItems == null)
                _DocumentItems = new ContentDocumentItemRepository(
                    new DatabaseObjectStore(
                        "DocumentItems",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemContentDocumentItem>(
                            "DocumentItems", _DatabaseDirectory, true, null, false),
                        contentOptions));

            if (_Sandboxes == null)
                _Sandboxes = new SandboxRepository(
                    new DatabaseObjectStore(
                        "Sandboxes",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemSandbox>(
                            "Sandboxes", _DatabaseDirectory, false, null, false),
                        optionsCacheAll));

            if (_ContentStatistics == null)
                _ContentStatistics = new ContentStatisticsRepository(
                    new DatabaseObjectStore(
                        "ContentStatistics",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemContentStatistics>(
                            "ContentStatistics", _DatabaseDirectory, false, null, false),
                        optionsCacheAll));

            if (_UserRunItems == null)
                _UserRunItems = new UserRunItemRepository(
                    new DatabaseLanguageObjectStore(
                        "UserRunItems",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemUserRunItem>>(
                            _DatabaseDirectory, false, true),
                        optionsCacheNone));

            if (_MarkupTemplates == null)
                _MarkupTemplates = new MarkupTemplateRepository(
                    new DatabaseObjectStore(
                        "MarkupTemplates",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemMarkupTemplate>(
                            "MarkupTemplates", _DatabaseDirectory, true, null, false),
                        templateOptions));

            if (_NodeMasters == null)
                _NodeMasters = new NodeMasterRepository(
                    new DatabaseObjectStore(
                        "NodeMasters",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemNodeMaster>(
                            "NodeMasters", _DatabaseDirectory, true, null, false),
                        templateOptions));

            if (_PlanHeaders == null)
                _PlanHeaders = new NodeTreeReferenceRepository(
                    new DatabaseObjectStore(
                        "PlanHeaders",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemObjectReferenceNodeTree>(
                            "PlanHeaders", _DatabaseDirectory, false, null, false),
                        optionsCacheAll));

            if (_Plans == null)
                _Plans = new NodeTreeRepository(
                    new DatabaseObjectStore(
                        "Plans",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemBaseObjectNodeTree>(
                            "Plans", _DatabaseDirectory, true, null, false),
                        treeOptions));

            if (_ToolProfiles == null)
                _ToolProfiles = new ToolProfileRepository(
                    new DatabaseObjectStore(
                        "ToolProfiles",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemToolProfile>(
                            "ToolProfiles", _DatabaseDirectory, false, null, false),
                        optionsCache64));

            if (_ToolStudyLists == null)
                _ToolStudyLists = new ToolStudyListRepository(
                    new DatabaseObjectStore(
                        "ToolStudyLists",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemToolStudyList>(
                            "ToolStudyLists", _DatabaseDirectory, false, null, false),
                        contentOptions));

            if (_ToolSessions == null)
                _ToolSessions = new ToolSessionRepository(
                    new DatabaseObjectStore(
                        "ToolSessions",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemToolSession>(
                            "ToolSessions", _DatabaseDirectory, false, null, false),
                        optionsCache64));

            if (_LessonImages == null)
                _LessonImages = new ImageRepository(
                    new DatabaseObjectStore(
                        "LessonImages",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemImage>(
                            "LessonImages", _DatabaseDirectory, false, null, false),
                        optionsCache64));

            if (_ProfileImages == null)
                _ProfileImages = new ImageRepository(
                    new DatabaseObjectStore(
                        "ProfileImages",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemImage>(
                            "ProfileImages", _DatabaseDirectory, false, null, false),
                        optionsCache256));

            if (_DictionaryAudio == null)
                _DictionaryAudio = new AudioRepository(
                    new DatabaseLanguageObjectStore(
                        "DictionaryAudio",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemAudioReference>>(
                            _DatabaseDirectory, false, true),
                        optionsCache256));

            if (_DictionaryMultiAudio == null)
                _DictionaryMultiAudio = new AudioMultiRepository(
                    new DatabaseLanguageObjectStore(
                        "DictionaryMultiAudio",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemAudioMultiReference>>(
                            _DatabaseDirectory, false, true),
                        optionsCache256));

            if (_DictionaryPictures == null)
                _DictionaryPictures = new PictureRepository(
                    new DatabaseLanguageObjectStore(
                        "DictionaryPictures",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemPictureReference>>(
                            _DatabaseDirectory, false, true),
                        optionsCache256));

            if (_UserRecords == null)
                _UserRecords = new UserRecordRepository(
                    new DatabaseObjectStore(
                        "UserRecords",
                        null,
                        new DatabaseTableTyped<string, DatabaseItemUserRecord>(
                            "UserRecords", _DatabaseDirectory, false, null, false),
                        optionsCache256));

            if (_AnonymousUserRecords == null)
                _AnonymousUserRecords = new AnonymousUserRecordRepository(
                    new ObjectStore("AnonymousUserRecords", null, 50));
                //_AnonymousUserRecords = new AnonymousUserRecordRepository(
                //  new DatabaseObjectStore(
                //      "AnonymousUserRecords",
                //      null,
                //      new DatabaseTableTyped<string, DatabaseItemUserRecord>("AnonymousUserRecords", _DatabaseDirectory, false, null, false),
                //      optionsCache256));

            if (_ChangeLogItems == null)
                _ChangeLogItems = new ChangeLogItemRepository(
                    new DatabaseObjectStore(
                        "ChangeLogItems",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemChangeLogItem>(
                            "ChangeLogItems", _DatabaseDirectory, true, null, false),
                        optionsCache256));

            if (_ForumCategories == null)
                _ForumCategories = new ForumCategoryRepository(
                    new DatabaseObjectStore(
                        "ForumCategories",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemForumCategory>(
                            "ForumCategories", _DatabaseDirectory, true, null, false),
                        optionsCache64));

            if (_ForumHeadings == null)
                _ForumHeadings = new ForumHeadingRepository(
                    new DatabaseObjectStore(
                        "ForumHeadings",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemForumHeading>(
                            "ForumHeadings", _DatabaseDirectory, true, null, false),
                        optionsCache256));

            if (_ForumTopics == null)
                _ForumTopics = new ForumTopicRepository(
                    new DatabaseObjectStore(
                        "ForumTopics",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemForumTopic>(
                            "ForumTopics", _DatabaseDirectory, true, null, false),
                        optionsCache256));

            if (_ForumPostings == null)
                _ForumPostings = new ForumPostingRepository(
                    new DatabaseObjectStore(
                        "ForumPostings",
                        null,
                        new DatabaseTableTyped<int, DatabaseItemForumPosting>(
                            "ForumPostings", _DatabaseDirectory, true, null, false),
                        optionsCache256));

            if (_CourseTreeCache == null)
                _CourseTreeCache = new LanguageBaseStringRepository(
                    new DatabaseLanguageObjectStore(
                        "CourseTreeCache",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemTreeCache>>(
                            _DatabaseDirectory, false, false),
                        treeOptions));

            if (_PlanTreeCache == null)
                _PlanTreeCache = new LanguageBaseStringRepository(
                    new DatabaseLanguageObjectStore(
                        "PlanTreeCache",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemTreeCache>>(
                            _DatabaseDirectory, false, false),
                        treeOptions));

            if (_TranslationCache == null)
                _TranslationCache = new LanguagePairBaseStringRepository(
                    new DatabaseLanguagePairObjectStore(
                        "TranslationCache",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemBaseString>>(
                            _DatabaseDirectory, false, false),
                        optionsCacheNone));

            if (_TranslationWithAlternatesCache == null)
                _TranslationWithAlternatesCache = new LanguagePairBaseStringsRepository(
                    new DatabaseLanguagePairObjectStore(
                        "TranslationWithAlternatesCache",
                        new DatabaseTableFactoryTyped<DatabaseTableTyped<string, DatabaseItemBaseStrings>>(
                            _DatabaseDirectory, false, false),
                        optionsCacheNone));
        }

        public virtual string DatabaseDirectory
        {
            get
            {
                return _DatabaseDirectory;
            }
            set
            {
                _DatabaseDirectory = value;
            }
        }
    }
}
