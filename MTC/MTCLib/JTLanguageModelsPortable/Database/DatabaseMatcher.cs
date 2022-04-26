using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Database
{
    public class DatabaseMatcher : Matcher
    {
        public DatabaseTable Table { get; set; }
        public Matcher SubMatcher { get; set; }

        public DatabaseMatcher(DatabaseTable table, Matcher subMatcher)
        {
            Table = table;
            SubMatcher = subMatcher;
        }

        public override bool Match(object obj)
        {
            IBaseObject target = Table.Get(obj);
            bool returnValue = false;
            if (target != null)
                returnValue = SubMatcher.Match(target);
            return returnValue;
        }
    }

    public class DatabaseFieldOnlyMatcher : DatabaseMatcher
    {
        public DatabaseFieldOnlyMatcher(DatabaseTable table, Matcher subMatcher)
            : base(table, subMatcher)
        {
        }

        public override bool Match(object obj)
        {
            bool returnValue = SubMatcher.Match(obj);
            return returnValue;
        }
    }

    public class DatabaseCompoundMatcher<IndexKeyType> : DatabaseMatcher
        where IndexKeyType : IBaseObject, new()
    {
        public DatabaseCompoundMatcher(DatabaseTable table, Matcher subMatcher)
            : base(table, subMatcher)
        {
        }

        public override bool Match(object obj)
        {
            IBaseObject indexKey = new IndexKeyType();
            indexKey.BinaryData = obj as byte[];
            bool returnValue = SubMatcher.Match(indexKey);
            return returnValue;
        }
    }
}
