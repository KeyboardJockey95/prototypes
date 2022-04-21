using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Service
{
    public class ClientObjectStore : CachedObjectStore, IObjectStore
    {
        public LanguageID LanguageID { get; set; }
        public LanguageID AdditionalLanguageID { get; set; }
        public FixupDictionary Fixups { get; set; }
        public ClientServiceBase Service { get; set; }
        public string TracerName { get; set; }

        public UserID UserID
        {
            get
            {
                return ClientMainRepository.UserID;
            }
        }

        public bool RequiresAuthentication
        {
            get
            {
                return ClientMainRepository.RequiresAuthentication;
            }
        }

        public ClientObjectStore(string name, LanguageID languageID, ClientServiceBase service, CacheOptions cacheOptions)
            : base(name, cacheOptions)
        {
            ObjectStore = this;
            LanguageID = languageID;
            AdditionalLanguageID = null;
            Service = service;
            if (name.Contains("_"))
                name = name.Substring(0, name.IndexOf("_"));
            TracerName = name;
        }

        public ClientObjectStore(string name, LanguageID languageID1, LanguageID languageID2, ClientServiceBase service, CacheOptions cacheOptions)
            : base(name, cacheOptions)
        {
            ObjectStore = this;
            LanguageID = languageID1;
            AdditionalLanguageID = languageID2;
            Service = service;
            if (name.Contains("_"))
                name = name.Substring(0, name.IndexOf("_"));
            TracerName = name;
        }

        public ClientObjectStore()
        {
            ObjectStore = this;
            LanguageID = null;
            Service = null;
            TracerName = String.Empty;
        }

        public override void Clear()
        {
            base.Clear();
            ClearClientObjectStore();
        }

        public void ClearClientObjectStore()
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.ClearClientObjectStore");
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.ClearClientObjectStore");
        }

        public virtual bool StoreExists()
        {
            /*
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "StoreExists", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            return false;
            */
            return true;
        }

        public virtual bool CreateStore()
        {
            /*
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "CreateStore", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            return false;
            */
            return true;
        }

        public virtual bool CreateStoreCheck()
        {
            /*
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "CreateStoreCheck", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            return false;
            */
            return true;
        }

        public virtual bool DeleteStore()
        {
            /*
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "DeleteStore", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            return false;
            */
            return true;
        }

        public virtual bool DeleteStoreCheck()
        {
            /*
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "DeleteStoreCheck", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            return false;
            */
            return true;
        }

        public virtual bool RecreateStoreCheck()
        {
            /*
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "RecreateStoreCheck", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            return false;
            */
            return true;
        }

        public virtual IBaseObjectKeyed Get(object key)
        {
            IBaseObjectKeyed returnValue = CacheCheckKey(key);
            if (returnValue != null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Get");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Get", new List<IBaseObject>() { new BaseObjectKeyed(key) });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = result.GetBaseArgumentIndexed(0);
            AddToCache(returnValue);
            AddToMirrorCheck(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Get");
            return returnValue;
        }

        public virtual IBaseObjectKeyed GetFirst(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.GetFirst");
            IBaseObjectKeyed returnValue = null;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "GetFirst", new List<IBaseObject>() { matcher });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = result.GetBaseArgumentIndexed(0);
            AddToMirrorCheck(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.GetFirst");
            return returnValue;
        }

        public virtual IBaseObjectKeyed GetIndexed(int index)
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.GetIndexed");
            IBaseObjectKeyed returnValue = null;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "GetIndexed", new List<IBaseObject>() { new BaseObjectKeyed(index) });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = result.GetBaseArgumentIndexed(0);
            AddToMirrorCheck(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.GetIndexed");
            return returnValue;
        }

        public virtual List<IBaseObjectKeyed> GetAll()
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.GetAll");
            List<IBaseObjectKeyed> returnValue = null;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "GetAll", null);
            MessageBase result = Service.ExecuteCommand(command);
            if (result != null)
            {
                if ((returnValue = result.BaseArguments) == null)
                    returnValue = new List<IBaseObjectKeyed>();
            }
            else
                returnValue = new List<IBaseObjectKeyed>();
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.GetAll");
            return returnValue;
        }

        public virtual List<object> GetAllKeys()
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.GetAllKeys");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "GetAllKeys", null);
            MessageBase result = Service.ExecuteCommand(command);
            List<object> keys = new List<object>();
            if (result != null)
            {
                List<IBaseObjectKeyed> list = result.BaseArguments;

                foreach (IBaseObjectKeyed obj in list)
                    keys.Add(obj.Key);
            }
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.GetAllKeys");
            return keys;
        }

        public List<object> QueryKeys(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.QueryKeys");
            List<object> returnValue = null;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Query", new List<IBaseObject>() { matcher });
            MessageBase result = Service.ExecuteCommand(command);
            if (result != null)
                returnValue = result.BaseArgumentKeys;
            if (returnValue == null)
                returnValue = new List<object>();
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.QueryKeys");
            return returnValue;
        }

        public List<IBaseObjectKeyed> Query(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Query");
            List<IBaseObjectKeyed> returnValue = null;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Query", new List<IBaseObject>() { matcher });
            MessageBase result = Service.ExecuteCommand(command);
            if (result != null)
                returnValue = result.BaseArguments;
            if (returnValue == null)
                returnValue = new List<IBaseObjectKeyed>();
            else
                CacheCheckList(returnValue);
            AddListToMirrorCheck(returnValue);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Query");
            return returnValue;
        }

        public int QueryCount(Matcher matcher)
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.QueryCount");
            int count = 0;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "QueryCount", new List<IBaseObject>() { matcher });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                count = (int)result.GetBaseArgumentIndexed(0).Key;
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.QueryCount");
            return count;
        }

        public virtual bool Contains(object key)
        {
            if (CacheCheckKey(key) != null)
                return true;
            bool returnValue = false;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Contains");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Contains", new List<IBaseObject>() { new BaseObjectKeyed(key) });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Contains");
            return returnValue;
        }

        public virtual bool Contains(Matcher matcher)
        {
            if (Cache != null)
            {
                lock (this)
                {
                    if (Cache.FirstOrDefault(x => matcher.Match(x)) != null)
                        return true;
                }
            }
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Contains");
            bool returnValue = false;
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "ContainsQuery", new List<IBaseObject>() { matcher });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Contains");
            return returnValue;
        }

        public virtual bool Add(IBaseObjectKeyed item)
        {
            Boolean returnValue = false;
            if (item == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Add");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Add", new List<IBaseObject>() { item });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            if (Cache != null)
                AddToCache(item);
            AddToMirrorCheck(item);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Add");
            return returnValue;
        }

        public virtual bool AddList(List<IBaseObjectKeyed> items)
        {
            Boolean returnValue = false;
            if (items == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.AddList");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "AddList", items.Cast<IBaseObject>().ToList());
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            AddListToCache(items);
            AddListToMirrorCheck(items);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.AddList");
            return returnValue;
        }

        public virtual bool CopyFrom(IObjectStore other, int startIndex = 0, int count = -1)
        {
            if (other == null)
                return false;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.CopyFrom");
            int otherCount = other.Count();
            IBaseObjectKeyed item;
            if (count == -1)
                count = otherCount - startIndex;
            int endIndex = startIndex + count;
            if (endIndex > otherCount)
                endIndex = otherCount;
            List<IBaseObjectKeyed> list = new List<IBaseObjectKeyed>(count);
            for (; startIndex < endIndex; startIndex++)
            {
                item = other.GetIndexed(startIndex);
                if (item == null)
                    return false;
                list.Add(item);
            }
            bool returnValue = AddList(list);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.CopyFrom");
            return returnValue;
        }

        public virtual bool Update(IBaseObjectKeyed item)
        {
            bool returnValue = false;
            if (item == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Update");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Update", new List<IBaseObject>() { item });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            UpdateMirrorCheck(item);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Update");
            return returnValue;
        }

        public virtual bool UpdateList(List<IBaseObjectKeyed> items)
        {
            Boolean returnValue = false;
            if (items == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.AddList");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "UpdateList", items.Cast<IBaseObject>().ToList());
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            AddListToCache(items);
            UpdateListMirrorCheck(items);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.AddList");
            return returnValue;
        }

        public virtual bool Delete(IBaseObjectKeyed item)
        {
            bool returnValue = false;
            if (item == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Delete");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Delete", new List<IBaseObject>() { item });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
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
            if (Mirror != null)
                Mirror.Delete(item);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Delete");
            return returnValue;
        }

        public virtual bool DeleteList(List<IBaseObjectKeyed> items)
        {
            bool returnValue = false;
            if (items == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.DeleteList");
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
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "DeleteList", items.Cast<IBaseObject>().ToList());
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            if (Mirror != null)
                Mirror.DeleteList(items);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.DeleteList");
            return returnValue;
        }

        public virtual bool DeleteKeyList(List<object> keys)
        {
            bool returnValue = false;
            if (keys == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.DeleteKeyList");
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
            List<IBaseObject> objs = new List<IBaseObject>();
            foreach (IBaseObjectKeyed key in keys)
                objs.Add(new BaseObjectKeyed(key));
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "DeleteKeyList", objs);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            if (Mirror != null)
                Mirror.DeleteKeyList(keys);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.DeleteKeyList");
            return returnValue;
        }

        public int DeleteQuery(Matcher matcher)
        {
            int count = 0;

            if ((matcher is StringMatcher) && (matcher.MemberName == "Key") && (((StringMatcher)matcher).Patterns != null) &&
                    (((StringMatcher)matcher).Patterns.Count() == 1))
            {
                string pattern = ((StringMatcher)matcher).Patterns[0];
                List<object> keys = GetAllKeys();
                List<object> deleteKeys = new List<object>();

                foreach (object key in keys)
                {
                    bool badPattern;

                    if (TextUtilities.RegexMatch(key.ToString(), pattern, out badPattern))
                        deleteKeys.Add(key);

                    if (badPattern)
                        break;
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

        public virtual bool DeleteKey(object key)
        {
            bool returnValue = false;
            if (key == null)
                return returnValue;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.DeleteKey");
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
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "DeleteKey", new List<IBaseObject>() { new BaseObjectKeyed(key) });
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                return (bool)result.GetBaseArgumentIndexed(0).Key;
            if (Mirror != null)
                Mirror.DeleteKey(key);
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.DeleteKey");
            return returnValue;
        }

        public virtual bool DeleteAll()
        {
            bool returnValue = false;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.DeleteAll");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "DeleteAll", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (bool)result.GetBaseArgumentIndexed(0).Key;
            if (Mirror != null)
                Mirror.DeleteAll();
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.DeleteAll");
            return returnValue;
        }

        public virtual int Count()
        {
            int returnValue = 0;
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.Count");
            MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "Count", null);
            MessageBase result = Service.ExecuteCommand(command);
            if ((result != null) && (result.GetArgumentCount() != 0))
                returnValue = (int)result.GetBaseArgumentIndexed(0).Key;
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.Count");
            return returnValue;
        }

        public override DateTime CreationTime
        {
            get
            {
                DateTime returnValue = DateTime.MinValue;
                TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.CreationTime");
                MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "CreationTime", null);
                MessageBase result = Service.ExecuteCommand(command);
                if ((result != null) && (result.GetArgumentCount() != 0))
                    returnValue = (DateTime)result.GetBaseArgumentIndexed(0).Key;
                TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.CreationTime");
                return returnValue;
            }
            set
            {
            }
        }

        public override DateTime ModifiedTime
        {
            get
            {
                DateTime returnValue = DateTime.MinValue;
                TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.ModifiedTime");
                MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "ModifiedTime", null);
                MessageBase result = Service.ExecuteCommand(command);
                if ((result != null) && (result.GetArgumentCount() != 0))
                    returnValue = (DateTime)result.GetBaseArgumentIndexed(0).Key;
                TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.ModifiedTime");
                return returnValue;
            }
            set
            {
            }
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
                    resultArguments.Add(new BaseObjectKeyed(CreationTime));
                    break;
                case "CreationTime":
                    resultArguments.Add(new BaseObjectKeyed(CreationTime));
                    break;
                case "ModifiedTime":
                    resultArguments.Add(new BaseObjectKeyed(ModifiedTime));
                    break;
                default:
                    throw new Exception("ObjectStore.Dispatch: Unknown command \"" + command.MessageName + "\"");
            }

            return result;
        }

        public override string CacheDirectory
        {
            get
            {
                return ClientMainRepository.CacheDirectory;
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
                MessageBase command = new MessageBase(-1, UserID, RequiresAuthentication, KeyString, "GetAll", null);
                MessageBase result = Service.ExecuteCommand(command);
                if (result != null)
                    returnValue = result.BaseArguments;
                if (returnValue == null)
                    returnValue = new List<IBaseObjectKeyed>();
            }
            return returnValue;
        }

        public override void LoadCache()
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.LoadCache");
            base.LoadCache();
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.LoadCache");
        }

        public override void SaveCache()
        {
            TimeTracer.RecordEventStatic(TracerName, "ClientObjectStore.SaveCache");
            base.SaveCache();
            TimeTracer.RecordEventEndStatic(TracerName, "ClientObjectStore.SaveCache");
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if ((LanguageID != null) && !String.IsNullOrEmpty(LanguageID.LanguageCultureExtensionCode))
                element.Add(new XAttribute("LanguageID", LanguageID.LanguageCultureExtensionCode));
            element.Add(new XAttribute("Count", Count().ToString()));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageID(attributeValue);
                    break;
                case "Count":
                    if (!String.IsNullOrEmpty(attributeValue))
                        Convert.ToInt32(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            IBaseObjectKeyed item = ObjectUtilities.ResurrectBaseObject(childElement) as IBaseObjectKeyed;
            return true;
        }
    }
}
