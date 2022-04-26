using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
//using JTLanguageModelsPortable.Remote;

namespace JTLanguageModelsPortable.RepositoryInterfaces
{
    public delegate void UpdateProgressFunction(double count, double value);

    public interface IMainRepository : IBaseObjectKeyed
    {
        string ContentPath { get; set; }
        string ServerUrl { get; set; }
        int RepositoryCount { get; set; }
        List<string> RepositoryNames { get; }
        LanguageBaseStringRepository UIStrings { get; set; }
        LanguageBaseStringRepository UIText { get; set; }
        LanguageDescriptionRepository LanguageDescriptions { get; set; }
        DictionaryRepository Dictionary { get; set; }
        DictionaryRepository DictionaryStems { get; set; }
        DeinflectionRepository Deinflections { get; set; }
        NodeTreeReferenceRepository CourseHeaders { get; set; }
        NodeTreeRepository Courses { get; set; }
        ContentStudyListRepository StudyLists { get; set; }
        ContentMediaItemRepository MediaItems { get; set; }
        ContentDocumentItemRepository DocumentItems { get; set; }
        SandboxRepository Sandboxes { get; set; }
        UserRunItemRepository UserRunItems { get; set; }
        ContentStatisticsRepository ContentStatistics { get; set; }
        MarkupTemplateRepository MarkupTemplates { get; set; }
        NodeMasterRepository NodeMasters { get; set; }
        NodeTreeReferenceRepository PlanHeaders { get; set; }
        NodeTreeRepository Plans { get; set; }
        ToolProfileRepository ToolProfiles { get; set; }
        ToolStudyListRepository ToolStudyLists { get; set; }
        ToolSessionRepository ToolSessions { get; set; }
        ImageRepository LessonImages { get; set; }
        ImageRepository ProfileImages { get; set; }
        AudioRepository DictionaryAudio { get; set; }
        AudioMultiRepository DictionaryMultiAudio { get; set; }
        PictureRepository DictionaryPictures { get; set; }
        UserRecordRepository UserRecords { get; set; }
        AnonymousUserRecordRepository AnonymousUserRecords { get; set; }
        ChangeLogItemRepository ChangeLogItems { get; set; }
        ForumCategoryRepository ForumCategories { get; set; }
        ForumHeadingRepository ForumHeadings { get; set; }
        ForumTopicRepository ForumTopics { get; set; }
        ForumPostingRepository ForumPostings { get; set; }
        LanguageBaseStringRepository CourseTreeCache { get; set; }
        LanguageBaseStringRepository PlanTreeCache { get; set; }
        LanguagePairBaseStringRepository TranslationCache { get; set; }
        LanguagePairBaseStringsRepository TranslationWithAlternatesCache { get; set; }
        void Copy(IMainRepository other);
        void Initialize();
        void CopyFrom(IMainRepository other);
        void CreateStore();
        void CreateStoreCheck();
        void DeleteStore();
        void DeleteStoreCheck();
        bool Recreate(string source, LanguageID targetLanguageID, LanguageID hostLanguageID, List<IBaseObjectKeyed> items);
        void DeleteAll();
        bool CheckReference(string source, LanguageID languageID, object key);
        IBaseObjectKeyed ResolveGuidReference(string source, LanguageID languageID, Guid guid);
        bool SaveGuidReference(string source, LanguageID languageID, IBaseObjectKeyed item, bool update);
        IBaseObjectKeyed ResolveNamedReference(string source, LanguageID languageID, string owner, string name);
        bool SaveNamedReference(string source, LanguageID languageID, IBaseObjectKeyed item, bool update);
        IBaseObjectKeyed ResolveReference(string source, LanguageID languageID, object key);
        IBaseObjectKeyed CacheCheckReference(string source, LanguageID languageID, IBaseObjectKeyed item);
        bool SaveReference(string source, LanguageID languageID, IBaseObjectKeyed item);
        List<IBaseObjectKeyed> ResolveListReference(string source, LanguageID languageID, Matcher matcher);
        bool UpdateReference(string source, LanguageID languageID, IBaseObjectKeyed item);
        bool DeleteReference(string source, LanguageID languageID, object key);
        bool IsGenerateKeys(string source);
        bool Synchronize();
        IObjectStore FindObjectStore(string key);
        IObjectStore FindObjectStore(string name, LanguageID languageID, LanguageID languageID2);
        //string UpdateFlashEntryListStatus(object planKey, object flashListKey, List<FlashEntry> flashEntries);
        bool TransferTo(List<IBaseObjectKeyed> objects, bool add, string source, LanguageID languageID);
        List<IBaseObjectKeyed> TransferFrom(string source, LanguageID languageID, Matcher matcher);
        BaseString TranslateUIString(string englishString, LanguageID languageID);
        List<BaseString> TranslateUIStrings(List<string> englishStrings, LanguageID languageID);
        BaseString TranslateUIText(string stringID, string englishString, LanguageID languageID);
        void Log(string errorMessage, string action, UserRecord userRecord);
        void ClearLog();
        string ConvertFile(string fromFilePath, string fromMimeType, string toFilePath, string toMimeType);
        MessageBase Dispatch(MessageBase command);
        void EnableCache(bool enable);
        void LoadCache();
        void SaveCache();
        void ClearCache();
        IMainRepository Mirror { get; set; }
        bool IsMirror { get; set; }
        UpdateProgressFunction UpdateProgress { get; set; }
        void UpdateProgressCheck(int index);
        bool RequestCancel { get; set; }
    }
}
