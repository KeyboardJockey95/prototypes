using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Media;
using Lex.Db;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseTableTyped<KeyType, ItemType> : DatabaseTable
        where ItemType : DatabaseItem, new()
    {
        public DatabaseTableTyped(string name, string databaseDirectory, bool generateKeys,
                LanguageID languageID, bool caseInsensitive)
            : base(name, databaseDirectory, generateKeys, languageID, caseInsensitive)
        {
        }

        public DatabaseTableTyped()
        {
        }

        public override void Map()
        {
            ItemType item = new ItemType();
            item.Map(_DB, GenerateKeys, LanguageID, CaseInsensitive);
        }

        public override void Initialize()
        {
            base.Initialize();

            // This is to have the table create its file, if not already created.
            /*
            try
            {
                Get(default(KeyType));
            }
            catch (Exception)
            {
            }
            */
        }

        public override bool ContainsKey(object key)
        {
            bool returnValue = false;

            var items = _DB.Table<ItemType>().Query<KeyType>().Key((KeyType)key).ToLazyList();

            if (items.Count() != 0)
                returnValue = true;

            return returnValue;
        }

        public override bool Contains(Matcher matcher)
        {
            if (matcher == null)
                return false;

            bool returnValue = false;

            if (matcher.MemberName == "Key")
            {
                var items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToLazyList();
                if (items.Count() != 0)
                    returnValue = true;
            }
            else
            {
                DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                var items = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i)).ToLazyList();
                if (items.Count() != 0)
                    returnValue = true;
            }

            return returnValue;
        }

        public override List<object> GetKeys()
        {
            List<object> keys = _DB.AllKeys<ItemType, KeyType>().Cast<object>().ToList();
            return keys;
        }

        public override IBaseObject Get(object key)
        {
            ItemType item = _DB.LoadByKey<ItemType, KeyType>((KeyType)key);
            IBaseObject obj = null;

            if (item != null)
            {
                try
                {
                    obj = item.Get();
                }
                catch (Exception exc)
                {
                    if (exc is OutOfMemoryException)
                    {
#if DEBUG
                        //DeleteKey(key);
                        throw;
#endif
                    }
                }
            }

            return obj;
        }

        public override IBaseObject GetFirst(Matcher matcher)
        {
            if (matcher == null)
                return null;

            IBaseObject obj = null;

            if (matcher.PageSize > 0)
            {
                if (matcher.MemberName == "Key")
                {
                    var lazyItems = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i))
                        .Skip((matcher.Page - 1) * matcher.PageSize).Take(matcher.PageSize).ToLazyList();

                    if (lazyItems.Count() != 0)
                        obj = lazyItems.First().Value.Get();
                }
                else
                {
                    DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                    var lazyItems = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i))
                        .Skip((matcher.Page - 1) * matcher.PageSize).Take(matcher.PageSize).ToLazyList();

                    if (lazyItems.Count() != 0)
                        obj = lazyItems.First().Value.Get();
                }
            }
            else
            {
                if (matcher.MemberName == "Key")
                {
                    var lazyItems = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToLazyList();

                    if (lazyItems.Count() != 0)
                        obj = lazyItems.First().Value.Get();
                }
                else
                {
                    DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                    var lazyItems = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i)).ToLazyList();

                    if (lazyItems.Count() != 0)
                        obj = lazyItems.First().Value.Get();
                }
            }

            return obj;
        }

        public override IBaseObject GetIndexed(int index)
        {
            ItemType item = _DB.Table<ItemType>().ElementAt(index);
            IBaseObject obj = null;

            if (item != null)
                obj = item.Get();

            return obj;
        }

        public override List<object> QueryKeys(Matcher matcher)
        {
            if (matcher == null)
                return null;

            List<ItemType> items = null;

            if (matcher.PageSize > 0)
            {
                if (matcher.MemberName == "Key")
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i))
                        .Skip((matcher.Page - 1) * matcher.PageSize).Take(matcher.PageSize).ToList();
                else
                {
                    DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i))
                        .Skip((dbMatcher.Page - 1) * dbMatcher.PageSize).Take(dbMatcher.PageSize).ToList();
                }
            }
            else
            {
                if (matcher.MemberName == "Key")
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToList();
                else
                {
                    DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i)).ToList();
                }
            }

            List<object> objs = new List<object>(items.Count());

            foreach (ItemType item in items)
                objs.Add(item.GetKey());

            return objs;
        }

        public override List<IBaseObject> Query(Matcher matcher)
        {
            if (matcher == null)
                return null;

            List<ItemType> items = null;

            if (matcher.PageSize > 0)
            {
                if (matcher.MemberName == "Key")
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i))
                        .Skip((matcher.Page - 1) * matcher.PageSize).Take(matcher.PageSize).ToList();
                else
                {
                    DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i))
                        .Skip((dbMatcher.Page - 1) * dbMatcher.PageSize).Take(dbMatcher.PageSize).ToList();
                }
            }
            else
            {
                if (matcher.MemberName == "Key")
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToList();
                else
                {
                    DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                    items = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i)).ToList();
                }
            }

            List<IBaseObject> objs = new List<IBaseObject>(items.Count());

            foreach (ItemType item in items)
                objs.Add(item.Get());

            return objs;
        }

        public override int QueryCount(Matcher matcher)
        {
            int count = 0;
            List<ItemType> items = null;
            if (matcher.MemberName == "Key")
                items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToList();
            else
            {
                DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                items = _DB.Table<ItemType>().Query<KeyType>().Where(i => dbMatcher.Match(i)).ToList();
            }
            if (items != null)
                count = items.Count();
            return count;
        }

        public override List<IBaseObject> GetAll()
        {
            List<ItemType> items = _DB.LoadAll<ItemType>().ToList();
            List<IBaseObject> objs = new List<IBaseObject>(items.Count());
            foreach (ItemType item in items)
                objs.Add(item.Get());
            return objs;
        }

        public override bool Add(object key, IBaseObject obj)
        {
            ItemType item = new ItemType();
            item.Set((KeyType)key, obj);
            _DB.Save(item);
            return true;
        }

        public override bool Add(IBaseObjectKeyed obj)
        {
            ItemType item = new ItemType();
            item.Set((KeyType)obj.Key, obj);
            _DB.Save(item);
            if (GenerateKeys)
            {
                obj.SetKeyNoModify(item.GetKey());
                item.Set((KeyType)obj.Key, obj);
                _DB.Save(item);     // Save it again to save the key.
            }
            return true;
        }

        public override bool AddList(List<IBaseObjectKeyed> objs)
        {
            bool returnValue = true;

            if (objs == null)
                return false;

            int count = objs.Count();
            int index;

            if (count == 0)
                return true;

            ItemType[] dbObjs = new ItemType[count];
            IBaseObjectKeyed obj;
            ItemType item;

            for (index = 0; index < count; index++)
            {
                obj = objs[index];
                item = new ItemType();
                item.Set((KeyType)obj.Key, obj);
                dbObjs[index] = item;
            }

            _DB.Save(dbObjs);

            if (GenerateKeys)
            {
                for (index = 0; index < count; index++)
                {
                    obj = objs[index];
                    item = dbObjs[index];
                    obj.SetKeyNoModify(item.GetKey());
                    item.Set((KeyType)obj.Key, obj);
                }

                _DB.Save(dbObjs);     // Save it again to save the key.
            }

            /*
            foreach (IBaseObjectKeyed obj in objs)
            {
                if (!Add(obj))
                    returnValue = false;
            }
            */

            return returnValue;
        }

        public override bool Update(object key, IBaseObject obj)
        {
            ItemType item = new ItemType();
            item.Set((KeyType)key, obj);
            _DB.Save(item);
            return true;
        }

        public override bool Update(IBaseObjectKeyed obj)
        {
            ItemType item = new ItemType();
            item.Set((KeyType)obj.Key, obj);
            _DB.Save(item);
            return true;
        }

        public override bool UpdateList(List<IBaseObjectKeyed> objs)
        {
            bool returnValue = true;

            if (objs == null)
                return false;

            int count = objs.Count();
            int index;

            if (count == 0)
                return true;

            ItemType[] dbObjs = new ItemType[count];
            IBaseObjectKeyed obj;
            ItemType item;

            for (index = 0; index < count; index++)
            {
                obj = objs[index];
                item = new ItemType();
                item.Set((KeyType)obj.Key, obj);
                dbObjs[index] = item;
            }

            _DB.Save(dbObjs);

            return returnValue;
        }

        public override bool DeleteKey(object key)
        {
            bool returnValue = _DB.DeleteByKey<ItemType>((KeyType)key);
            return returnValue;
        }

        public override bool Delete(IBaseObjectKeyed obj)
        {
            bool returnValue = _DB.DeleteByKey<ItemType>((KeyType)obj.Key);
            return returnValue;
        }

        public override bool DeleteKeyList(List<object> keys)
        {
            int count = _DB.DeleteByKeys<ItemType>(keys);
            bool returnValue = true;
            if (count != keys.Count())
                returnValue = false;
            return returnValue;
        }

        public override int DeleteQuery(Matcher matcher)
        {
            int count = 0;

            if ((matcher is StringMatcher) && (matcher.MemberName == "Key") && (((StringMatcher)matcher).Patterns != null) &&
                    (((StringMatcher)matcher).Patterns.Count() >= 1))
            {
                List<string> patterns = ((StringMatcher)matcher).Patterns;
                List<object> keys = _DB.AllKeys<ItemType, KeyType>().Cast<object>().ToList();
                List<object> deleteKeys = new List<object>();

                foreach (string pattern in patterns)
                {
                    foreach (object key in keys)
                    {
                        bool badPattern;

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

        public override bool DeleteList(List<IBaseObjectKeyed> objs)
        {
            bool returnValue = true;

            if (objs == null)
                return false;

            List<object> keys = new List<object>();

            foreach (IBaseObjectKeyed obj in objs)
                keys.Add(obj.Key);

            returnValue = DeleteKeyList(keys);

            return returnValue;
        }

        public override bool DeleteAll()
        {
            bool returnValue = true;
            _DB.Purge();
            return returnValue;
        }

        public override int Count()
        {
            int count = _DB.Count<ItemType>();
            return count;
        }
    }
}
