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
    public class BaseRepository<T> : BaseObjectKeyed where T : IBaseObjectKeyed, new()
    {
        public IObjectStore ObjectStore { get; set; }

        public BaseRepository(IObjectStore objectStore)
            : base(objectStore.Key)
        {
            ObjectStore = objectStore;
        }

        public BaseRepository(BaseRepository<T> other)
            : base(other.Key)
        {
            IObjectStore objectStore = null;

            if (other != null)
            {
                objectStore = other.ObjectStore;

                if (objectStore != null)
                    objectStore = (IObjectStore)objectStore.Clone();
            }

            ObjectStore = objectStore;
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseRepository();
        }

        public void ClearBaseRepository()
        {
            if (ObjectStore != null)
                ObjectStore.DeleteAll();
        }

        public override IBaseObject Clone()
        {
            return new BaseRepository<T>(this);
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
                {
                    if (ObjectStore.CreationTime != value)
                    {
                        ObjectStore.CreationTime = value;
                        ModifiedFlag = true;
                    }
                }
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
                {
                    if (ObjectStore.CreationTime != value)
                    {
                        ObjectStore.CreationTime = value;
                        ModifiedFlag = true;
                    }
                }
            }
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

        public bool RecreateStoreCheck()
        {
            ObjectStore.RecreateStoreCheck();
            return true;
        }

        public T Get(object key)
        {
            IBaseObjectKeyed obj = ObjectStore.Get(key);

            if (obj != null)
                return (T)obj;

            return default(T);
        }

        public T GetFirst(Matcher matcher)
        {
            IBaseObjectKeyed obj = ObjectStore.GetFirst(matcher);

            if (obj != null)
                return (T)obj;

            return default(T);
        }

        public T GetIndexed(int index)
        {
            IBaseObjectKeyed obj = ObjectStore.GetIndexed(index);

            if (obj != null)
                return (T)obj;

            return default(T);
        }

        public List<T> GetAll()
        {
            List<IBaseObjectKeyed> objs = ObjectStore.GetAll();
            if (objs != null)
                return objs.AsEnumerable().Cast<T>().ToList();
            return new List<T>();
        }

        public List<object> GetAllKeys()
        {
            return ObjectStore.GetAllKeys();
        }

        public List<object> QueryKeys(Matcher matcher)
        {
            List<object> result = ObjectStore.QueryKeys(matcher);
            if (result == null)
                return new List<object>();
            return result;
        }

        public List<T> Query(Matcher matcher)
        {
            List<IBaseObjectKeyed> result = ObjectStore.Query(matcher);
            if (result != null)
                return result.AsEnumerable().Cast<T>().ToList();
            return new List<T>();
        }

        public int QueryCount(Matcher matcher)
        {
            int count = ObjectStore.QueryCount(matcher);
            return count;
        }

        public bool Contains(object key)
        {
            return ObjectStore.Contains(key);
        }

        public bool Contains(Matcher matcher)
        {
            return ObjectStore.Contains(matcher);
        }

        public bool Add(T item)
        {
            return ObjectStore.Add(item);
        }

        public bool AddList(List<T> items)
        {
            return ObjectStore.AddList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList());
        }

        public bool CopyFrom(BaseRepository<T> other, int startIndex = 0, int count = -1)
        {
            return ObjectStore.CopyFrom(other.ObjectStore, startIndex, count);
        }

        public bool Update(T item)
        {
            return ObjectStore.Update(item);
        }

        public bool UpdateList(List<T> items)
        {
            return ObjectStore.UpdateList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList());
        }

        public bool Delete(T item)
        {
            return ObjectStore.Delete(item);
        }

        public bool DeleteList(List<T> items)
        {
            return ObjectStore.DeleteList(items.AsEnumerable().Cast<IBaseObjectKeyed>().ToList());
        }

        public bool DeleteKeyList(List<object> keys)
        {
            return ObjectStore.DeleteKeyList(keys);
        }

        public int DeleteQuery(Matcher matcher)
        {
            int count = ObjectStore.DeleteQuery(matcher);
            return count;
        }

        public bool DeleteKey(object key)
        {
            return ObjectStore.DeleteKey(key);
        }

        public bool DeleteAll()
        {
            return ObjectStore.DeleteAll();
        }

        public int Count()
        {
            return ObjectStore.Count();
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

        public IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item)
        {
            return ObjectStore.CacheCheckObject(item);
        }

        public void CacheCheckList(List<IBaseObjectKeyed> list)
        {
            ObjectStore.CacheCheckList(list);
        }

        public IObjectStore Mirror
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
            element.Add(new XAttribute("Count", Count().ToString()));
            List<IBaseObjectKeyed> list = ObjectStore.GetAll();
            foreach (IBaseObjectKeyed item in list)
                element.Add(item.Xml);
            return element;
        }

        protected static List<IBaseObjectKeyed> _PendingObjects;

        public override void OnElement(XElement element)
        {
            ClearCache();
            _PendingObjects = new List<IBaseObjectKeyed>();
            base.OnElement(element);
            if ((_PendingObjects != null) && (_PendingObjects.Count() != 0))
                ObjectStore.AddList(_PendingObjects);
            _PendingObjects = null;
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
            IBaseObjectKeyed item = ObjectUtilities.ResurrectBase(childElement);

            if (item != null)
            {
                if (_PendingObjects != null)
                    _PendingObjects.Add(item);
                else
                    ObjectStore.Add(item);
            }

            return true;
        }

        public static int Compare(BaseRepository<T> other1, BaseRepository<T> other2)
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
            BaseRepository<T> otherRepository = other as BaseRepository<T>;
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
