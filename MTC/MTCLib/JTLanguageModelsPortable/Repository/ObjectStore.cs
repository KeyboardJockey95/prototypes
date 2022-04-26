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
    public class ObjectStore : BaseObjectTagged, IObjectStore
    {
        public LanguageID LanguageID { get; set; }
        public LanguageID AdditionalLanguageID { get; set; }
        public List<IBaseObjectKeyed> Store { get; set; }
        public Dictionary<string, IBaseObjectKeyed> StringDictionary { get; set; }
        public Dictionary<int, IBaseObjectKeyed> IntDictionary { get; set; }

        public ObjectStore(string name, LanguageID languageID, int preSize) : base(name)
        {
            LanguageID = languageID;
            AdditionalLanguageID = null;
            Store = new List<IBaseObjectKeyed>(preSize);
            StringDictionary = new Dictionary<string, IBaseObjectKeyed>(preSize);
            IntDictionary = new Dictionary<int, IBaseObjectKeyed>(preSize);
        }

        public ObjectStore(string name, LanguageID languageID1, LanguageID languageID2, int preSize) : base(name)
        {
            LanguageID = languageID1;
            AdditionalLanguageID = languageID2;
            Store = new List<IBaseObjectKeyed>(preSize);
            StringDictionary = new Dictionary<string, IBaseObjectKeyed>(preSize);
            IntDictionary = new Dictionary<int, IBaseObjectKeyed>(preSize);
        }

        public ObjectStore(IObjectStore other) : base(other)
        {
            LanguageID = other.LanguageID;
            AdditionalLanguageID = other.AdditionalLanguageID;
            List<IBaseObjectKeyed> otherList = other.GetAll();
            int preSize = otherList.Count();
            Store = new List<IBaseObjectKeyed>(preSize);
            StringDictionary = new Dictionary<string, IBaseObjectKeyed>(preSize);
            IntDictionary = new Dictionary<int, IBaseObjectKeyed>(preSize);
            foreach (IBaseObjectKeyed item in otherList)
                InternalAdd(item);
        }

        public ObjectStore(XElement element)
        {
            LanguageID = null;
            AdditionalLanguageID = null;
            Store = new List<IBaseObjectKeyed>();
            StringDictionary = new Dictionary<string, IBaseObjectKeyed>();
            IntDictionary = new Dictionary<int, IBaseObjectKeyed>();
            OnElement(element);
        }

        public ObjectStore()
        {
            LanguageID = null;
            AdditionalLanguageID = null;
            Store = new List<IBaseObjectKeyed>();
            StringDictionary = new Dictionary<string, IBaseObjectKeyed>();
            IntDictionary = new Dictionary<int, IBaseObjectKeyed>();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectStore();
        }

        public void ClearObjectStore()
        {
            if (Store != null)
                Store.Clear();
            if (StringDictionary != null)
                StringDictionary.Clear();
            if (IntDictionary != null)
                IntDictionary.Clear();
        }

        public override IBaseObject Clone()
        {
            return new ObjectStore(this);
        }

        public virtual bool StoreExists()
        {
            return true;
        }

        public virtual bool CreateStore()
        {
            return true;
        }

        public virtual bool CreateStoreCheck()
        {
            return true;
        }

        public virtual bool DeleteStore()
        {
            DeleteAll();
            return true;
        }

        public virtual bool DeleteStoreCheck()
        {
            DeleteAll();
            return true;
        }

        public virtual bool RecreateStoreCheck()
        {
            DeleteAll();
            return true;
        }

        public virtual IBaseObjectKeyed Get(object key)
        {
            return InternalGet(key);
        }

        public virtual IBaseObjectKeyed GetFirst(Matcher matcher)
        {
            return Store.FirstOrDefault(x => matcher.Match(x));
        }

        public virtual IBaseObjectKeyed GetIndexed(int index)
        {
            return Store.ElementAtOrDefault(index);
        }

        public virtual List<IBaseObjectKeyed> GetAll()
        {
            return Store;
        }

        public virtual List<object> GetAllKeys()
        {
            List<object> keys = new List<object>(Store.Count());
            foreach (IBaseObjectKeyed item in Store)
                keys.Add(item.Key);
            return keys;
        }

        public List<object> QueryKeys(Matcher matcher)
        {
            int index = 0;
            IEnumerable<object> result =
                from item in Store
                where matcher.MatchWithPaging(item, index, out index)
                select item.Key;
            if (result == null)
                return new List<object>();
            List<object> list = result.ToList();
            return list;
        }

        public List<IBaseObjectKeyed> Query(Matcher matcher)
        {
            int index = 0;
            IEnumerable<IBaseObjectKeyed> result =
                from item in Store
                where matcher.MatchWithPaging(item, index, out index)
                select item;
            if (result == null)
                return new List<IBaseObjectKeyed>();
            List<IBaseObjectKeyed> list = result.ToList();
            return list;
        }

        public int QueryCount(Matcher matcher)
        {
            int count = 0;
            foreach (IBaseObjectKeyed obj in Store)
            {
                if (matcher.Match(obj))
                    count++;
            }
            return count;
        }

        public virtual bool Contains(object key)
        {
            return InternalContains(key);
        }

        public virtual bool Contains(Matcher matcher)
        {
            return (Store.FirstOrDefault(x => matcher.Match(x)) != null) ? true : false;
        }

        public virtual bool Add(IBaseObjectKeyed item)
        {
            if (InternalAdd(item))
            {
                Touch();
                Modified = true;
                return true;
            }
            return false;
        }

        public virtual bool AddList(List<IBaseObjectKeyed> items)
        {
            bool returnValue = true;
            foreach (IBaseObjectKeyed item in items)
            {
                if (!Add(item))
                    returnValue = false;
            }
            return returnValue;
        }

        public virtual bool CopyFrom(IObjectStore other, int startIndex = 0, int count = -1)
        {
            if (other == null)
                return false;
            int otherCount = other.Count();
            IBaseObjectKeyed item;
            if (count == -1)
                count = otherCount - startIndex;
            int endIndex = startIndex + count;
            if (endIndex > otherCount)
                endIndex = otherCount;
            for (; startIndex < endIndex; startIndex++)
            {
                item = other.GetIndexed(startIndex);
                if (item == null)
                    return false;
                InternalAdd(item);
            }
            return true;
        }

        public virtual bool Update(IBaseObjectKeyed item)
        {
            if (item == null)
                return false;

            IBaseObjectKeyed testItem = InternalGet(item.Key);

            if (testItem != null)
            {
                if (item != testItem)
                {
                    Delete(testItem);
                    Add(item);
                }
                Touch();
                return true;
            }
            else
                return Add(item);
        }

        public virtual bool UpdateList(List<IBaseObjectKeyed> items)
        {
            bool returnValue = true;
            foreach (IBaseObjectKeyed item in items)
            {
                if (!Update(item))
                    returnValue = false;
            }
            return returnValue;
        }

        public virtual bool Delete(IBaseObjectKeyed item)
        {
            Modified = true;
            return InternalDelete(item);
        }

        public virtual bool DeleteList(List<IBaseObjectKeyed> items)
        {
            bool returnValue = true;
            foreach (IBaseObjectKeyed item in items)
            {
                if (!Delete(item))
                    returnValue = false;
            }
            return returnValue;
        }

        public virtual bool DeleteKeyList(List<object> keys)
        {
            bool returnValue = true;
            foreach (object key in keys)
            {
                if (!DeleteKey(key))
                    returnValue = false;
            }
            return returnValue;
        }

        public int DeleteQuery(Matcher matcher)
        {
            int count = 0;
            foreach (IBaseObjectKeyed obj in Store)
            {
                if (matcher.Match(obj))
                {
                    if (InternalDelete(obj))
                        count++;
                }
            }
            if (count != 0)
                Modified = true;
            return count;
        }

        public virtual bool DeleteKey(object key)
        {
            IBaseObjectKeyed item = InternalGet(key);
            if (item == null)
                return false;
            return InternalDelete(item);
        }

        public virtual bool DeleteAll()
        {
            Store.Clear();
            StringDictionary.Clear();
            IntDictionary.Clear();
            Touch();
            Modified = true;
            return true;
        }

        public virtual int Count()
        {
            return Store.Count();
        }

        public virtual MessageBase Dispatch(MessageBase command)
        {
            List<IBaseObject> resultArguments = new List<IBaseObject>();
            MessageBase result = new MessageBase(command.MessageID, command.UserID, false, command.MessageTarget, command.MessageName, resultArguments);
            IBaseObjectKeyed value = null;

            switch (command.MessageName)
            {
                case "StoreExists":
                    resultArguments.Add(new BaseObjectKeyed(StoreExists()));
                    break;
                case "CreateStore":
                    resultArguments.Add(new BaseObjectKeyed(CreateStore()));
                    break;
                case "CreateStoreCheck":
                    resultArguments.Add(new BaseObjectKeyed(CreateStoreCheck()));
                    break;
                case "DeleteStore":
                    resultArguments.Add(new BaseObjectKeyed(DeleteStore()));
                    break;
                case "DeleteStoreCheck":
                    resultArguments.Add(new BaseObjectKeyed(DeleteStoreCheck()));
                    break;
                case "Get":
                    if ((value = Get(command.GetBaseArgumentIndexed(0).Key)) != null)
                        resultArguments.Add(value);
                    break;
                case "GetFirst":
                    if ((value = GetFirst((Matcher)command.GetArgumentIndexed(0))) != null)
                        resultArguments.Add(value);
                    break;
                case "GetIndexed":
                    if ((value = Get((int)command.GetBaseArgumentIndexed(0).Key)) != null)
                        resultArguments.Add(value);
                    break;
                case "GetAll":
                    result.BaseArguments = GetAll();
                    break;
                case "GetAllKeys":
                    {
                        List<object> keys = GetAllKeys();
                        if (keys != null)
                        {
                            foreach (object key in keys)
                                resultArguments.Add(new BaseObjectKeyed(key));
                        }
                    }
                    break;
                case "Query":
                    result.BaseArguments = Query((Matcher)command.GetArgumentIndexed(0));
                    break;
                case "QueryCount":
                    resultArguments.Add(new BaseObjectKeyed(QueryCount((Matcher)command.GetArgumentIndexed(0))));
                    break;
                case "Contains":
                    resultArguments.Add(new BaseObjectKeyed(Contains(command.GetBaseArgumentIndexed(0).Key)));
                    break;
                case "ContainsQuery;":
                    resultArguments.Add(new BaseObjectKeyed(Contains((Matcher)command.GetArgumentIndexed(0))));
                    break;
                case "Add":
                    resultArguments.Add(new BaseObjectKeyed(Add(command.GetBaseArgumentIndexed(0))));
                    break;
                case "AddList":
                    resultArguments.Add(new BaseObjectKeyed(AddList(command.BaseArguments)));
                    break;
                case "Update":
                    resultArguments.Add(new BaseObjectKeyed(Update(command.GetBaseArgumentIndexed(0))));
                    break;
                case "UpdateList":
                    resultArguments.Add(new BaseObjectKeyed(UpdateList(command.BaseArguments)));
                    break;
                case "Delete":
                    resultArguments.Add(new BaseObjectKeyed(Delete(command.GetBaseArgumentIndexed(0))));
                    break;
                case "DeleteList":
                    resultArguments.Add(new BaseObjectKeyed(DeleteList(command.BaseArguments)));
                    break;
                case "DeleteKeyList":
                    resultArguments.Add(new BaseObjectKeyed(DeleteKeyList(command.BaseArgumentKeys)));
                    break;
                case "DeleteKey":
                    resultArguments.Add(new BaseObjectKeyed(DeleteKey(command.GetBaseArgumentIndexed(0).Key)));
                    break;
                case "DeleteAll":
                    resultArguments.Add(new BaseObjectKeyed(DeleteAll()));
                    break;
                case "Count":
                    resultArguments.Add(new BaseObjectKeyed(Count()));
                    break;
                case "CreationTime":
                    resultArguments.Add(new BaseObjectKeyed(CreationTime));
                    break;
                case "ModifiedTime":
                    resultArguments.Add(new BaseObjectKeyed(ModifiedTime));
                    break;
                default:
                    throw new ObjectException("ObjectStore.Dispatch: Unknown command \"" + command.MessageName + "\"");
            }

            return result;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if ((LanguageID != null) && !String.IsNullOrEmpty(LanguageID.LanguageCultureExtensionCode))
                element.Add(new XAttribute("LanguageID", LanguageID.LanguageCultureExtensionCode));
            if ((AdditionalLanguageID != null) && !String.IsNullOrEmpty(AdditionalLanguageID.LanguageCultureExtensionCode))
                element.Add(new XAttribute("AdditionalLanguageID", AdditionalLanguageID.LanguageCultureExtensionCode));
            element.Add(new XAttribute("Count", Count().ToString()));
            foreach (IBaseObjectKeyed item in Store)
                element.Add(item.Xml);
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
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageIDNoAdd(attributeValue);
                    break;
                case "AdditionalLanguageID":
                    AdditionalLanguageID = LanguageLookup.GetLanguageIDNoAdd(attributeValue);
                    break;
                case "Count":
                    if (!String.IsNullOrEmpty(attributeValue))
                        Store.Capacity = Convert.ToInt32(attributeValue);
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
                InternalAdd(item);

            return true;
        }

        public static int Compare(IObjectStore other1, IObjectStore other2)
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
            IObjectStore otherObjectStore = other as IObjectStore;
            if ((otherObjectStore == null) || (otherObjectStore.Count() == 0))
            {
                if (Count() == 0)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (LanguageID != otherObjectStore.LanguageID)
                    return LanguageID.Compare(LanguageID, otherObjectStore.LanguageID);
                if (AdditionalLanguageID != otherObjectStore.AdditionalLanguageID)
                    return LanguageID.Compare(AdditionalLanguageID, otherObjectStore.AdditionalLanguageID);
                if (Count() == 0)
                    return -1;
                else if (Count() == otherObjectStore.Count())
                {
                    List<IBaseObjectKeyed> list1 = GetAll();
                    List<IBaseObjectKeyed> list2 = otherObjectStore.GetAll();
                    int index;
                    int count = list1.Count();
                    for (index = 0; index < count; index++)
                    {
                        IBaseObjectKeyed item1 = list1[index];
                        IBaseObjectKeyed item2 = list2[index];
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

        public virtual void InitializeCache()
        {
        }

        public virtual void EnableCache(bool enable)
        {
        }

        public virtual void LoadCache()
        {
        }

        public virtual void SaveCache()
        {
        }

        public virtual void ClearCache()
        {
        }

        public virtual IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item)
        {
            return item;
        }

        public virtual void CacheCheckList(List<IBaseObjectKeyed> list)
        {
        }

        public virtual IObjectStore Mirror { get; set; }

        public virtual bool IsMirror { get; set; }

        public virtual bool Synchronize()
        {
            return true;
        }

        protected bool InternalContains(object key)
        {
            if (key == null)
                return false;

            if (key is string)
            {
                string stringKey = (string)key;

                if (StringDictionary.ContainsKey(stringKey))
                    return true;
            }
            else if (key is int)
            {
                int intKey = (int)key;

                if (IntDictionary.ContainsKey(intKey))
                    return true;
            }
            else
            {
                if (Store.FirstOrDefault(x => x.CompareKey(key) == 0) != null)
                    return true;
            }

            return false;
        }

        protected IBaseObjectKeyed InternalGet(object key)
        {
            if (key == null)
                return null;

            IBaseObjectKeyed item = null;

            if (key is string)
            {
                string stringKey = (string)key;

                if (StringDictionary.TryGetValue(stringKey, out item))
                    return item;
            }
            else if (key is int)
            {
                int intKey = (int)key;

                if (IntDictionary.TryGetValue(intKey, out item))
                    return item;
            }
            else
                return Store.FirstOrDefault(x => x.CompareKey(key) == 0);

            return null;
        }

        protected bool InternalAdd(IBaseObjectKeyed item)
        {
            if (item == null)
                return false;

            object key = item.Key;

            if (key is string)
            {
                string stringKey = (string)key;

                if (StringDictionary.ContainsKey(stringKey))
                    return false;

                StringDictionary.Add(stringKey, item);
            }
            else if (key is int)
            {
                int intKey = (int)key;

                if (IntDictionary.ContainsKey(intKey))
                    return false;

                IntDictionary.Add(intKey, item);
            }

            Store.Add(item);
            Touch();

            return true;
        }

        protected bool InternalDelete(IBaseObjectKeyed item)
        {
            if (item == null)
                return false;

            object key = item.Key;

            if (key is string)
            {
                string stringKey = (string)key;
                StringDictionary.Remove(stringKey);
            }
            else if (key is int)
            {
                int intKey = (int)key;
                IntDictionary.Remove(intKey);
            }

            Touch();

            return Store.Remove(item);
        }
    }
}
