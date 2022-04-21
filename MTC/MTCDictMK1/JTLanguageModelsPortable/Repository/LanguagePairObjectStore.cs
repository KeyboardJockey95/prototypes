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
    public class LanguagePairObjectStore : BaseObjectTagged, ILanguagePairObjectStore
    {
        public Dictionary<string, IObjectStore> Stores { get; set; }
        private bool ShouldEnableCache = false;
        private ILanguagePairObjectStore _Mirror;
        private bool _IsMirror;

        public LanguagePairObjectStore(string name, int preSize) : base(name)
        {
            Stores = new Dictionary<string, IObjectStore>(preSize);
            _Mirror = null;
            _IsMirror = false;
        }

        public LanguagePairObjectStore(ILanguagePairObjectStore other) : base(other)
        {
            Stores = new Dictionary<string, IObjectStore>(other.Stores);
            _Mirror = null;
            _IsMirror = false;
        }

        public LanguagePairObjectStore(XElement element)
        {
            OnElement(element);
        }

        public LanguagePairObjectStore()
        {
            Stores = new Dictionary<string, IObjectStore>();
            _Mirror = null;
            _IsMirror = false;
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguagePairObjectStore();
        }

        public void ClearLanguagePairObjectStore()
        {
            if (Stores != null)
                Stores.Clear();
            _Mirror = null;
            _IsMirror = false;
        }

        public override IBaseObject Clone()
        {
            return new LanguagePairObjectStore(this);
        }

        public virtual string ComposeObjectStoreKey(LanguageID languageID1, LanguageID languageID2)
        {
            string symbol1;
            string symbol2;

            if (languageID1 != null)
                symbol1 = languageID1.SymbolName;
            else
                symbol1 = "none";

            if (languageID2 != null)
                symbol2 = languageID2.SymbolName;
            else
                symbol2 = "none";

            string key = KeyString + "_" + symbol1 + "_" + symbol2;

            return key;
        }

        public virtual IObjectStore GetObjectStore(LanguageID languageID1, LanguageID languageID2)
        {
            IObjectStore store = null;
            string key = ComposeObjectStoreKey(languageID1, languageID2);

            if (Stores.TryGetValue(key, out store))
                return store;

            store = CreateObjectStore(languageID1, languageID2);
            SetObjectStore(languageID1, languageID2, store);
            return store;
        }

        public virtual void SetObjectStore(LanguageID languageID1, LanguageID languageID2, IObjectStore objectStore)
        {
            IObjectStore existingStore = null;
            string key = ComposeObjectStoreKey(languageID1, languageID2);

            if (Stores.TryGetValue(key, out existingStore))
                Stores.Remove(key);

            if (objectStore != null)
                Stores.Add(key, objectStore);
        }

        public virtual IObjectStore CreateObjectStore(LanguageID languageID1, LanguageID languageID2)
        {
            string key = ComposeObjectStoreKey(languageID1, languageID2);

            IObjectStore store = new ObjectStore(key, languageID1, languageID2, 0);

            if (ShouldEnableCache)
                store.EnableCache(true);

            Touch();
            return store;
        }

        public virtual void CreateObjectStores()
        {
            foreach (LanguageID languageID1 in LanguageLookup.LanguageIDs)
            {
                if ((languageID1.LanguageCode == null) && languageID1.LanguageCode.StartsWith("("))
                    continue;

                foreach (LanguageID languageID2 in LanguageLookup.LanguageIDs)
                {
                    if ((languageID2.LanguageCode == null) && languageID2.LanguageCode.StartsWith("("))
                        continue;

                    GetObjectStore(languageID1, languageID2);
                }
            }
        }

        public virtual bool StoreExists()
        {
            bool returnValue = true;

            foreach (LanguageID languageID1 in LanguageLookup.LanguageIDs)
            {
                if ((languageID1.LanguageCode == null) && languageID1.LanguageCode.StartsWith("("))
                    continue;

                foreach (LanguageID languageID2 in LanguageLookup.LanguageIDs)
                {
                    if ((languageID2.LanguageCode == null) && languageID2.LanguageCode.StartsWith("("))
                        continue;

                    IObjectStore store = null;
                    string key = ComposeObjectStoreKey(languageID1, languageID2);

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

        public virtual bool RecreateStoreCheck(LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = false;

            IObjectStore objectStore = GetObjectStore(languageID1, languageID2);

            if (objectStore != null)
            {
                returnValue = objectStore.DeleteStoreCheck();

                if (returnValue)
                    returnValue = objectStore.CreateStoreCheck();
            }

            return returnValue;
        }

        public virtual IBaseObjectKeyed Get(object key, LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).Get(key);
        }

        public virtual IBaseObjectKeyed GetFirst(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).GetFirst(matcher);
        }

        public virtual IBaseObjectKeyed GetIndexed(int index, LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).GetIndexed(index);
        }

        public virtual List<IBaseObjectKeyed> GetAll(LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).GetAll();
        }

        public virtual List<object> GetAllKeys(LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).GetAllKeys();
        }

        public virtual List<IBaseObjectKeyed> Query(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);
                List<IBaseObjectKeyed> list = new List<IBaseObjectKeyed>();
                List<IBaseObjectKeyed> result;

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        result = GetObjectStore(aLanguageID, bLanguageID).Query(matcher);

                        if (result != null)
                            list.AddRange(result);
                    }
                }

                return list;
            }
            else
                return GetObjectStore(languageID1, languageID2).Query(matcher);
        }

        public virtual List<IBaseObjectKeyed> Query(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2)
        {
            List<IBaseObjectKeyed> list = new List<IBaseObjectKeyed>();
            List<IBaseObjectKeyed> result;

            foreach (LanguageID aLanguageID in LanguageLookup.NonSpecialLanguageIDs)
            {
                if (languageIDMatcher1.Match(aLanguageID))
                {
                    foreach (LanguageID bLanguageID in LanguageLookup.NonSpecialLanguageIDs)
                    {
                        if (languageIDMatcher2.Match(bLanguageID))
                        {
                            result = GetObjectStore(aLanguageID, bLanguageID).Query(keyMatcher);

                            if (result != null)
                                list.AddRange(result);
                        }
                    }
                }
            }

            return list;
        }

        public virtual int QueryCount(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);
                int sum = 0;
                int count;

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        count = GetObjectStore(aLanguageID, bLanguageID).QueryCount(matcher);

                        if (count != -1)
                            sum += count;
                    }
                }

                return sum;
            }
            else
                return GetObjectStore(languageID1, languageID2).QueryCount(matcher);
        }

        public virtual int QueryCount(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2)
        {
            int sum = 0;
            int count;

            foreach (LanguageID aLanguageID in LanguageLookup.NonSpecialLanguageIDs)
            {
                if (languageIDMatcher1.Match(aLanguageID))
                {
                    foreach (LanguageID bLanguageID in LanguageLookup.NonSpecialLanguageIDs)
                    {
                        if (languageIDMatcher2.Match(bLanguageID))
                        {
                            count = GetObjectStore(aLanguageID, bLanguageID).QueryCount(keyMatcher);

                            if (count != -1)
                                sum += count;
                        }
                    }
                }
            }

            return sum;
        }

        public virtual bool Contains(object key, LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).Contains(key);
        }

        public virtual bool Contains(Matcher matcher, LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).Contains(matcher);
        }

        public virtual bool Contains(Matcher keyMatcher, Matcher languageIDMatcher1, Matcher languageIDMatcher2)
        {
            foreach (LanguageID aLanguageID in LanguageLookup.NonSpecialLanguageIDs)
            {
                if (languageIDMatcher1.Match(aLanguageID))
                {
                    foreach (LanguageID bLanguageID in LanguageLookup.NonSpecialLanguageIDs)
                    {
                        if (languageIDMatcher2.Match(bLanguageID))
                        {
                            if (GetObjectStore(aLanguageID, bLanguageID).Contains(keyMatcher))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual bool Add(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = GetObjectStore(languageID1, languageID2).Add(item);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool AddList(List<IBaseObjectKeyed> items, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = GetObjectStore(languageID1, languageID2).AddList(items);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool CopyFrom(ILanguagePairObjectStore other, LanguageID languageID1, LanguageID languageID2, int startIndex = 0, int count = -1)
        {
            bool returnValue = true;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        if (!GetObjectStore(aLanguageID, bLanguageID).CopyFrom(other.GetObjectStore(aLanguageID, bLanguageID), startIndex, count))
                            returnValue = false;
                    }
                }
            }
            else
                returnValue = GetObjectStore(languageID1, languageID2).CopyFrom(other.GetObjectStore(languageID1, languageID2), startIndex, count);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool Update(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = GetObjectStore(languageID1, languageID2).Update(item);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool UpdateList(List<IBaseObjectKeyed> items, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = GetObjectStore(languageID1, languageID2).UpdateList(items);
            if (returnValue)
                Touch();
            return returnValue;
        }

        public virtual bool Delete(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        if (GetObjectStore(aLanguageID, bLanguageID).Delete(item))
                            returnValue = true;
                    }
                }
            }
            else
                returnValue = GetObjectStore(languageID1, languageID2).Delete(item);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteList(List<IBaseObjectKeyed> items, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        if (GetObjectStore(aLanguageID, bLanguageID).DeleteList(items))
                            returnValue = true;
                    }
                }
            }
            else
                returnValue = GetObjectStore(languageID1, languageID2).DeleteList(items);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteKeyList(List<object> keys, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        if (GetObjectStore(aLanguageID, bLanguageID).DeleteKeyList(keys))
                            returnValue = true;
                    }
                }
            }
            else
                returnValue = GetObjectStore(languageID1, languageID2).DeleteKeyList(keys);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteKey(object key, LanguageID languageID1, LanguageID languageID2)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        if (GetObjectStore(aLanguageID, bLanguageID).DeleteKey(key))
                            returnValue = true;
                    }
                }
            }
            else
                returnValue = GetObjectStore(languageID1, languageID2).DeleteKey(key);

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual bool DeleteAll(LanguageID languageID1 = null, LanguageID languageID2 = null)
        {
            bool returnValue = false;

            Modified = true;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        if (GetObjectStore(aLanguageID, bLanguageID).DeleteAll())
                            returnValue = true;
                    }
                }
            }
            else
                returnValue = GetObjectStore(languageID1, languageID2).DeleteAll();

            if (returnValue)
                Touch();

            return returnValue;
        }

        public virtual int Count(LanguageID languageID1 = null, LanguageID languageID2 = null)
        {
            int count = 0;

            if ((languageID1 == null) || String.IsNullOrEmpty(languageID1.LanguageCode) || (languageID1.LanguageCode == "(any)") ||
                (languageID2 == null) || String.IsNullOrEmpty(languageID2.LanguageCode) || (languageID2.LanguageCode == "(any)"))
            {
                List<LanguageID> languageIDs1 = LanguageLookup.ExpandLanguageID(languageID1, null);
                List<LanguageID> languageIDs2 = LanguageLookup.ExpandLanguageID(languageID2, null);

                CreateObjectStores();

                foreach (LanguageID aLanguageID in languageIDs1)
                {
                    if (String.IsNullOrEmpty(aLanguageID.LanguageCode) || aLanguageID.LanguageCode.StartsWith("("))
                        continue;

                    foreach (LanguageID bLanguageID in languageIDs2)
                    {
                        if (String.IsNullOrEmpty(bLanguageID.LanguageCode) || bLanguageID.LanguageCode.StartsWith("("))
                            continue;

                        count += GetObjectStore(aLanguageID, bLanguageID).Count();
                    }
                }

                return count;
            }
            else
                return GetObjectStore(languageID1, languageID2).Count();
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

        public virtual IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item, LanguageID languageID1, LanguageID languageID2)
        {
            return GetObjectStore(languageID1, languageID2).CacheCheckObject(item);
        }

        public virtual void CacheCheckList(List<IBaseObjectKeyed> list, LanguageID languageID1, LanguageID languageID2)
        {
            GetObjectStore(languageID1, languageID2).CacheCheckList(list);
        }

        public virtual ILanguagePairObjectStore Mirror
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
                            kvp.Value.Mirror = value.GetObjectStore(kvp.Value.LanguageID, kvp.Value.AdditionalLanguageID);
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
            XAttribute languageAttribute1 = childElement.Attributes().FirstOrDefault(x => x.Name == "LanguageID");

            if (languageAttribute1 == null)
                throw new ObjectException("ObjectStore element missing \"LanguageID\" attribute.");

            LanguageID languageID1 = LanguageLookup.GetLanguageID(languageAttribute1.Value.Trim());

            XAttribute languageAttribute2 = childElement.Attributes().FirstOrDefault(x => x.Name == "AdditionalLanguageID");

            if (languageAttribute2 == null)
                throw new ObjectException("ObjectStore element missing \"AdditionalLanguageID\" attribute.");

            LanguageID languageID2 = LanguageLookup.GetLanguageID(languageAttribute2.Value.Trim());

            IObjectStore objectStore = GetObjectStore(languageID1, languageID2);

            objectStore.Xml = childElement;

            return true;
        }

        public static int Compare(ILanguagePairObjectStore other1, ILanguagePairObjectStore other2)
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
            ILanguagePairObjectStore otherObjectStore = other as ILanguagePairObjectStore;
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
                        LanguageID key1 = list1[index].Value.LanguageID;
                        LanguageID key2 = list1[index].Value.AdditionalLanguageID;
                        IObjectStore item1 = list1[index].Value;
                        IObjectStore item2 = otherObjectStore.GetObjectStore(key1, key2);
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

        public void TouchLanguage(LanguageID languageID1, LanguageID languageID2)
        {
            IObjectStore objectStore = GetObjectStore(languageID1, languageID2);
            objectStore.Touch();
        }

        public void TouchAndClearModifiedLanguage(LanguageID languageID1, LanguageID languageID2)
        {
            IObjectStore objectStore = GetObjectStore(languageID1, languageID2);
            objectStore.TouchAndClearModified();
        }
    }
}
