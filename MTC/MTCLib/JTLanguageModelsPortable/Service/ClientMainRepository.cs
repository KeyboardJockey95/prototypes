using System;
using System.Collections.Generic;
using System.Linq;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Service
{
    public class ClientMainRepository : MainRepository
    {
        public ClientServiceBase Service { get; set; }
        public static UserID UserID { get; set; }
        public static bool RequiresAuthentication { get; set; }
        private static string MainRepositoryTargetName = "MainRepository";

        public ClientMainRepository(ClientServiceBase service)
        {
            Service = service;
            Initialize();
        }

        public ClientMainRepository()
        {
            Service = null;
        }

        public override void Initialize()
        {
            if (Service == null)
                throw new Exception("ClientMainRepository.Initialize:  Missing service object.");

            CacheOptions optionsCacheNone = new CacheOptions(0, false, false);
            CacheOptions optionsCacheAll = new CacheOptions(-1, false, false);
            CacheOptions optionsCacheAllPreload = new CacheOptions(-1, true, false);
            CacheOptions optionsCache64 = new CacheOptions(64, false, false);
            CacheOptions optionsCache128 = new CacheOptions(128, false, false);
            CacheOptions optionsCache256 = new CacheOptions(256, false, false);

            if (_UIStrings == null)
                _UIStrings = new LanguageBaseStringRepository(new ClientLanguageObjectStore("UIStrings", Service, optionsCacheAllPreload));

            if (_UIText == null)
                _UIText = new LanguageBaseStringRepository(new ClientLanguageObjectStore("UIText", Service, optionsCacheAll));

            if (_LanguageDescriptions == null)
                _LanguageDescriptions = new LanguageDescriptionRepository(new ClientObjectStore("LanguageDescriptions", null, Service, optionsCacheAll));

            if (_Dictionary == null)
                _Dictionary = new DictionaryRepository(new ClientLanguageObjectStore("Dictionary", Service, optionsCacheNone));

            if (_DictionaryStems == null)
                _DictionaryStems = new DictionaryRepository(new ClientLanguageObjectStore("DictionaryStems", Service, optionsCacheNone));

            if (_Deinflections == null)
                _Deinflections = new DeinflectionRepository(new ClientLanguageObjectStore("Deinflections", Service, optionsCacheNone));

            if (_CourseHeaders == null)
                _CourseHeaders = new NodeTreeReferenceRepository(new ClientObjectStore("CourseHeaders", null, Service, optionsCache64));

            if (_Courses == null)
                _Courses = new NodeTreeRepository(new ClientObjectStore("Courses", null, Service, optionsCache64));

            if (_StudyLists == null)
                _StudyLists = new ContentStudyListRepository(new ClientObjectStore("StudyLists", null, Service, optionsCache64));

            if (_MediaItems == null)
                _MediaItems = new ContentMediaItemRepository(new ClientObjectStore("MediaItems", null, Service, optionsCache64));

            if (_DocumentItems == null)
                _DocumentItems = new ContentDocumentItemRepository(new ClientObjectStore("DocumentItems", null, Service, optionsCache64));

            if (_Sandboxes == null)
                _Sandboxes = new SandboxRepository(new ClientObjectStore("Sandboxes", null, Service, optionsCache64));

            if (_MarkupTemplates == null)
                _MarkupTemplates = new MarkupTemplateRepository(new ClientObjectStore("MarkupTemplates", null, Service, optionsCache64));

            if (_NodeMasters == null)
                _NodeMasters = new NodeMasterRepository(new ClientObjectStore("NodeMasters", null, Service, optionsCache64));

            if (_PlanHeaders == null)
                _PlanHeaders = new NodeTreeReferenceRepository(new ClientObjectStore("PlanHeaders", null, Service, optionsCache64));

            if (_Plans == null)
                _Plans = new NodeTreeRepository(new ClientObjectStore("Plans", null, Service, optionsCache64));

            if (_ToolProfiles == null)
                _ToolProfiles = new ToolProfileRepository(new ClientObjectStore("ToolProfiles", null, Service, optionsCache64));

            if (_ToolStudyLists == null)
                _ToolStudyLists = new ToolStudyListRepository(new ClientObjectStore("ToolStudyLists", null, Service, optionsCache64));

            if (_ToolSessions == null)
                _ToolSessions = new ToolSessionRepository(new ClientObjectStore("ToolSessions", null, Service, optionsCache64));

            if (_LessonImages == null)
                _LessonImages = new ImageRepository(new ClientObjectStore("LessonImages", null, Service, optionsCache64));

            if (_ProfileImages == null)
                _ProfileImages = new ImageRepository(new ClientObjectStore("ProfileImages", null, Service, optionsCache64));

            if (_DictionaryAudio == null)
                _DictionaryAudio = new AudioRepository(new ClientLanguageObjectStore("DictionaryAudio", Service, optionsCache64));

            if (_DictionaryMultiAudio == null)
                _DictionaryMultiAudio = new AudioMultiRepository(new ClientLanguageObjectStore("DictionaryMultiAudio", Service, optionsCache256));

            if (_DictionaryPictures == null)
                _DictionaryPictures = new PictureRepository(new ClientLanguageObjectStore("DictionaryPictures", Service, optionsCache256));

            if (_UserRecords == null)
                _UserRecords = new UserRecordRepository(new ClientObjectStore("UserRecords", null, Service, optionsCache64));

            if (_AnonymousUserRecords == null)
                _AnonymousUserRecords = new AnonymousUserRecordRepository(new ClientObjectStore("AnonymousUserRecords", null, Service, optionsCache64));

            if (_ChangeLogItems == null)
                _ChangeLogItems = new ChangeLogItemRepository(new ClientObjectStore("ChangeLogItems", null, Service, optionsCache64));

            if (_ForumCategories == null)
                _ForumCategories = new ForumCategoryRepository(new ClientObjectStore("ForumCategories", null, Service, optionsCache64));

            if (_ForumHeadings == null)
                _ForumHeadings = new ForumHeadingRepository(new ClientObjectStore("ForumHeadings", null, Service, optionsCache64));

            if (_ForumTopics == null)
                _ForumTopics = new ForumTopicRepository(new ClientObjectStore("ForumTopics", null, Service, optionsCache128));

            if (_ForumPostings == null)
                _ForumPostings = new ForumPostingRepository(new ClientObjectStore("ForumPostings", null, Service, optionsCache256));

            if (_CourseTreeCache == null)
                _CourseTreeCache = new LanguageBaseStringRepository(new ClientLanguageObjectStore("CourseTreeCache", Service, optionsCache64));

            if (_PlanTreeCache == null)
                _PlanTreeCache = new LanguageBaseStringRepository(new ClientLanguageObjectStore("PlanTreeCache", Service, optionsCache64));

            if (_TranslationCache == null)
                _TranslationCache = new LanguagePairBaseStringRepository(new ClientLanguagePairObjectStore("TranslationCache", Service, optionsCache256));

            if (_TranslationWithAlternatesCache == null)
                _TranslationWithAlternatesCache = new LanguagePairBaseStringsRepository(new ClientLanguagePairObjectStore("TranslationWithAlternatesCache", Service, optionsCache256));
        }

        /*
        public override string UpdateFlashEntryListStatus(object planKey, object flashListKey, List<FlashEntry> flashEntries)
        {
            string returnValue;
            FlashList flashList = new FlashList();
            flashList.Key = flashListKey;
            flashList.Parent = new TitledReference(planKey, "Plans");
            flashList.Entries = flashEntries;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, MainRepositoryTargetName,
                "UpdateFlashEntryListStatus", new List<IBaseObject>() { flashList });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
            {
                BaseString resultString = result.GetBaseArgumentIndexed(0) as BaseString;
                returnValue = resultString.Text;
            }
            else
                returnValue = "(no result)";
            return returnValue;
        }
        */

        public override bool TransferTo(List<IBaseObjectKeyed> objects, bool add, string source, LanguageID languageID)
        {
            if (objects == null)
                return true;

            bool returnValue = false;
            ObjectStore objectStore = new ObjectStore(source, languageID, objects.Count());
            objectStore.AddList(objects);
            BaseString addString = new BaseString(add.ToString());

            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, MainRepositoryTargetName,
                "TransferTo", new List<IBaseObject>() { objectStore, addString, languageID });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
            {
                BaseString resultString = result.GetBaseArgumentIndexed(0) as BaseString;
                returnValue = Convert.ToBoolean(resultString);
            }

            return returnValue;
        }

        public override List<IBaseObjectKeyed> TransferFrom(string source, LanguageID languageID, Matcher matcher)
        {
            List<IBaseObjectKeyed> returnValue;
            BaseString sourceString = new BaseString(source);
            ObjectStore objectStore;

            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, MainRepositoryTargetName,
                "TransferFrom", new List<IBaseObject>() { sourceString, languageID, matcher });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
            {
                objectStore = result.GetBaseArgumentIndexed(0) as ObjectStore;
                returnValue = objectStore.GetAll();
            }
            else
                returnValue = new List<IBaseObjectKeyed>();

            return returnValue;
        }

        public override BaseString TranslateUIString(string stringID, LanguageID languageID)
        {
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, MainRepositoryTargetName,
                "TranslateUIString", new List<IBaseObject>() { new BaseObjectKeyed(stringID), languageID });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return result.GetBaseArgumentIndexed(0) as BaseString;
            return null;
        }

        public override List<BaseString> TranslateUIStrings(List<string> stringIDs, LanguageID languageID)
        {
            StringMatcher matcher = new StringMatcher(stringIDs, null, MatchCode.Exact, 0, 0);
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, MainRepositoryTargetName,
                "TranslateUIStrings", new List<IBaseObject>() { matcher, languageID });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.BaseArguments != null))
                return result.BaseArguments.Cast<BaseString>().ToList();
            return null;
        }

        public override BaseString TranslateUIText(string stringID, string englishString, LanguageID languageID)
        {
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, MainRepositoryTargetName,
                "TranslateUIText", new List<IBaseObject>() { new BaseObjectKeyed(stringID), new BaseObjectKeyed(englishString), languageID });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return result.GetBaseArgumentIndexed(0) as BaseString;
            return null;
        }
    }
}
