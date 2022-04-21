using System;
using System.Collections.Generic;
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
    public class ClientLanguagePairObjectStore : LanguagePairObjectStore
    {
        public ClientServiceBase Service { get; set; }
        public CacheOptions CacheOptions { get; set; }
        public bool CacheEnabled { get; set; }

        public ClientLanguagePairObjectStore(string name, ClientServiceBase service, CacheOptions cacheOptions)
            : base(name, 100)
        {
            Service = service;
            CacheOptions = cacheOptions;
        }

        public override IObjectStore CreateObjectStore(LanguageID languageID1, LanguageID languageID2)
        {
            IObjectStore store = new ClientObjectStore(
                Key + "_" + languageID1.SymbolName + "_" + languageID2.SymbolName,
                languageID1,
                languageID2,
                Service,
                CacheOptions);
            store.EnableCache(CacheEnabled);
            return store;
        }
    }
}
