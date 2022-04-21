using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.RepositoryInterfaces
{
    public interface IObjectStore : IBaseObjectKeyed
    {
        LanguageID LanguageID { get; set; }
        LanguageID AdditionalLanguageID { get; set; }
        bool StoreExists();
        bool CreateStore();
        bool CreateStoreCheck();
        bool DeleteStore();
        bool DeleteStoreCheck();
        bool RecreateStoreCheck();
        IBaseObjectKeyed Get(object key);
        IBaseObjectKeyed GetFirst(Matcher matcher);
        IBaseObjectKeyed GetIndexed(int index);
        List<IBaseObjectKeyed> GetAll();
        List<object> GetAllKeys();
        List<object> QueryKeys(Matcher matcher);
        List<IBaseObjectKeyed> Query(Matcher matcher);
        int QueryCount(Matcher matcher);
        bool Contains(object key);
        bool Contains(Matcher matcher);
        bool Add(IBaseObjectKeyed item);
        bool AddList(List<IBaseObjectKeyed> items);
        bool CopyFrom(IObjectStore other, int startIndex = 0, int count = -1);
        bool Update(IBaseObjectKeyed item);
        bool UpdateList(List<IBaseObjectKeyed> items);
        bool Delete(IBaseObjectKeyed item);
        bool DeleteList(List<IBaseObjectKeyed> items);
        bool DeleteKeyList(List<object> keys);
        int DeleteQuery(Matcher matcher);
        bool DeleteKey(object key);
        bool DeleteAll();
        int Count();
        MessageBase Dispatch(MessageBase command);
        void InitializeCache();
        void EnableCache(bool enable);
        void LoadCache();
        void SaveCache();
        void ClearCache();
        IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item);
        void CacheCheckList(List<IBaseObjectKeyed> list);
        IObjectStore Mirror { get; set; }
        bool IsMirror { get; set; }
        bool Synchronize();
    }
}
