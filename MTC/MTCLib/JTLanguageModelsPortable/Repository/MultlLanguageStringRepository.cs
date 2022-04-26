using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Repository
{
    public class MultiLanguageStringRepository
    {
        public LanguageBaseStringRepository Repository;
        public List<LanguageID> LanguageIDs;

        public MultiLanguageStringRepository(LanguageBaseStringRepository languageBaseStringRepository, List<LanguageID> languageIDs)
        {
            Repository = languageBaseStringRepository;
            LanguageIDs = languageIDs;
        }

        public void Clear()
        {
            if (Repository != null)
                Repository.Clear();
            if (LanguageIDs != null)
                LanguageIDs.Clear();
        }

        public MultiLanguageString Get(object key)
        {
            MultiLanguageString mls = new MultiLanguageString(key);
            foreach (LanguageID languageID in LanguageIDs)
            {
                BaseString bs = Repository.Get(key, languageID);
                if (bs != null)
                {
                    LanguageString ls = new LanguageString(key, languageID, bs.Text);
                    mls.Add(ls);
                }
                else
                {
                    LanguageString ls = new LanguageString(key, languageID, String.Empty);
                    mls.Add(ls);
                }
            }
            return mls;
        }

        public List<MultiLanguageString> GetList(List<object> keys)
        {
            List<MultiLanguageString> mlsList = new List<MultiLanguageString>();
            if (keys != null)
            {
                foreach (object key in keys)
                {
                    MultiLanguageString mls = Get(key);

                    if (mls != null)
                        mlsList.Add(mls);
                }
            }
            return mlsList;
        }

        public bool GetList(List<object> keys, List<MultiLanguageString> mlsList)
        {
            bool returnValue = true;

            if (keys != null)
            {
                foreach (object key in keys)
                {
                    MultiLanguageString mls = Get(key);

                    if (mls != null)
                        mlsList.Add(mls);
                    else
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public List<MultiLanguageString> GetAll()
        {
            List<MultiLanguageString> mlsList = new List<MultiLanguageString>();
            Dictionary<string, MultiLanguageString> mlsDictionary = new Dictionary<string, MultiLanguageString>();

            foreach (LanguageID languageID in LanguageIDs)
            {
                List<BaseString> bsList = Repository.GetAll(languageID);

                if (bsList != null)
                {
                    foreach (BaseString bs in bsList)
                    {
                        MultiLanguageString mls;
                        string key = bs.KeyString;
                        LanguageString ls = new LanguageString(key, languageID, bs.Text);

                        if (mlsDictionary.TryGetValue(key, out mls))
                            mls.Add(ls);
                        else
                        {
                            mls = new MultiLanguageString(key, ls);
                            mlsList.Add(mls);
                            mlsDictionary.Add(key, mls);
                        }
                    }
                }
            }

            return mlsList;
        }

        public List<MultiLanguageString> Query(LanguageStringMatcher matcher)
        {
            List<MultiLanguageString> mlsList = new List<MultiLanguageString>();
            List<BaseString> bsList;
            MultiLanguageString mls;
            Matcher keyMatcher = matcher.KeyMatcher;
            foreach (LanguageID languageID in LanguageIDs)
            {
                bsList = Repository.Query(keyMatcher, languageID);
                if (bsList != null)
                {
                    foreach (BaseString bs in bsList)
                    {
                        string keyString = bs.KeyString;
                        mls = mlsList.FirstOrDefault(x => x.KeyString == keyString);
                        if (mls != null)
                            mls.SetText(languageID, bs.Text);
                        else
                            mlsList.Add(new MultiLanguageString(bs.Key, languageID, bs.Text, LanguageIDs, String.Empty));
                    }
                }
            }
            if (keyMatcher.PageSize > 0)
                return mlsList.Take(keyMatcher.PageSize).ToList();
            return mlsList;
        }

        public bool Contains(object key)
        {
            foreach (LanguageID languageID in LanguageIDs)
            {
                if (Repository.Contains(key, languageID))
                    return true;
            }
            return false;
        }

        public bool Contains(LanguageStringMatcher matcher)
        {
            Matcher keyMatcher = matcher.KeyMatcher;
            foreach (LanguageID languageID in LanguageIDs)
            {
                if (Repository.Contains(keyMatcher, languageID))
                    return true;
            }
            return false;
        }

        public bool Add(MultiLanguageString item)
        {
            if (item.LanguageStrings == null)
                return false;
            bool returnValue = true;
            foreach (LanguageString ls in item.LanguageStrings)
            {
                if (!Repository.Add(new BaseString(ls.Key, ls.Text), ls.LanguageID))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool AddList(List<MultiLanguageString> items)
        {
            if (items == null)
                return false;
            bool returnValue = true;
            foreach (MultiLanguageString mls in items)
            {
                if (!Add(mls))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool Update(MultiLanguageString item)
        {
            if (item.LanguageStrings == null)
                return false;
            bool returnValue = true;
            foreach (LanguageString ls in item.LanguageStrings)
            {
                if (!Repository.Update(new BaseString(ls.Key, ls.Text), ls.LanguageID))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool UpdateList(List<MultiLanguageString> items)
        {
            if (items == null)
                return false;
            bool returnValue = true;
            foreach (MultiLanguageString mls in items)
            {
                if (!Update(mls))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool Delete(MultiLanguageString item)
        {
            if (item.LanguageStrings == null)
                return false;
            bool returnValue = true;
            foreach (LanguageString ls in item.LanguageStrings)
            {
                if (!Repository.Delete(new BaseString(ls.Key, ls.Text), ls.LanguageID))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteList(List<MultiLanguageString> items)
        {
            if (items == null)
                return false;
            bool returnValue = true;
            foreach (MultiLanguageString mls in items)
            {
                if (!Delete(mls))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteList(List<object> keys)
        {
            if (keys == null)
                return false;
            bool returnValue = true;
            foreach (object key in keys)
            {
                if (!DeleteKey(key))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteKey(object key)
        {
            bool returnValue = true;
            foreach (LanguageID languageID in LanguageIDs)
            {
                if (!Repository.DeleteKey(key, languageID))
                    returnValue = false;
            }
            return returnValue;
        }

        public bool DeleteAll()
        {
            bool returnValue = true;
            foreach (LanguageID languageID in LanguageIDs)
                Repository.DeleteAll(languageID);
            return returnValue;
        }

        public int Count()
        {
            return Repository.Count(LanguageIDs[0]);
        }
    }
}
