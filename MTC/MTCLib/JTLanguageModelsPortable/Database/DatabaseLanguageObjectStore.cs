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
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Repository;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseLanguageObjectStore : LanguageObjectStore
    {
        public DatabaseTableFactory TableFactory { get; set; }
        public CacheOptions CacheOptions { get; set; }
        public bool CacheEnabled { get; set; }

        public DatabaseLanguageObjectStore(string name, DatabaseTableFactory tableFactory, CacheOptions cacheOptions)
            : base(name, 100)
        {
            TableFactory = tableFactory;
            CacheOptions = cacheOptions;
        }

        public override IObjectStore CreateObjectStore(LanguageID languageID)
        {
            if (Key == null)
                throw new ObjectException("Object store is missing key.");
            string name = KeyString;
            DatabaseTable table = TableFactory.Create(name, languageID);
            IObjectStore store = new DatabaseObjectStore(
                name,
                languageID,
                table,
                CacheOptions);
            store.CreateStoreCheck();
            store.EnableCache(CacheEnabled);
            return store;
        }

        public override void EnableCache(bool enable)
        {
            CacheEnabled = enable;
            base.EnableCache(enable);
        }
    }
}
