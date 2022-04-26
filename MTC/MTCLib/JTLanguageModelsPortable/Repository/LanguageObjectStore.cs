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
    public class LanguageObjectStore : BaseObjectTagged, ILanguageObjectStore
    {
        public Dictionary<string, IObjectStore> Stores { get; set; }
        private bool ShouldEnableCache = false;
        private ILanguageObjectStore _Mirror;
        private bool _IsMirror;

        public LanguageObjectStore(string name, int preSize) : base(name)
        {
            Stores = new Dictionary<string, IObjectStore>(preSize);
            _Mirror = null;
            _IsMirror = false;
        }

        public LanguageObjectStore(ILanguageObjectStore other) : base(other)
        {
            Stores = new Dictionary<string, IObjectStore>(other.Stores);
            _Mirror = null;
            _IsMirror = false;
        }

        public LanguageObjectStore(XElement element)
        {
            OnElement(element);
        }

        public LanguageObjectStore()
        {
            Stores = new Dictionary<string, IObjectStore>();
            _Mirror = null;
            _IsMirror = false;
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageObjectStore();
        }

        public void ClearLanguageObjectStore()
        {
            if (Stores != null)
                Stores.Clear();
            _Mirror = null;
            _IsMirror = false;
        }

        public override IBaseObject Clone()
        {
            return new LanguageObjectStore(this);
        }

        public virtual IObjectStore GetObjectStore(LanguageID languageID)
        {
            IObjectStore store = null;
            string key = null;

            if (languageID != null)
                key = languageID.LanguageCultureExtensionCode;

            if (key == null)
                key = "(none)";

            if (Stores.TryGetValue(key, out store))
                return store;

            store = CreateObjectStore(languageID);
            SetObjectStore(languageID, store);
            return store;
        }

        public virtual void SetObjectStore(LanguageID languageID, IObjectStore objectStore)
        {
            IObjectStore existingStore = null;
            string key = null;

            if (languageID != null)
                key = languageID.LanguageCultureExtensionCode;

            if (key == null)
                key = "(none)";

            if (Stores.TryGetValue(key, out existingStore))
                Stores.Remove(key);

            if (objectStore != null)
                Stores.Add(key, objectStore);
        }

        public virtual IObjectStore CreateObjectStore(LanguageID languageID)
        {
            string symbolName = null;

            if (languageID != null)
                symbolName = languageID.SymbolName;

            if (symbolName == null)
                symbolName = "none";

            IObjectStore store = new ObjectStore(Key + "_" + symbolName, languageID, 0);

            if (ShouldEnableCache)
                store.EnableCache(true);

            Touch();
            return store;
        }

        public virtual void CreateObjectStores()
        {
            foreach (LanguageID languageID in LanguageLookup.LanguageIDs)
            {
                if ((languageID.LanguageCode == null) && languageID.LanguageCode.StartsWith("("))
                    continue;

                GetObjectStore(languageID);
            }
        }

        public virtual bool StoreExists()
        {
            bool returnValue = true;

            foreach (LanguageID languageID in LanguageLookup.LanguageIDs)
            {
                if ((languageID.LanguageCode == null) && languageID.LanguageCode.StartsWith("("))
                    continue;

                IObjectStore store = null;
                string key = languageID.LanguageCultureExtensionCode;

                if (key == null)
                    key = String.Empty;

                if (Stores.TryGetValue(key, out store))
                {
                    if (!store.StoreExists())
                    {
                        returnValue = false;
                        break;
                    }
                }
                else
                {
                    returnValue = false;
                    break;
                }
            }

            return returnValue;
        }

        public virtual bool CreateStore()
        {
            bool returnValue = true;

            CreateObjectStores();

            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
            {
                if (!kvp.Value.CreateStore())
                    returnValue = false;
            }

            return returnValue;
        }

        public virtual bool CreateStoreCheck()
        {
            bool returnValue = true;

            CreateObjectStores();

            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
            {
                if (!kvp.Value.CreateStoreCheck())
                    returnValue = false;
            }

            return returnValue;
        }

        public virtual bool DeleteStore()
        {
            bool returnValue = true;

            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
            {
                if (!kvp.Value.DeleteStore())
                    returnValue = false;
            }
            Touch();

            return returnValue;
        }

        public virtual bool DeleteStoreCheck()
        {
            bool returnValue = true;

            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
            {
                if (!kvp.Value.DeleteStoreCheck())
                    returnValue = false;
            }

            return returnValue;
        }

        public virtual bool RecreateStoreCheck(LanguageID languageID)
        {
            bool returnValue = false;

            IObjectStore objectStore = GetObjectStore(languageID);

            if (objectStore != null)
            {
                returnValue = objectStore.DeleteStoreCheck();

                if (returnValue)
                    returnValue = objectStore.CreateStoreCheck();
            }

            return returnValue;
        }

        public virtual IBaseObjectKeyed Get(object key, LanguageID languageID)
        {
            return GetObjectStore(languageID).Get(key);
        }

        public virtual IBaseObjectKeyed GetFirst(Matcher matcher, LanguageID languageID)
        {
            return GetObjectStore(languageID).GetFirst(matcher);
        }

        public virtual IBaseObjectKeyed GetIndexed(int index, LanguageID languageID)
        {
            return GetObjectStore(languageID).GetIndexed(index);
        }

        public virtual List<IBaseObjectKeyed> GetAll(LanguageID languageID)
        {
            return GetObjectStore(languageID).GetAll();
        }

        public virtual List<object> GetAllKeys(LanguageID languageID)
        {
            return GetObjectStore(languageID).GetAllKeys();
        }

        public virtual List<IBaseObjectKeyed> Query(Matcher matcher, LanguageID languageID)
        {
            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                List<IBaseObjectKeyed> list = new List<IBaseObjectKeyed>();
                List<IBaseObjectKeyed> result;

                foreach (LanguageID aLanguageID in LanguageLookup.LanguageIDs)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    result = GetObjectStore(aLanguageID).Query(matcher);

                    if (result != null)
                        list.AddRange(result);
                }

                /*
                foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                {
                    languageID = kvp.Value.LanguageID;

                    if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
                        continue;

                    result = kvp.Value.Query(matcher);
                    list.AddRange(result);
                }
                */

                return list;
            }
            else
                return GetObjectStore(languageID).Query(matcher);
        }

        public virtual List<IBaseObjectKeyed> Query(Matcher keyMatcher, Matcher languageIDMatcher)
        {
            List<IBaseObjectKeyed> list = new List<IBaseObjectKeyed>();
            List<IBaseObjectKeyed> result;

            foreach (LanguageID aLanguageID in LanguageLookup.LanguageIDs)
            {
                if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                    continue;

                if (languageIDMatcher.Match(aLanguageID))
                {
                    result = GetObjectStore(aLanguageID).Query(keyMatcher);

                    if (result != null)
                        list.AddRange(result);
                }
            }

            /*
            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
            {
                if (languageIDMatcher.Match(kvp.Value.LanguageID))
                {
                    result = kvp.Value.Query(keyMatcher);
                    list.AddRange(result);
                }
            }
            */

            return list;
        }

        public virtual int QueryCount(Matcher matcher, LanguageID languageID)
        {
            int sum = 0;
            int count;
            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                foreach (LanguageID aLanguageID in LanguageLookup.LanguageIDs)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    count = GetObjectStore(aLanguageID).QueryCount(matcher);

                    if (count != -1)
                        sum += count;
                }

                return sum;
            }
            else
                return GetObjectStore(languageID).QueryCount(matcher);
        }

        public virtual int QueryCount(Matcher matcher, List<LanguageID> languageIDs)
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

        public virtual int QueryCount(Matcher keyMatcher, Matcher languageIDMatcher)
        {
            int sum = 0;
            int count;

            foreach (LanguageID aLanguageID in LanguageLookup.LanguageIDs)
            {
                if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                    continue;

                if (languageIDMatcher.Match(aLanguageID))
                {
                    count = GetObjectStore(aLanguageID).QueryCount(keyMatcher);

                    if (count != -1)
                        sum += count;
                }
            }

            return sum;
        }

        public virtual bool Contains(object key, LanguageID languageID)
        {
            return GetObjectStore(languageID).Contains(key);
        }

        public virtual bool Contains(Matcher matcher, LanguageID languageID)
        {
            return GetObjectStore(languageID).Contains(matcher);
        }

        public virtual bool Contains(Matcher keyMatcher, Matcher languageIDMatcher)
        {
            foreach (LanguageID aLanguageID in LanguageLookup.LanguageIDs)
            {
                if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                    continue;

                if (languageIDMatcher.Match(aLanguageID))
                {
                    if (GetObjectStore(aLanguageID).Contains(keyMatcher))
                        return true;
                }
            }

            /*
            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
            {
                if (languageIDMatcher.Match(kvp.Value.LanguageID))
                {
                    if (kvp.Value.Contains(keyMatcher))
                        return true;
                }
            }
            */

            return false;
        }

        public virtual bool Add(IBaseObjectKeyed item, LanguageID languageID)
        {
            bool returnValue = GetObjectStore(languageID).Add(item);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool AddList(List<IBaseObjectKeyed> items, LanguageID languageID)
        {
            bool returnValue = GetObjectStore(languageID).AddList(items);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool CopyFrom(ILanguageObjectStore other, LanguageID languageID, int startIndex = 0, int count = -1)
        {
            bool returnValue = true;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                CreateObjectStores();

                foreach (KeyValuePair<string, IObjectStore> kvp in other.Stores)
                {
                    if (!GetObjectStore(kvp.Value.LanguageID).CopyFrom(kvp.Value, startIndex, count))
                        returnValue = false;
                }
            }
            else
                returnValue =  GetObjectStore(languageID).CopyFrom(other.GetObjectStore(languageID), startIndex, count);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool Update(IBaseObjectKeyed item, LanguageID languageID)
        {
            bool returnValue = GetObjectStore(languageID).Update(item);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool UpdateList(List<IBaseObjectKeyed> items, LanguageID languageID)
        {
            bool returnValue = GetObjectStore(languageID).UpdateList(items);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool Delete(IBaseObjectKeyed item, LanguageID languageID)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                CreateObjectStores();

                foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                {
                    if (kvp.Value.Delete(item))
                        returnValue = true;
                }
            }
            else
                returnValue = GetObjectStore(languageID).Delete(item);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteList(List<IBaseObjectKeyed> items, LanguageID languageID)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                CreateObjectStores();

                foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                {
                    if (kvp.Value.DeleteList(items))
                        returnValue = true;
                }
            }
            else
                returnValue = GetObjectStore(languageID).DeleteList(items);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteKeyList(List<object> keys, LanguageID languageID)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                CreateObjectStores();

                foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                {
                    if (kvp.Value.DeleteKeyList(keys))
                        returnValue = true;
                }
            }
            else
                returnValue = GetObjectStore(languageID).DeleteKeyList(keys);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteKey(object key, LanguageID languageID)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                CreateObjectStores();

                foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                {
                    if (kvp.Value.DeleteKey(key))
                        returnValue = true;
                }
            }
            else
                returnValue = GetObjectStore(languageID).DeleteKey(key);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteAll(LanguageID languageID = null)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                CreateObjectStores();

                foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                {
                    if (kvp.Value.DeleteAll())
                        returnValue = true;
                }
            }
            else
                returnValue = GetObjectStore(languageID).DeleteAll();

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual int Count(LanguageID languageID = null)
        {
            if ((languageID == null) || String.IsNullOrEmpty(languageID.LanguageCode) || (languageID.LanguageCode == "(any)"))
            {
                int count = 0;

                CreateObjectStores();

                foreach (var kvp in Stores)
                    count += kvp.Value.Count();

                return count;
            }
            else
                return GetObjectStore(languageID).Count();
        }

        public virtual void EnableCache(bool enable)
        {
            ShouldEnableCache = enable;

            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                kvp.Value.EnableCache(enable);
        }

        public virtual void LoadCache()
        {
            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                kvp.Value.InitializeCache();
        }

        public virtual void SaveCache()
        {
            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                kvp.Value.LoadCache();
        }

        public virtual void ClearCache()
        {
            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                kvp.Value.ClearCache();
        }

        public virtual IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item, LanguageID languageID)
        {
            return GetObjectStore(languageID).CacheCheckObject(item);
        }

        public virtual void CacheCheckList(List<IBaseObjectKeyed> list, LanguageID languageID)
        {
            GetObjectStore(languageID).CacheCheckList(list);
        }

        public virtual ILanguageObjectStore Mirror
        {
            get
            {
                return _Mirror;
            }
            set
            {
                if (value != _Mirror)
                {
                    _Mirror = value;

                    if (value != null)
                    {
                        foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                            kvp.Value.Mirror = value.GetObjectStore(kvp.Value.LanguageID);
                    }
                    else
                    {
                        foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                            kvp.Value.Mirror = null;
                    }
                }
            }
        }

        public virtual bool IsMirror
        {
            get
            {
                return _IsMirror;
            }
            set
            {
                if (value != _IsMirror)
                {
                    _IsMirror = value;

                    foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                        kvp.Value.IsMirror = value;
                }
            }
        }

        public virtual bool Synchronize()
        {
            bool returnValue = true;

            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
                returnValue = kvp.Value.Synchronize() && returnValue;

            return returnValue;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            foreach (KeyValuePair<string, IObjectStore> kvp in Stores)
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

        public override bool OnChildElement(XElement childElement)
        {
            XAttribute languageAttribute = childElement.Attributes().FirstOrDefault(x => x.Name == "LanguageID");

            if (languageAttribute == null)
                throw new ObjectException("ObjectStore element missing \"LanguageID\" attribute.");

            LanguageID languageID = LanguageLookup.GetLanguageID(languageAttribute.Value.Trim());

            IObjectStore objectStore = GetObjectStore(languageID);

            objectStore.Xml = childElement;

            return true;
        }

        public static int Compare(ILanguageObjectStore other1, ILanguageObjectStore other2)
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
            ILanguageObjectStore otherObjectStore = other as ILanguageObjectStore;
            if ((otherObjectStore == null) || (otherObjectStore.Count() == 0))
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
                else if (Count() == otherObjectStore.Count())
                {
                    List<KeyValuePair<string, IObjectStore>> list1 = Stores.ToList();
                    int index;
                    int count = list1.Count();
                    for (index = 0; index < count; index++)
                    {
                        LanguageID key = list1[index].Value.LanguageID;
                        IObjectStore item1 = list1[index].Value;
                        IObjectStore item2 = otherObjectStore.GetObjectStore(key);
                        int diff = item1.Compare(item2);
                        if (diff != 0)
                            return diff;
                    }
                    return 0;
                }
                else
                    return Count().CompareTo(otherObjectStore.Count());
            }
        }

        public void TouchLanguage(LanguageID languageID)
        {
            IObjectStore objectStore = GetObjectStore(languageID);
            objectStore.Touch();
        }

        public void TouchAndClearModifiedLanguage(LanguageID languageID)
        {
            IObjectStore objectStore = GetObjectStore(languageID);
            objectStore.TouchAndClearModified();
        }
    }
}
