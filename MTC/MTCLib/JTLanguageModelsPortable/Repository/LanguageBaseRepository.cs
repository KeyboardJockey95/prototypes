using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Repository
{
    public class LanguageBaseRepository<T> : BaseObjectKeyed where T : IBaseObjectKeyed, new()
    {
        public ILanguageObjectStore ObjectStore { get; set; }

        public LanguageBaseRepository(ILanguageObjectStore objectStore)
            : base(objectStore.Key)
        {
            ObjectStore = objectStore;
        }

        public LanguageBaseRepository(ILanguageObjectStore objectStore, XElement element)
            : base(objectStore.Key)
        {
            ObjectStore = objectStore;
            OnElement(element);
        }

        public LanguageBaseRepository(LanguageBaseRepository<T> other)
            : base(other.Key)
        {
            ILanguageObjectStore objectStore = null;

            if (other != null)
            {
                objectStore = other.ObjectStore;

                if (objectStore != null)
                    objectStore = (ILanguageObjectStore)objectStore.Clone();
            }

            ObjectStore = objectStore;
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageBaseRepository();
        }

        public void ClearLanguageBaseRepository()
        {
            if (ObjectStore != null)
                ObjectStore.ClearLanguageObjectStore();
        }

        public override IBaseObject Clone()
        {
            return new LanguageBaseRepository<T>(this);
        }

        public override bool Modified
        {
            get
            {
                if (ObjectStore != null)
                    return ObjectStore.Modified;
                return ModifiedFlag;
            }
            set
            {
                if (ObjectStore != null)
                    ObjectStore.Modified = value;
                ModifiedFlag = value;
            }
        }

        public void TouchLanguage(LanguageID languageID)
        {
            ObjectStore.TouchLanguage(languageID);
        }

        public void TouchAndClearModifiedLanguage(LanguageID languageID)
        {
            ObjectStore.TouchAndClearModifiedLanguage(languageID);
        }

        public override DateTime CreationTime
        {
            get
            {
                if (ObjectStore != null)
                    return ObjectStore.CreationTime;
                return DateTime.MinValue;
            }
            set
            {
                if (ObjectStore != null)
                    ObjectStore.CreationTime = value;
                ModifiedFlag = true;
            }
        }

        public override DateTime ModifiedTime
        {
            get
            {
                if (ObjectStore != null)
                    return ObjectStore.ModifiedTime;
                return DateTime.MinValue;
            }
            set
            {
                if (ObjectStore != null)
                    ObjectStore.ModifiedTime = value;
                ModifiedFlag = true;
            }
        }

        public void CreateObjectStores()
        {
           ObjectStore.CreateObjectStores();
        }

        public bool StoreExists()
        {
            return ObjectStore.StoreExists();
        }

        public bool CreateStore()
        {
            return ObjectStore.CreateStore();
        }

        public bool CreateStoreCheck()
        {
            return ObjectStore.CreateStoreCheck();
        }

        public bool DeleteStore()
        {
            ObjectStore.DeleteStore();
            return true;
        }

        public bool DeleteStoreCheck()
        {
            ObjectStore.DeleteStoreCheck();
            return true;
        }

        public bool RecreateStoreCheck(LanguageID languageID)
        {
            return ObjectStore.RecreateStoreCheck(languageID);
        }

        public T Get(object key, LanguageID languageID)
        {
            return (T)ObjectStore.Get(key, languageID);
        }

        public T GetFirst(Matcher matcher, LanguageID languageID)
        {
            return (T)ObjectStore.GetFirst(matcher, languageID);
        }

        public T GetIndexed(int index, LanguageID languageID)
        {
            return (T)ObjectStore.GetIndexed(index, languageID);
        }

        public List<T> GetAll(LanguageID languageID)
        {
            List<IBaseObjectKeyed> objs = ObjectStore.GetAll(languageID);
            if (objs != null)
                return objs.AsEnumerable().Cast<T>().ToList();
            return new List<T>();
        }

        public List<object> GetAllKeys(LanguageID languageID)
        {
            return ObjectStore.GetAllKeys(languageID);
        }

        public List<T> Query(Matcher matcher, LanguageID languageID)
        {
            List<IBaseObjectKeyed> result = ObjectStore.Query(matcher, languageID);
            if (result != null)
                return result.AsEnumerable().Cast<T>().ToList();
            return new List<T>();
        }

        public List<T> Query(Matcher keyMatcher, Matcher languageIDMatcher)
        {
            List<IBaseObjectKeyed> result = ObjectStore.Query(keyMatcher, languageIDMatcher);
            if (result != null)
                return result.AsEnumerable().Cast<T>().ToList();
            return null;
        }

        public int QueryCount(Matcher matcher, LanguageID languageID)
        {
            int count = ObjectStore.QueryCount(matcher, languageID);
            return count;
        }

        public int QueryCount(Matcher matcher, List<LanguageID> languageIDs)
        {
            int count = 0;
            if (languageIDs == null)
                return count;
            foreach (LanguageID languageID in languageIDs)
            {
                int tmpCount = QueryCount(matcher, languageID);
                if (tmpCount >= 0)
                    count += tmpCount;
                else
                    return -1;
            }
            return count;
        }

        public int QueryCount(Matcher keyMatcher, Matcher languageIDMatcher)
        {
            int count = ObjectStore.QueryCount(keyMatcher, languageIDMatcher);
            return count;
        }

        public bool Contains(object key, LanguageID languageID)
        {
            return ObjectStore.Contains(key, languageID);
        }

        public bool Contains(Matcher matcher, LanguageID languageID)
        {
            return ObjectStore.Contains(matcher, languageID);
        }

        public bool Contains(Matcher keyMatcher, Matcher languageIDMatcher)
        {
            return ObjectStore.Contains(keyMatcher, languageIDMatcher);
        }

        public bool Add(T item, LanguageID languageID)
        {
            return ObjectStore.Add(item, languageID);
        }

        public bool AddList(List<T> items, LanguageID languageID)
        {
            return ObjectStore.AddList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList(), languageID);
        }

        public bool CopyFrom(LanguageBaseRepository<T> other, LanguageID languageID, int startIndex = 0, int count = -1)
        {
            return ObjectStore.CopyFrom(other.ObjectStore, languageID, startIndex, count);
        }

        public bool Update(T item, LanguageID languageID)
        {
            return ObjectStore.Update(item, languageID);
        }

        public bool UpdateList(List<T> items, LanguageID languageID)
        {
            return ObjectStore.UpdateList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList(), languageID);
        }

        public bool Delete(T item, LanguageID languageID)
        {
            return ObjectStore.Delete(item, languageID);
        }

        public bool DeleteList(List<T> items, LanguageID languageID)
        {
            return ObjectStore.DeleteList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList(), languageID);
        }

        public bool DeleteKeyList(List<object> keys, LanguageID languageID)
        {
            return ObjectStore.DeleteKeyList(keys, languageID);
        }

        public bool DeleteKey(object key, LanguageID languageID)
        {
            return ObjectStore.DeleteKey(key, languageID);
        }

        public void DeleteAll(LanguageID languageID = null)
        {
            ObjectStore.DeleteAll(languageID);
        }

        public int Count(LanguageID languageID = null)
        {
            return ObjectStore.Count(languageID);
        }

        public void EnableCache(bool enable)
        {
            ObjectStore.EnableCache(enable);
        }

        public void LoadCache()
        {
            ObjectStore.LoadCache();
        }

        public void SaveCache()
        {
            ObjectStore.SaveCache();
        }

        public void ClearCache()
        {
            ObjectStore.ClearCache();
        }

        public IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item, LanguageID languageID)
        {
            return ObjectStore.CacheCheckObject(item, languageID);
        }

        public void CacheCheckList(List<IBaseObjectKeyed> list, LanguageID languageID)
        {
            ObjectStore.CacheCheckList(list, languageID);
        }

        public ILanguageObjectStore Mirror
        {
            get { return ObjectStore.Mirror; }
            set { ObjectStore.Mirror = value; }
        }

        public bool IsMirror
        {
            get { return ObjectStore.IsMirror; }
            set { ObjectStore.IsMirror = value; }
        }

        public bool Synchronize()
        {
            return ObjectStore.Synchronize();
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("Count", ObjectStore.Stores.Count().ToString()));
            foreach (KeyValuePair<string, IObjectStore> kvp in ObjectStore.Stores)
            {
                XElement childElement = kvp.Value.Xml;
                element.Add(childElement);
            }
            return element;
        }

        public override void OnElement(XElement element)
        {
            ClearCache();
            base.OnElement(element);
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Count":
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            XAttribute languageAttribute = childElement.Attributes().FirstOrDefault(x => x.Name == "LanguageID");
            LanguageID languageID;

            if (languageAttribute == null)
                languageID = LanguageLookup.Any;
            else
                languageID = LanguageLookup.GetLanguageID(languageAttribute.Value.Trim());

            IObjectStore objectStore = ObjectStore.GetObjectStore(languageID);

            objectStore.Xml = childElement;

            return true;
        }

        public static int Compare(LanguageBaseRepository<T> other1, LanguageBaseRepository<T> other2)
        {
            if (other1 != null)
                return other1.Compare(other2);
            else if (other2 != null)
                return -other1.Compare(other1);
            else
                return 0;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            LanguageBaseRepository<T> otherRepository = other as LanguageBaseRepository<T>;
            if ((otherRepository == null) || (otherRepository.Count() == 0))
            {
                if (Count() == 0)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (Count() == 0)
                    return -1;
                else if (Count() == otherRepository.Count())
                    return ObjectStore.Compare(otherRepository.ObjectStore);
                else
                    return Count().CompareTo(otherRepository.Count());
            }
        }
    }
}
