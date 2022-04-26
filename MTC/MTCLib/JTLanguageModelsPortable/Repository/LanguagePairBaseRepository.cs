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
    public class LanguagePairBaseRepository<T> : BaseObjectKeyed where T : IBaseObjectKeyed, new()
    {
        public ILanguagePairObjectStore ObjectStore { get; set; }

        public LanguagePairBaseRepository(ILanguagePairObjectStore objectStore)
            : base(objectStore.Key)
        {
            ObjectStore = objectStore;
        }

        public LanguagePairBaseRepository(ILanguagePairObjectStore objectStore, XElement element)
            : base(objectStore.Key)
        {
            ObjectStore = objectStore;
            OnElement(element);
        }

        public LanguagePairBaseRepository(LanguagePairBaseRepository<T> other)
            : base(other.Key)
        {
            ILanguagePairObjectStore objectStore = null;

            if (other != null)
            {
                objectStore = other.ObjectStore;

                if (objectStore != null)
                    objectStore = (ILanguagePairObjectStore)objectStore.Clone();
            }

            ObjectStore = objectStore;
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguagePairBaseRepository();
        }

        public void ClearLanguagePairBaseRepository()
        {
            if (ObjectStore != null)
                ObjectStore.ClearLanguagePairObjectStore();
        }

        public override IBaseObject Clone()
        {
            return new LanguagePairBaseRepository<T>(this);
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

        public void TouchLanguage(LanguageID languageID1, LanguageID languageID2)
        {
            ObjectStore.TouchLanguage(languageID1, languageID2);
        }

        public void TouchAndClearModifiedLanguage(LanguageID languageID1, LanguageID languageID2)
        {
            ObjectStore.TouchAndClearModifiedLanguage(languageID1, languageID2);
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

        public virtual bool RecreateStoreCheck(LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = false;

            IObjectStore objectStore = ObjectStore.GetObjectStore(languageID1, languageID2);

            if (objectStore != null)
            {
                returnValue = objectStore.DeleteStoreCheck();

                if (returnValue)
                    returnValue = objectStore.CreateStore();
            }

            return returnValue;
        }

        public T Get(object key, LanguageID languageID1, LanguageID languageID2)
        {
            return (T)ObjectStore.Get(key, languageID1, languageID2);
        }

        public T GetFirst(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            return (T)ObjectStore.GetFirst(matcher, languageID1, languageID2);
        }

        public T GetIndexed(int index, LanguageID languageID1, LanguageID languageID2)
        {
            return (T)ObjectStore.GetIndexed(index, languageID1, languageID2);
        }

        public List<T> GetAll(LanguageID languageID1, LanguageID languageID2)
        {
            List<IBaseObjectKeyed> objs = ObjectStore.GetAll(languageID1, languageID2);
            if (objs != null)
                return objs.AsEnumerable().Cast<T>().ToList();
            return new List<T>();
        }

        public List<object> GetAllKeys(LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.GetAllKeys(languageID1, languageID2);
        }

        public List<T> Query(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            List<IBaseObjectKeyed> result = ObjectStore.Query(matcher, languageID1, languageID2);
            if (result != null)
                return result.AsEnumerable().Cast<T>().ToList();
            return new List<T>();
        }

        public List<T> Query(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2)
        {
            List<IBaseObjectKeyed> result = ObjectStore.Query(keyMatcher, languageIDMatcher1, languageIDMatcher2);
            if (result != null)
                return result.AsEnumerable().Cast<T>().ToList();
            return null;
        }

        public int QueryCount(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            int count = ObjectStore.QueryCount(matcher, languageID1, languageID2);
            return count;
        }

        public int QueryCount(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2)
        {
            int count = ObjectStore.QueryCount(keyMatcher, languageIDMatcher1, languageIDMatcher2);
            return count;
        }

        public bool Contains(object key, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.Contains(key, languageID1, languageID2);
        }

        public bool Contains(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.Contains(matcher, languageID1, languageID2);
        }

        public bool Contains(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2)
        {
            return ObjectStore.Contains(keyMatcher, languageIDMatcher1, languageIDMatcher2);
        }

        public bool Add(T item, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.Add(item, languageID1, languageID2);
        }

        public bool AddList(List<T> items, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.AddList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList(), languageID1, languageID2);
        }

        public bool CopyFrom(LanguagePairBaseRepository<T> other, LanguageID languageID1, LanguageID languageID2, int startIndex = 0, int count = -1)
        {
            return ObjectStore.CopyFrom(other.ObjectStore, languageID1, languageID2, startIndex, count);
        }

        public bool Update(T item, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.Update(item, languageID1, languageID2);
        }

        public bool UpdateList(List<T> items, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.UpdateList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList(), languageID1, languageID2);
        }

        public bool Delete(T item, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.Delete(item, languageID1, languageID2);
        }

        public bool DeleteList(List<T> items, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.DeleteList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList(), languageID1, languageID2);
        }

        public bool DeleteKeyList(List<object> keys, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.DeleteKeyList(keys, languageID1, languageID2);
        }

        public bool DeleteKey(object key, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.DeleteKey(key, languageID1, languageID2);
        }

        public void DeleteAll(LanguageID languageID1 = null, LanguageID languageID2 = null)
        {
            ObjectStore.DeleteAll(languageID1, languageID2);
        }

        public int Count(LanguageID languageID1 = null, LanguageID languageID2 = null)
        {
            return ObjectStore.Count(languageID1, languageID2);
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

        public IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2)
        {
            return ObjectStore.CacheCheckObject(item, languageID1, languageID2);
        }

        public void CacheCheckList(List<IBaseObjectKeyed> list, LanguageID languageID1, LanguageID languageID2)
        {
            ObjectStore.CacheCheckList(list, languageID1, languageID2);
        }

        public ILanguagePairObjectStore Mirror
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
            XAttribute languageAttribute1 = childElement.Attributes().FirstOrDefault(x => x.Name == "LanguageID");
            XAttribute languageAttribute2 = childElement.Attributes().FirstOrDefault(x => x.Name == "AdditionalLanguageID");
            LanguageID languageID1;
            LanguageID languageID2;

            if (languageAttribute1 == null)
                languageID1 = LanguageLookup.Any;
            else
                languageID1 = LanguageLookup.GetLanguageID(languageAttribute1.Value.Trim());

            if (languageAttribute2 == null)
                languageID2 = LanguageLookup.Any;
            else
                languageID2 = LanguageLookup.GetLanguageID(languageAttribute2.Value.Trim());

            IObjectStore objectStore = ObjectStore.GetObjectStore(languageID1, languageID2);

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
