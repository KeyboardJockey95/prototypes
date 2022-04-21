using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Repository
{
    public class CachedObjectStore : BaseObjectTagged
    {
        public CacheOptions CacheOptions { get; set; }
        public bool CacheEnabled { get; set; }
        public List<IBaseObjectKeyed> Cache { get; set; }
        public int CacheBumpedCount { get; set; }
        public virtual string CacheDirectory { get { return String.Empty; } set { } }
        public virtual IObjectStore ObjectStore { get; set; }
        public virtual bool IsMirror { get; set; }
        protected IObjectStore _Mirror;
        public virtual IObjectStore Mirror
        {
            get { return _Mirror; }
            set
            {
                _Mirror = value;
                IsMirror = (value == null ? false : true);
            }
        }

        public CachedObjectStore(string name, CacheOptions cacheOptions)
            : base(name)
        {
            CacheOptions = cacheOptions;
            CacheEnabled = false;
            Cache = null;
            CacheBumpedCount = 0;
            ObjectStore = null;
            Mirror = null;
            IsMirror = false;
        }

        public CachedObjectStore()
        {
            CacheOptions = null;
            CacheEnabled = false;
            Cache = null;
            CacheBumpedCount = 0;
            ObjectStore = null;
            Mirror = null;
            IsMirror = false;
        }

        public override void Clear()
        {
            base.Clear();
            ClearCachedObjectStore();
        }

        public void ClearCachedObjectStore()
        {
            if (Cache != null)
            {
                Cache.Clear();
                SaveCache();
            }
        }

        public int CacheSize
        {
            get
            {
                return CacheOptions.CacheSize;
            }
            set
            {
                CacheOptions.CacheSize = value;
            }
        }

        public virtual void InitializeCache()
        {
            lock (this)
            {
                Cache = null;

                if (CacheEnabled && (CacheSize != 0))
                    LoadCache();
            }
        }

        public virtual void EnableCache(bool enable)
        {
            if (CacheSize != 0)
                CacheEnabled = enable;
            else
                CacheEnabled = false;

            InitializeCache();
        }

        protected virtual List<IBaseObjectKeyed> GetAllRaw()
        {
            return new List<IBaseObjectKeyed>();
        }

        protected virtual void LoadCacheRaw()
        {
            if ((CacheSize != 0) && !String.IsNullOrEmpty(CacheDirectory))
            {
                lock (this)
                {
                    if (Cache == null)
                    {
                        if (CacheSize == -1)
                            Cache = new List<IBaseObjectKeyed>(128);
                        else
                            Cache = new List<IBaseObjectKeyed>(CacheSize);
                    }

                    string path = CacheDirectory + KeyString + ".xml";

                    try
                    {
                        using (Stream stream = FileSingleton.Open(path, PortableFileMode.Open))
                        {
                            if (stream != null)
                            {
                                using (StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding(true)))
                                {
                                    XElement root = XElement.Load(reader, LoadOptions.PreserveWhitespace);

                                    foreach (XElement childElement in root.Elements())
                                    {
                                        if (Cache.Count() >= CacheSize)
                                            break;

                                        IBaseObjectKeyed item = ObjectUtilities.ResurrectBase(childElement);
                                        Cache.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (Cache != null)
                Cache = null;
        }

        public virtual void LoadCache()
        {
            if (CacheOptions.PreloadAll)
            {
                if (CacheSize != 0)
                    Cache = GetAllRaw();
            }
            else if (CacheOptions.FileStorage)
            {
                LoadCacheRaw();
            }
            else if ((CacheSize != 0) && (Cache == null))
            {
                if (CacheSize == -1)
                    Cache = new List<IBaseObjectKeyed>(128);
                else
                    Cache = new List<IBaseObjectKeyed>(CacheSize);
            }
        }

        protected virtual void SaveCacheRaw()
        {
            lock (this)
            {
                if ((Cache != null) && !String.IsNullOrEmpty(CacheDirectory))
                {
                    string name = Name;

                    if (name.Contains("_"))
                        name = name.Substring(0, name.IndexOf("_"));

                    XElement root = new XElement(name);

                    foreach (IBaseObjectKeyed item in Cache)
                        root.Add(item.Xml);

                    string path = CacheDirectory + KeyString + ".xml";

                    try
                    {
                        using (Stream stream = FileSingleton.Create(path))
                        {
                            using (StreamWriter writer = new StreamWriter(stream, new System.Text.UTF8Encoding(true)))
                            {
                                root.Save(writer);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public virtual void SaveCache()
        {
            if (CacheOptions.FileStorage)
                SaveCacheRaw();
        }

        public virtual void ClearCache()
        {
            lock (this)
            {
                if (Cache != null)
                {
                    Cache.Clear();

                    if (!String.IsNullOrEmpty(CacheDirectory))
                        SaveCache();
                }
            }
        }

        public virtual void AddToCache(IBaseObjectKeyed item)
        {
            if ((Cache != null) && (item != null))
            {
                lock (this)
                {
                    int count;

                    if (CacheSize == -1)
                        Cache.Insert(0, item);
                    else if ((count = Cache.Count()) >= CacheSize)
                    {
                        Cache.RemoveAt(count - 1);
                        Cache.Insert(0, item);
                        if (CacheOptions.FileStorage)
                        {
                            CacheBumpedCount = CacheBumpedCount + 1;
                            if (CacheBumpedCount > CacheSize / 2)
                            {
                                CacheBumpedCount = 0;
                                SaveCache();
                            }
                        }
                    }
                    else
                    {
                        Cache.Insert(0, item);
                        if (CacheOptions.FileStorage)
                            SaveCache();
                    }
                }
            }
        }

        public virtual void AddToCacheNoSave(IBaseObjectKeyed item)
        {
            int count;

            if (CacheSize == -1)
                Cache.Insert(0, item);
            else if ((count = Cache.Count()) >= CacheSize)
            {
                Cache.RemoveAt(count - 1);
                Cache.Insert(0, item);
                CacheBumpedCount = CacheBumpedCount + 1;
            }
            else
                Cache.Insert(0, item);
        }

        public virtual void AddListToCache(List<IBaseObjectKeyed> list)
        {
            if ((Cache != null) && (list != null))
            {
                lock (this)
                {
                    if (CacheOptions.FileStorage)
                    {
                        int count = Cache.Count();

                        foreach (IBaseObjectKeyed item in list)
                            AddToCacheNoSave(item);

                        if ((count < Cache.Count()) || (CacheBumpedCount > CacheSize / 2))
                        {
                            CacheBumpedCount = 0;
                            SaveCache();
                        }
                    }
                    else
                    {
                        foreach (IBaseObjectKeyed item in list)
                            AddToCacheNoSave(item);
                    }
                }
            }
        }

        public virtual IBaseObjectKeyed CacheCheckKey(object key)
        {
            if ((Cache != null) && (key != null))
            {
                lock (this)
                {
                    IBaseObjectKeyed returnValue = Cache.FirstOrDefault(x => x.MatchKey(key));
                    return returnValue;
                }
            }

            return null;
        }

        public virtual IBaseObjectKeyed CacheCheckObject(IBaseObjectKeyed item)
        {
            if (item != null)
            {
                IBaseObjectKeyed cacheItem = CacheCheckKey(item.Key);

                if (cacheItem != null)
                    item = cacheItem;
                else
                    AddToCache(item);
            }

            return item;
        }

        public virtual void CacheCheckList(List<IBaseObjectKeyed> list)
        {
            if (list == null)
                return;

            int count = list.Count();
            int index;

            for (index = 0; index < count; index++)
            {
                IBaseObjectKeyed item = list[index];
                IBaseObjectKeyed cacheItem = CacheCheckObject(item);

                if (cacheItem != item)
                    list[index] = cacheItem;
            }
        }

        public virtual void CacheUpdateObject(IBaseObjectKeyed item)
        {
            if ((Cache != null) && (item != null))
            {
                lock (this)
                {
                    IBaseObjectKeyed cacheItem = Cache.FirstOrDefault(x => x.MatchKey(item.Key));

                    if (cacheItem != null)
                    {
                        if (cacheItem != item)
                        {
                            int index = Cache.IndexOf(cacheItem);
                            Cache[index] = item;
                        }
                    }
                }
            }
        }

        public virtual void AddToMirrorCheck(IBaseObjectKeyed item)
        {
            if (!IsMirror || (Mirror == null))
                return;

            if (item == null)
                return;

            if (Mirror.Contains(item.Key))
                return;

            Mirror.Add(item);
        }

        public virtual void AddListToMirrorCheck(List<IBaseObjectKeyed> items)
        {
            if (!IsMirror || (Mirror == null))
                return;

            if (items == null)
                return;

            foreach (IBaseObjectKeyed item in items)
            {
                if (!Mirror.Contains(item.Key))
                    Mirror.Add(item);
            }
        }

        public virtual void UpdateMirrorCheck(IBaseObjectKeyed item)
        {
            if (!IsMirror || (Mirror == null))
                return;

            if (item == null)
                return;

            if (!Mirror.Contains(item.Key))
                Mirror.Add(item);
            else
                Mirror.Update(item);
        }

        public virtual void UpdateListMirrorCheck(List<IBaseObjectKeyed> items)
        {
            if (!IsMirror || (Mirror == null))
                return;

            if (items == null)
                return;

            foreach (IBaseObjectKeyed item in items)
            {
                if (!Mirror.Contains(item.Key))
                    Mirror.Update(item);
            }
        }

        public virtual bool Synchronize()
        {
            if (!IsMirror || (Mirror == null))
                return true;

            List<object> keys = Mirror.GetAllKeys();

            foreach (object key in keys)
            {
                IBaseObjectKeyed targetObject = ObjectStore.Get(key);
                IBaseObjectKeyed mirrorObject = Mirror.Get(key);

                if (mirrorObject == null)
                {
                    if (!ObjectStore.Add(targetObject))
                        return false;
                }
                else if (mirrorObject.ModifiedTime != targetObject.ModifiedTime)
                {
                    if (mirrorObject.ModifiedTime > targetObject.ModifiedTime)
                    {
                        if (!ObjectStore.Update(targetObject))
                            return false;
                    }
                    else
                    {
                        if (!Mirror.Update(targetObject))
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
