using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseObjectStore : CachedObjectStore, IObjectStore
    {
        public LanguageID LanguageID { get; set; }
        public LanguageID AdditionalLanguageID { get; set; }
        public DatabaseTable Table { get; set; }
        public string TracerName { get; set; }

        public DatabaseObjectStore(string name, LanguageID languageID, DatabaseTable table, CacheOptions cacheOptions)
            : base(name, cacheOptions)
        {
            LanguageID = languageID;
            AdditionalLanguageID = null;
            Table = table;
            if (name.Contains("_"))
                name = name.Substring(0, name.IndexOf('_'));
            TracerName = name;
            Table.Initialize();
        }

        public DatabaseObjectStore(string name, LanguageID languageID, LanguageID additionalLanguageID, DatabaseTable table, CacheOptions cacheOptions)
            : base(name, cacheOptions)
        {
            LanguageID = languageID;
            AdditionalLanguageID = additionalLanguageID;
            Table = table;
            if (name.Contains("_"))
                name = name.Substring(0, name.IndexOf('_'));
            TracerName = name;
            Table.Initialize();
        }

        public DatabaseObjectStore(IObjectStore other, DatabaseTable table, CacheOptions cacheOptions)
            : base(table.KeyString, cacheOptions)
        {
            LanguageID = other.LanguageID;
            AdditionalLanguageID = other.AdditionalLanguageID;
            Table = table;
            string name = table.Name;
            if (name.Contains("_"))
                name = name.Substring(0, name.IndexOf('_'));
            TracerName = name;
            Table.Initialize();
            CreateStoreCheck();
            DeleteAll();
            AddList(other.GetAll());
        }

        public override void Clear()
        {
            base.Clear();
            ClearDatabaseObjectStore();
        }

        public void ClearDatabaseObjectStore()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.ClearDatabaseObjectStore");
            if (Table != null)
                Table.DeleteAll();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.ClearDatabaseObjectStore");
        }

        public virtual bool StoreExists()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.StoreExists");
            bool returnValue = Table.TableExists();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.StoreExists");
            return returnValue;
        }

        public virtual bool CreateStore()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.CreateStore");
            if (Cache != null)
            {
                Cache.Clear();
                SaveCache();
            }
            bool returnValue = Table.CreateTable();
            Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.CreateStore");
            return returnValue;
        }

        public virtual bool CreateStoreCheck()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.CreateStoreCheck");
            bool returnValue = Table.CreateTableCheck();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.CreateStoreCheck");
            return returnValue;
        }

        public virtual bool DeleteStore()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.DeleteStore");
            if (Cache != null)
            {
                Cache.Clear();
                SaveCache();
            }
            bool returnValue = Table.DeleteTable();
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.DeleteStore");
            return returnValue;
        }

        public virtual bool DeleteStoreCheck()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.DeleteStoreCheck");
            if (Cache != null)
            {
                Cache.Clear();
                SaveCache();
            }
            bool returnValue = Table.DeleteTableCheck();
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.DeleteStoreCheck");
            return returnValue;
        }

        public virtual bool RecreateStoreCheck()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.RecreateStoreCheck");
            if (Cache != null)
                Cache.Clear();
            bool returnValue = Table.RecreateTableCheck();
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.RecreateStoreCheck");
            return returnValue;
        }

        public IBaseObjectKeyed Get(object key)
        {
            IBaseObjectKeyed returnValue = CacheCheckKey(key);
            if (returnValue != null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Get");
            returnValue = Table.Get(key) as IBaseObjectKeyed;
            AddToCache(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Get");
            return returnValue;
        }

        public IBaseObjectKeyed GetFirst(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.GetFirst");
            IBaseObjectKeyed returnValue = Table.GetFirst(matcher) as IBaseObjectKeyed;
            returnValue = CacheCheckObject(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.GetFirst");
            return returnValue;
        }

        public IBaseObjectKeyed GetIndexed(int index)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.GetIndexed");
            IBaseObjectKeyed returnValue = Table.GetIndexed(index) as IBaseObjectKeyed;
            returnValue = CacheCheckObject(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.GetIndexed");
            return returnValue;
        }

        public List<IBaseObjectKeyed> GetAll()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.GetAll");
            List<IBaseObjectKeyed> returnValue = Table.GetAll().Cast<IBaseObjectKeyed>().ToList();
            //CacheCheckList(returnValue);
            ClearCache();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.GetAll");
            return returnValue;
        }

        public List<object> GetAllKeys()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.GetAllKeys");
            List<object> returnValue = Table.GetKeys();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.GetAllKeys");
            return returnValue;
        }

        public List<object> QueryKeys(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.QueryKeys");
            List<object> returnValue = Table.QueryKeys(matcher);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.QueryKeys");
            return returnValue;
        }

        public List<IBaseObjectKeyed> Query(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Query");
            List<IBaseObjectKeyed> returnValue = Table.Query(matcher).Cast<IBaseObjectKeyed>().ToList();
            CacheCheckList(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Query");
            return returnValue;
        }

        public int QueryCount(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.QueryCount");
            int count = Table.QueryCount(matcher);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.QueryCount");
            return count;
        }

        public bool Contains(object key)
        {
            if (CacheCheckKey(key) != null)
                return true;
            if (key == null)
                return false;
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Contains");
            bool returnValue = Table.ContainsKey(key);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Contains");
            return returnValue;
        }

        public bool Contains(Matcher matcher)
        {
            if (Cache != null)
            {
                lock (this)
                {
                    if (Cache.FirstOrDefault(x => matcher.Match(x)) != null)
                        return true;
                }
            }
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Contains");
            bool returnValue = Table.Contains(matcher);
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Contains");
            return returnValue;
        }

        public bool Add(IBaseObjectKeyed item)
        {
            Boolean returnValue = false;

            if (item == null)
                return returnValue;

            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Add");

            if ((item.Key == null) || !Table.ContainsKey(item.Key))
            {
                Table.Add(item);
                Modified = true;
                returnValue = true;

                if (Cache != null)
                    AddToCache(item);

                Touch();
            }

            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Add");

            return returnValue;
        }

        public bool AddList(List<IBaseObjectKeyed> items)
        {
            if (items == null)
                return false;

            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.AddList");

            Modified = true;

            bool returnValue = Table.AddList(items);

            AddListToCache(items);

            Touch();

            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.AddList");

            return returnValue;
        }

        public bool CopyFrom(IObjectStore other, int startIndex = 0, int count = -1)
        {
            if (other == null)
                return false;
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.CopyFrom");
            int otherCount = other.Count();
            IBaseObjectKeyed item;
            if (count == -1)
                count = otherCount - startIndex;
            int endIndex = startIndex + count;
            if (endIndex > otherCount)
                endIndex = otherCount;
            bool returnValue = true;
            for (; startIndex < endIndex; startIndex++)
            {
                item = other.GetIndexed(startIndex);
                if (item == null)
                    return false;
                //Add(item);
                try
                {
                    Table.Add(item);
                }
                catch (Exception)
                {
                    returnValue = false;
                }
            }
            Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.CopyFrom");
            return returnValue;
        }

        public bool Update(IBaseObjectKeyed item)
        {
            bool returnValue = false;

            if (item == null)
                return returnValue;

            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Update");
            Modified = true;

            CacheUpdateObject(item);

            if ((item.Key != null) && Table.ContainsKey(item.Key))
            {
                returnValue = Table.Update(item);

                if (returnValue)
                    Touch();
            }
            else
                returnValue = Add(item);

            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Update");

            return returnValue;
        }

        public bool UpdateList(List<IBaseObjectKeyed> items)
        {
            if (items == null)
                return false;

            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.AddList");

            Modified = true;

            bool returnValue = Table.UpdateList(items);

            AddListToCache(items);

            Touch();

            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.AddList");

            return returnValue;
        }

        public bool Delete(IBaseObjectKeyed item)
        {
            if (item == null)
                return false;
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Delete");
            Modified = true;
            bool returnValue = Table.Delete(item);
            if (Cache != null)
            {
                bool needSave = false;

                lock (this)
                {
                    IBaseObjectKeyed cacheItem = Cache.FirstOrDefault(x => x.MatchKey(item.Key));
                    if (cacheItem != null)
                    {
                        Cache.Remove(cacheItem);
                        needSave = true;
                    }
                }

                if (needSave)
                    SaveCache();
            }
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Delete");
            return returnValue;
        }

        public bool DeleteList(List<IBaseObjectKeyed> items)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.DeleteList");
            if (items == null)
                return false;
            Modified = true;
            bool returnValue = Table.DeleteList(items);
            if (Cache != null)
            {
                int count = 0;
                lock (this)
                {
                    foreach (IBaseObjectKeyed listItem in items)
                    {
                        IBaseObjectKeyed cacheItem = Cache.FirstOrDefault(x => x.MatchKey(listItem.Key));
                        if (cacheItem != null)
                        {
                            Cache.Remove(cacheItem);
                            count++;
                        }
                    }
                }
                if (count != 0)
                    SaveCache();
            }
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.DeleteList");
            return returnValue;
        }

        public bool DeleteKeyList(List<object> keys)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.DeleteKeyList");
            if (keys == null)
                return false;
            Modified = true;
            bool returnValue = Table.DeleteKeyList(keys);
            if (Cache != null)
            {
                int count = 0;
                lock (this)
                {
                    foreach (object key in keys)
                    {
                        IBaseObjectKeyed cacheItem = Cache.FirstOrDefault(x => x.MatchKey(key));
                        if (cacheItem != null)
                        {
                            Cache.Remove(cacheItem);
                            count++;
                        }
                    }
                }
                if (count != 0)
                    SaveCache();
            }
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.DeleteKeyList");
            return returnValue;
        }

        public int DeleteQuery(Matcher matcher)
        {
            int count = 0;

            if ((matcher is StringMatcher) && (matcher.MemberName == "Key") && (((StringMatcher)matcher).Patterns != null) &&
                    (((StringMatcher)matcher).Patterns.Count() >= 1))
            {
                List<string> patterns = ((StringMatcher)matcher).Patterns;
                List<object> keys = GetAllKeys();
                List<object> deleteKeys = new List<object>();

                foreach (string pattern in patterns)
                {
                    foreach (object key in keys)
                    {
                        bool badPattern;

                        if (key == null)
                            continue;

                        if (pattern == null)
                            continue;

                        if (TextUtilities.RegexMatch(key.ToString(), pattern, out badPattern))
                        {
                            deleteKeys.Add(key);
                        }

                        if (badPattern)
                            return 0;
                    }
                }

                if (deleteKeys.Count() != 0)
                {
                    if (DeleteKeyList(deleteKeys))
                        count = deleteKeys.Count();
                }
            }
            else
            {
                List<object> keys = QueryKeys(matcher);

                if ((keys != null) && (keys.Count() != 0))
                {
                    if (DeleteKeyList(keys))
                        count = keys.Count();
                }
            }

            return count;
        }

        public bool DeleteKey(object key)
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.DeleteKey");
            if (key == null)
                return false;
            Modified = true;
            if (Cache != null)
            {
                bool needSave = false;

                lock (this)
                {
                    IBaseObjectKeyed cacheItem = Cache.FirstOrDefault(x => x.MatchKey(key));
                    if (cacheItem != null)
                    {
                        Cache.Remove(cacheItem);
                        needSave = true;
                    }
                }

                if (needSave)
                    SaveCache();
            }
            bool returnValue = Table.DeleteKey(key);
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.DeleteKey");
            return returnValue;
        }

        public bool DeleteAll()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.DeleteAll");
            Modified = true;
            if (Cache != null)
            {
                Cache.Clear();
                SaveCache();
            }
            bool returnValue = Table.DeleteAll();
            if (returnValue)
                Touch();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.DeleteAll");
            return returnValue;
        }

        public int Count()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.Count");
            int returnValue = Table.Count();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.Count");
            return returnValue;
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
                    if ((value = GetIndexed((int)command.GetBaseArgumentIndexed(0).Key)) != null)
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
                case "ContainsQuery":
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

        public override string CacheDirectory
        {
            get
            {
                return DatabaseMainRepository.CacheDirectory;
            }
            set
            {
            }
        }

        protected override List<IBaseObjectKeyed> GetAllRaw()
        {
            List<IBaseObjectKeyed> returnValue = null;
            lock (this)
            {
                returnValue = Table.GetAll().Cast<IBaseObjectKeyed>().ToList();
                if (returnValue == null)
                    returnValue = new List<IBaseObjectKeyed>();
            }
            return returnValue;
        }

        public override void LoadCache()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.LoadCache");
            base.LoadCache();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.LoadCache");
        }

        public override void SaveCache()
        {
            TimeTracer.RecordEventStatic(TracerName, "DatabaseObjectStore.SaveCache");
            base.SaveCache();
            TimeTracer.RecordEventEndStatic(TracerName, "DatabaseObjectStore.SaveCache");
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if ((LanguageID != null) && !String.IsNullOrEmpty(LanguageID.LanguageCultureExtensionCode))
                element.Add(new XAttribute("LanguageID", LanguageID.LanguageCultureExtensionCode));
            if ((AdditionalLanguageID != null) && !String.IsNullOrEmpty(AdditionalLanguageID.LanguageCultureExtensionCode))
                element.Add(new XAttribute("AdditionalLanguageID", AdditionalLanguageID.LanguageCultureExtensionCode));
            List<IBaseObject> list = Table.GetAll();
            int count = list.Count();
            element.Add(new XAttribute("Count", count.ToString()));
            foreach (IBaseObject item in list)
                element.Add(item.Xml);
            return element;
        }

        protected static List<IBaseObjectKeyed> _PendingObjects;

        public override void OnElement(XElement element)
        {
            _PendingObjects = new List<IBaseObjectKeyed>();
            base.OnElement(element);
            if ((_PendingObjects != null) && (_PendingObjects.Count() != 0))
                Table.AddList(_PendingObjects);
            _PendingObjects = null;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageID(attributeValue);
                    break;
                case "AdditionalLanguageID":
                    AdditionalLanguageID = LanguageLookup.GetLanguageIDNoAdd(attributeValue);
                    break;
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
                    Table.Add(item);
            }

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
    }
}
