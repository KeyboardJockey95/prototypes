using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Content
{
    public class ContentStatisticsCache : BaseObject
    {
        protected Dictionary<string, ContentStatistics> _Cache;
        protected Dictionary<string, ContentStatistics> _Map;
        protected UserRecord _UserRecord;
        protected UserProfile _UserProfile;

        public ContentStatisticsCache(UserRecord userRecord, UserProfile userProfile)
        {
            _Cache = new Dictionary<string, ContentStatistics>(StringComparer.OrdinalIgnoreCase);
            _Map = new Dictionary<string, ContentStatistics>(StringComparer.OrdinalIgnoreCase);
            _UserRecord = userRecord;
            _UserProfile = userProfile;
        }

        public ContentStatisticsCache(ContentStatisticsCache other)
        {
            CopyContentStatisticsCache(other);
        }

        public ContentStatisticsCache(XElement element)
        {
            ClearContentStatisticsCache();
            OnElement(element);
        }

        public ContentStatisticsCache()
        {
            ClearContentStatisticsCache();
        }

        public override void Clear()
        {
            ClearContentStatisticsCache();
        }

        public void ClearContentStatisticsCache()
        {
            _Cache = new Dictionary<string, ContentStatistics>(StringComparer.OrdinalIgnoreCase);
            _Map = new Dictionary<string, ContentStatistics>(StringComparer.OrdinalIgnoreCase);
            _UserRecord = null;
            _UserProfile = null;
        }

        public void CopyContentStatisticsCache(ContentStatisticsCache other)
        {
            if (other != null)
            {
                _Cache = new Dictionary<string, ContentStatistics>(other.Cache, StringComparer.OrdinalIgnoreCase);
                _Map = new Dictionary<string, ContentStatistics>(other.Map, StringComparer.OrdinalIgnoreCase);
                _UserRecord = other.UserRecord;
                _UserProfile = other.UserProfile;
            }
            else
                ClearContentStatisticsCache();
        }

        public override IBaseObject Clone()
        {
            return new ContentStatisticsCache(this);
        }

        public Dictionary<string, ContentStatistics> Cache
        {
            get { return _Cache; }
            set { _Cache = value; }
        }

        public int CacheCount
        {
            get
            {
                if (_Cache != null)
                    return _Cache.Count();
                return 0;
            }
        }

        public List<ContentStatistics> List
        {
            get
            {
                List<ContentStatistics> list = new List<ContentStatistics>();

                foreach (KeyValuePair<string, ContentStatistics> kvp in _Cache)
                    list.Add(kvp.Value);

                return list;
            }
            set
            {
                _Cache.Clear();

                if (value != null)
                    AddList(value);
            }
        }

        public ContentStatistics GetAny()
        {
            if (_Cache.Count() != 0)
                return _Cache.First().Value;

            return null;
        }

        public List<ContentStatistics> GetModifiedList(bool touchAndClearModified)
        {
            List<ContentStatistics> list = new List<ContentStatistics>();

            foreach (KeyValuePair<string, ContentStatistics> kvp in _Cache)
            {
                ContentStatistics cs = kvp.Value;

                if (cs.Modified)
                {
                    if (touchAndClearModified)
                        cs.TouchAndClearModified();

                    list.Add(cs);
                }
            }

            return list;
        }

        public Dictionary<string, ContentStatistics> Map
        {
            get { return _Map; }
            set { _Map = value; }
        }

        public UserRecord UserRecord
        {
            get { return _UserRecord; }
            set { _UserRecord = value; }
        }

        public UserProfile UserProfile
        {
            get { return _UserProfile; }
            set { _UserProfile = value; }
        }


        public ContentStatistics Get(IBaseObjectKeyed sourceObject)
        {
            string key = ContentStatistics.ComposeKey(sourceObject, _UserRecord, _UserProfile);

            if (String.IsNullOrEmpty(key))
                return null;

            ContentStatistics stats;

            if (_Cache.TryGetValue(key, out stats))
                return stats;

            return null;
        }

        public ContentStatistics GetRoot()
        {
            string key = ContentStatistics.ComposeRootKey(_UserRecord, _UserProfile);

            if (String.IsNullOrEmpty(key))
                return null;

            ContentStatistics stats;

            if (_Cache.TryGetValue(key, out stats))
                return stats;

            return null;
        }

        public void Add(ContentStatistics stats)
        {
            if (stats == null)
                return;

            string key = stats.KeyString;
            ContentStatistics value;

            if (!_Cache.TryGetValue(key, out value))
                _Cache.Add(key, stats);
        }

        public void Replace(ContentStatistics stats)
        {
            if (stats == null)
                return;

            string key = stats.KeyString;
            ContentStatistics value;

            if (!_Cache.TryGetValue(key, out value))
                _Cache.Add(key, stats);
            else
                _Cache[key] = stats;
        }

        public void AddList(List<ContentStatistics> statsList)
        {
            if (statsList == null)
                return;

            foreach (ContentStatistics stats in statsList)
                _Cache.Add(stats.KeyString, stats);
        }

        public void AddHierarchy(ContentStatistics stats)
        {
            if (stats == null)
                return;

            string key = stats.KeyString;
            ContentStatistics value;

            if (!_Cache.TryGetValue(key, out value))
                _Cache.Add(key, stats);

            if (stats.HasChildren())
            {
                foreach (ContentStatistics cs in stats.Children)
                    AddHierarchy(cs);
            }
        }

        public ContentStatistics GetMapped(string key)
        {
            if (String.IsNullOrEmpty(key))
                return null;

            ContentStatistics stats;

            if (_Map.TryGetValue(key, out stats))
                return stats;

            return null;
        }

        public void AddMapped(string key, ContentStatistics stats)
        {
            ContentStatistics value;

            if (!_Map.TryGetValue(key, out value))
                _Map.Add(key, stats);
        }

        public void ClearMap()
        {
            _Map.Clear();
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            foreach (KeyValuePair<string, ContentStatistics> kvp in _Cache)
                element.Add(kvp.Value.GetElement("CS"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "CS":
                    {
                        ContentStatistics cs = new ContentStatistics(childElement);
                        _Cache.Add(cs.KeyString, cs);
                    }
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}
