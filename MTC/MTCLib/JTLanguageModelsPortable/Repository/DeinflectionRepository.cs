using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Language;

namespace JTLanguageModelsPortable.Repository
{
    public class DeinflectionRepository : LanguageBaseRepository<Deinflection>
    {
        public DeinflectionRepository(ILanguageObjectStore objectStore) : base(objectStore) { }

        public DeinflectionRepository(ILanguageObjectStore objectStore, XElement element) : base(objectStore, element) { }

        public List<Deinflection> Lookup(string pattern, MatchCode matchType, LanguageID languageID, int page, int pageSize)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, true, null, page, pageSize);
            List<Deinflection> returnValue = Query(matcher, languageID);
            return returnValue;
        }

        // Note: Actual page count will be the union of individual language queries.
        public List<Deinflection> Lookup(string pattern, MatchCode matchType, List<LanguageID> languageIDs, int page, int pageSize)
        {
            List<Deinflection> returnValue = new List<Deinflection>();
            List<Deinflection> lookup;

            foreach (LanguageID languageID in languageIDs)
            {
                lookup = Lookup(pattern, matchType, languageID, page, pageSize);

                if ((lookup != null) && (lookup.Count() != 0))
                    returnValue.AddRange(lookup);
            }

            return returnValue;
        }
    }
}
