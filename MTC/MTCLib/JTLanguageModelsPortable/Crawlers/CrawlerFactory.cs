using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Formats;

namespace JTLanguageModelsPortable.Crawlers
{
    public class CrawlerFactory
    {
        protected Dictionary<string, Crawler> _Crawlers;

        public CrawlerFactory()
        {
            _Crawlers = new Dictionary<string, Crawler>();
        }

        public Dictionary<string, Crawler> Crawlers
        {
            get
            {
                return _Crawlers;
            }
            set
            {
                _Crawlers = value;
            }
        }

        public void Add(Crawler crawler)
        {
            if (_Crawlers.ContainsKey(crawler.Name))
                return;

            _Crawlers.Add(crawler.Name, crawler);
        }

        public virtual Crawler Create(string name, FormatCrawler format)
        {
            Crawler crawler = null;

            if (_Crawlers.TryGetValue(name, out crawler))
            {
                crawler = crawler.Clone();

                if (format != null)
                    crawler.InitializeFromFormat(format);
            }

            return crawler;
        }

        public virtual T CreateTyped<T>(string name, FormatCrawler format) where T : Crawler
        {
            return Create(name, format) as T;
        }

        public List<string> GetFilteredNames(UserRecord userRecord)
        {
            List<string> names = new List<string>();
            string errorMessage = null;

            foreach (KeyValuePair<string, Crawler> kvp in _Crawlers)
            {
                Crawler crawler = kvp.Value;

                if (crawler.PermissionsCheck(userRecord, ref errorMessage))
                    names.Add(kvp.Value.Name);
            }

            return names;
        }

        public List<string> Names
        {
            get
            {
                List<string> names = new List<string>();

                foreach (KeyValuePair<string, Crawler> kvp in _Crawlers)
                    names.Add(kvp.Value.Name);

                return names;
            }
        }

        public List<string> Types
        {
            get
            {
                List<string> types = new List<string>();

                foreach (KeyValuePair<string, Crawler> kvp in _Crawlers)
                    types.Add(kvp.Value.Type);

                return types;
            }
        }

        public List<string> GetSupportedNames(string componentName, string capability)
        {
            List<string> names = new List<string>();

            foreach (KeyValuePair<string, Crawler> kvp in _Crawlers)
            {
                if (kvp.Value.IsSupportedVirtual(componentName, capability))
                    names.Add(kvp.Value.Name);
            }

            return names;
        }

        public List<string> GetSupportedTypes(string componentName, string capability)
        {
            List<string> types = new List<string>();

            foreach (KeyValuePair<string, Crawler> kvp in _Crawlers)
            {
                if (kvp.Value.IsSupportedVirtual(componentName, capability))
                    types.Add(kvp.Value.Type);
            }

            return types;
        }
    }
}
