using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Repository
{
    public class ToolProfileRepository : BaseRepository<ToolProfile>
    {
        public ToolProfileRepository(IObjectStore objectStore) : base(objectStore) { }

        public List<ToolProfile> GetList(UserRecord userRecord)
        {
            string pattern = ToolUtilities.ComposeToolProfileKey(userRecord, String.Empty);
            List<ToolProfile> list = Query(new StringMatcher(pattern, "Key", MatchCode.StartsWith, 0, 0));
            return list;
        }

        public ToolProfile GetNamed(UserRecord userRecord, string name)
        {
            string key = ToolUtilities.ComposeToolProfileKey(userRecord, name);
            ToolProfile obj = Get(key);
            return obj;
        }
    }

    public class ToolStudyListRepository : BaseRepository<ToolStudyList>
    {
        public ToolStudyListRepository(IObjectStore objectStore) : base(objectStore) { }
    }

    public class ToolSessionRepository : BaseRepository<ToolSession>
    {
        public ToolSessionRepository(IObjectStore objectStore) : base(objectStore) { }
    }
}
