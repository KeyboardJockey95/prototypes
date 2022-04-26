using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.RepositoryInterfaces
{
    public interface ILanguageObjectStore : IBaseObjectKeyed
    {
        Dictionary<string, IObjectStore> Stores { get; set; }
        void ClearLanguageObjectStore();
        IObjectStore GetObjectStore(LanguageID languageID);
        void SetObjectStore(LanguageID languageID, IObjectStore objectStore);
        IObjectStore CreateObjectStore(LanguageID languageID);
        void CreateObjectStores();
        bool StoreExists();
        bool CreateStore();
        bool CreateStoreCheck();
        bool DeleteStore();
        bool DeleteStoreCheck();
        bool RecreateStoreCheck(LanguageID languageID);
        IBaseObjectKeyed Get(object key, LanguageID languageID);
        IBaseObjectKeyed GetFirst(Matcher matcher, LanguageID languageID);
        IBaseObjectKeyed GetIndexed(int index, LanguageID languageID);
        List<IBaseObjectKeyed> GetAll(LanguageID languageID);
        List<object> GetAllKeys(LanguageID languageID);
        List<IBaseObjectKeyed> Query(Matcher matcher, LanguageID languageID);
        List<IBaseObjectKeyed> Query(Matcher keyMatcher, Matcher languageIDMatcher);
        int QueryCount(Matcher matcher, LanguageID languageID);
        int QueryCount(Matcher matcher, List<LanguageID> languageIDs);
        int QueryCount(Matcher keyMatcher, Matcher languageIDMatcher);
        bool Contains(object key, LanguageID languageID);
        bool Contains(Matcher matcher, LanguageID languageID);
        bool Contains(Matcher keyMatcher, Matcher languageIDMatcher);
        bool Add(IBaseObjectKeyed item, LanguageID languageID);
        bool AddList(List<IBaseObjectKeyed> items, LanguageID languageID);
        bool CopyFrom(ILanguageObjectStore other, LanguageID languageID, int startIndex = 0, int count = -1);
        bool Update(IBaseObjectKeyed item, LanguageID languageID);
        bool UpdateList(List<IBaseObjectKeyed> items, LanguageID languageID);
        bool Delete(IBaseObjectKeyed item, LanguageID languageID);
        bool DeleteList(List<IBaseObjectKeyed> items, LanguageID languageID);
        bool DeleteKeyList(List<object> keys, LanguageID languageID);
        bool DeleteKey(object key, LanguageID languageID);
        bool DeleteAll(LanguageID languageID = null);
        int Count(LanguageID languageID = null);
        void EnableCache(bool enable);
        void LoadCache();
        void SaveCache();
        void ClearCache();
        IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item, LanguageID languageID);
        void CacheCheckList(List<IBaseObjectKeyed> list, LanguageID languageID);
        ILanguageObjectStore Mirror { get; set; }
        bool IsMirror { get; set; }
        bool Synchronize();
        void TouchLanguage(LanguageID languageID);
        void TouchAndClearModifiedLanguage(LanguageID languageID);
    }
}
