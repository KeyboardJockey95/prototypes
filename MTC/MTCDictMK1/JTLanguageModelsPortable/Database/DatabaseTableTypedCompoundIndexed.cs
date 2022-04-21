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
    public class DatabaseTableTypedCompoundIndexed<KeyType, IndexKeyType, ItemType> : DatabaseTableTyped<KeyType, ItemType>
        where ItemType : DatabaseItem, new()
        where IndexKeyType : IBaseObject, new()
    {
        public DatabaseTableTypedCompoundIndexed(string name, string databaseDirectory, bool generateKeys,
                LanguageID languageID, bool caseInsensitive)
            : base(name, databaseDirectory, generateKeys, languageID, caseInsensitive)
        {
        }

        public DatabaseTableTypedCompoundIndexed()
        {
        }

        public override DatabaseMatcher CreateDatabaseMatcher(Matcher subMatcher)
        {
            return new DatabaseCompoundMatcher<IndexKeyType>(this, subMatcher);
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
                var items = _DB.Table<ItemType>().IndexQuery<byte[]>("Index").Where(i => dbMatcher.Match(i)).ToLazyList();
                if (items.Count() != 0)
                    returnValue = true;
            }

            return returnValue;
        }

        public override IBaseObject GetFirst(Matcher matcher)
        {
            if (matcher == null)
                return null;

            IBaseObject obj = null;

            if (matcher.MemberName == "Key")
            {
                var lazyItems = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToLazyList();

                if (lazyItems.Count() != 0)
                    obj = lazyItems.First().Value.Get();
            }
            else
            {
                DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                var lazyItems = _DB.Table<ItemType>().IndexQuery<byte[]>("Index").Where(i => dbMatcher.Match(i)).ToLazyList();

                if (lazyItems.Count() != 0)
                    obj = lazyItems.First().Value.Get();
            }

            return obj;
        }

        public override List<object> QueryKeys(Matcher matcher)
        {
            if (matcher == null)
                return null;

            List<ItemType> items = null;

            if (matcher.MemberName == "Key")
                items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToList();
            else
            {
                DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                items = _DB.Table<ItemType>().IndexQuery<byte[]>("Index").Where(i => dbMatcher.Match(i)).ToList();
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

            if (matcher.MemberName == "Key")
                items = _DB.Table<ItemType>().Query<KeyType>().Where(i => matcher.Match(i)).ToList();
            else
            {
                DatabaseMatcher dbMatcher = CreateDatabaseMatcher(matcher);
                items = _DB.Table<ItemType>().IndexQuery<byte[]>("Index").Where(i => dbMatcher.Match(i)).ToList();
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
                items = _DB.Table<ItemType>().IndexQuery<byte[]>("Index").Where(i => dbMatcher.Match(i)).ToList();
            }
            if (items != null)
                count = items.Count();
            return count;
        }
    }
}
