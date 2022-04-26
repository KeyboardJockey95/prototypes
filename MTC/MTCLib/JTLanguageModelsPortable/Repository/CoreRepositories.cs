using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;

namespace JTLanguageModelsPortable.Repository
{
    public class LanguageDescriptionRepository : BaseRepository<LanguageDescription>
    {
        public LanguageDescriptionRepository(IObjectStore objectStore) : base(objectStore) { }
    }

    public class BaseStringRepository : BaseRepository<BaseString>
    {
        public BaseStringRepository(IObjectStore objectStore) : base(objectStore) {}
    }

    public class LanguageStringRepository : BaseRepository<LanguageString>
    {
        public LanguageStringRepository(IObjectStore objectStore) : base(objectStore) { }
    }

    public class LanguageBaseStringRepository : LanguageBaseRepository<BaseString>
    {
        public LanguageBaseStringRepository(ILanguageObjectStore objectStore) : base(objectStore) { }
    }

    public class LanguagePairBaseStringRepository : LanguagePairBaseRepository<BaseString>
    {
        public LanguagePairBaseStringRepository(ILanguagePairObjectStore objectStore) : base(objectStore) { }
    }

    public class LanguagePairBaseStringsRepository : LanguagePairBaseRepository<BaseStrings>
    {
        public LanguagePairBaseStringsRepository(ILanguagePairObjectStore objectStore) : base(objectStore) { }
    }

    public class TitledBaseRepository : BaseRepository<BaseObjectTitled>
    {
        public TitledBaseRepository(IObjectStore objectStore) : base(objectStore) { }
    }

    public class TitledReferenceRepository : BaseRepository<ObjectReferenceTitled>
    {
        public TitledReferenceRepository(IObjectStore objectStore) : base(objectStore) { }
        public ObjectReferenceTitled GetNamed(string userName, string name)
        {
            List<ObjectReferenceTitled> list = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            ObjectReferenceTitled obj = null;
            if (list != null)
                obj = list.FirstOrDefault(x => (x.Title != null) && (x.Title.Count() != 0) && (x.Title.AnyTextMatchesExact(name)));
            return obj;
        }
        public ObjectReferenceTitled GetGuid(Guid guid)
        {
            List<ObjectReferenceTitled> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            ObjectReferenceTitled obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }

    public class NodeTreeRepository : BaseRepository<BaseObjectNodeTree>
    {
        public NodeTreeRepository(IObjectStore objectStore) : base(objectStore) { }
        public NodeTreeRepository() : base(new ObjectStore()) { }
        public BaseObjectNodeTree GetNamed(string userName, string name)
        {
            List<BaseObjectNodeTree> list = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            BaseObjectNodeTree obj = null;
            if (list != null)
                obj = list.FirstOrDefault(x => (x.Title != null) && (x.Title.Count() != 0) && (x.Title.AnyTextMatchesExact(name)));
            return obj;
        }
        public BaseObjectNodeTree GetGuid(Guid guid)
        {
            List<BaseObjectNodeTree> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            BaseObjectNodeTree obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }

    public class NodeTreeReferenceRepository : BaseRepository<ObjectReferenceNodeTree>
    {
        public NodeTreeReferenceRepository(IObjectStore objectStore) : base(objectStore) { }
        public ObjectReferenceNodeTree GetNamed(string userName, string name)
        {
            List<ObjectReferenceNodeTree> list = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            ObjectReferenceNodeTree obj = null;
            if (list != null)
                obj = list.FirstOrDefault(x => (x.Title != null) && (x.Title.Count() != 0) && (x.Title.AnyTextMatchesExact(name)));
            return obj;
        }
        public ObjectReferenceNodeTree GetGuid(Guid guid)
        {
            List<ObjectReferenceNodeTree> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            ObjectReferenceNodeTree obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }
}
