using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.RepositoryInterfaces
{
    public interface ILanguagePairObjectStore : IBaseObjectKeyed
    {
        Dictionary<string, IObjectStore> Stores { get; set; }
        void ClearLanguagePairObjectStore();
        IObjectStore GetObjectStore(LanguageID languageID1, LanguageID languageID2);
        void SetObjectStore(LanguageID languageID1, LanguageID languageID2, IObjectStore objectStore);
        IObjectStore CreateObjectStore(LanguageID languageID1, LanguageID languageID2);
        void CreateObjectStores();
        bool StoreExists();
        bool CreateStore();
        bool CreateStoreCheck();
        bool DeleteStore();
        bool DeleteStoreCheck();
        bool RecreateStoreCheck(LanguageID languageID1, LanguageID languageID2);
        IBaseObjectKeyed Get(object key, LanguageID languageID1, LanguageID languageID2);
        IBaseObjectKeyed GetFirst(Matcher matcher, LanguageID languageID1, LanguageID languageID2);
        IBaseObjectKeyed GetIndexed(int index, LanguageID languageID1, LanguageID languageID2);
        List<IBaseObjectKeyed> GetAll(LanguageID languageID1, LanguageID languageID2);
        List<object> GetAllKeys(LanguageID languageID1, LanguageID languageID2);
        List<IBaseObjectKeyed> Query(Matcher matcher, LanguageID languageID1, LanguageID languageID2);
        List<IBaseObjectKeyed> Query(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2);
        int QueryCount(Matcher matcher, LanguageID languageID1, LanguageID languageID2);
        int QueryCount(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2);
        bool Contains(object key, LanguageID languageID1, LanguageID languageID2);
        bool Contains(Matcher matcher, LanguageID languageID1, LanguageID languageID2);
        bool Contains(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2);
        bool Add(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2);
        bool AddList(List<IBaseObjectKeyed> items, LanguageID languageID1, LanguageID languageID2);
        bool CopyFrom(ILanguagePairObjectStore other, LanguageID languageID1, LanguageID languageID2, int startIndex = 0, int count = -1);
        bool Update(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2);
        bool UpdateList(List<IBaseObjectKeyed> items, LanguageID languageID1, LanguageID languageID2);
        bool Delete(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2);
        bool DeleteList(List<IBaseObjectKeyed> items, LanguageID languageID1, LanguageID languageID2);
        bool DeleteKeyList(List<object> keys, LanguageID languageID1, LanguageID languageID2);
        bool DeleteKey(object key, LanguageID languageID1, LanguageID languageID2);
        bool DeleteAll(LanguageID languageID1 = null, LanguageID languageID2 = null);
        int Count(LanguageID languageID1 = null, LanguageID languageID2 = null);
        void EnableCache(bool enable);
        void LoadCache();
        void SaveCache();
        void ClearCache();
        IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2);
        void CacheCheckList(List<IBaseObjectKeyed> list, LanguageID languageID1, LanguageID languageID2);
        ILanguagePairObjectStore Mirror { get; set; }
        bool IsMirror { get; set; }
        bool Synchronize();
        void TouchLanguage(LanguageID languageID1, LanguageID languageID2);
        void TouchAndClearModifiedLanguage(LanguageID languageID1, LanguageID languageID2);
    }
}
