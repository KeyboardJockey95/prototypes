using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
//using JTLanguageModelsPortable.Formats;

namespace JTLanguageModelsPortable.Repository
{
    public class ImageRepository : BaseRepository<Image>
    {
        public ImageRepository(IObjectStore objectStore) : base(objectStore) { }

        public Image GetImage(string owner, string name)
        {
            StringMatcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, false, null, 0, 0);
            StringMatcher nameMatcher = new StringMatcher(name, "Name", MatchCode.Exact, false, null, 0, 0);
            CompoundMatcher matcher = new CompoundMatcher(new List<Matcher>() { ownerMatcher, nameMatcher }, null, MatchCode.And, 0, 0);
            Image image = GetFirst(matcher);
            return image;
        }

        public List<Image> Lookup(string pattern, MatchCode matchType, string owner, int page, int pageSize)
        {
            StringMatcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, false, null, 0, 0);
            StringMatcher nameMatcher = new StringMatcher(pattern, "Name", MatchCode.Exact, false, null, 0, 0);
            CompoundMatcher matcher = new CompoundMatcher(new List<Matcher>() { ownerMatcher, nameMatcher }, null, MatchCode.And, 0, 0);
            List<Image> returnValue = Query(matcher);
            return returnValue;
        }
    }

    public class AudioRepository : LanguageBaseRepository<AudioReference>
    {
        public AudioRepository(ILanguageObjectStore objectStore) : base(objectStore) { }

        public AudioReference GetAudio(string name, LanguageID languageID)
        {
            List<AudioReference> audios = Lookup(name, MatchCode.Exact, languageID, 0, 0);
            AudioReference returnValue = null;

            if (audios != null)
                returnValue = audios.FirstOrDefault();

            return returnValue;
        }

        public List<AudioReference> Lookup(string pattern, MatchCode matchType, LanguageID languageID, int page, int pageSize)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, true, null, page, pageSize);
            List<AudioReference> returnValue = Query(matcher, languageID);
            return returnValue;
        }

        // Note: Actual page count will be the union of individual language queries.
        public List<AudioReference> Lookup(string pattern, MatchCode matchType, List<LanguageID> languageIDs, int page, int pageSize)
        {
            List<AudioReference> returnValue = new List<AudioReference>();
            List<AudioReference> lookup;

            foreach (LanguageID languageID in languageIDs)
            {
                lookup = Lookup(pattern, matchType, languageID, page, pageSize);

                if ((lookup != null) && (lookup.Count() != 0))
                    returnValue.AddRange(lookup);
            }

            return returnValue;
        }
    }

    public class AudioMultiRepository : LanguageBaseRepository<AudioMultiReference>
    {
        public AudioMultiRepository(ILanguageObjectStore objectStore) : base(objectStore) { }

        public AudioMultiReference GetAudio(
            string name,
            LanguageID languageID)
        {
            List<AudioMultiReference> audios = Lookup(name, MatchCode.Exact, languageID, 0, 0);
            AudioMultiReference returnValue = null;

            if (audios != null)
                returnValue = audios.FirstOrDefault();

            return returnValue;
        }

        public List<AudioMultiReference> Lookup(
            string pattern,
            MatchCode matchType,
            LanguageID languageID,
            int page,
            int pageSize)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, true, null, page, pageSize);
            List<AudioMultiReference> returnValue = Query(matcher, languageID);
            return returnValue;
        }

        // Note: Actual page count will be the union of individual language queries.
        public List<AudioMultiReference> Lookup(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs,
            int page,
            int pageSize)
        {
            List<AudioMultiReference> returnValue = new List<AudioMultiReference>();
            List<AudioMultiReference> lookup;

            foreach (LanguageID languageID in languageIDs)
            {
                lookup = Lookup(pattern, matchType, languageID, page, pageSize);

                if ((lookup != null) && (lookup.Count() != 0))
                    returnValue.AddRange(lookup);
            }

            return returnValue;
        }

        public int TotalCount(
            string pattern,
            MatchCode matchType,
            LanguageID languageID)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, true, null, 0, 0);
            int returnValue = QueryCount(matcher, languageID);
            return returnValue;
        }

        public int TotalCount(
            string pattern,
            MatchCode matchType,
            List<LanguageID> languageIDs)
        {
            int returnValue = 0;

            foreach (LanguageID languageID in languageIDs)
            {
                int count = TotalCount(pattern, matchType, languageID);

                if (count >= 0)
                    returnValue += count;
                else
                    return -1;
            }

            return returnValue;
        }
    }

    public class PictureRepository : LanguageBaseRepository<PictureReference>
    {
        public PictureRepository(ILanguageObjectStore objectStore) : base(objectStore) { }

        public PictureReference GetPicture(string name, LanguageID languageID)
        {
            List<PictureReference> pictures = Lookup(name, MatchCode.Exact, languageID, 0, 0);
            PictureReference returnValue = null;

            if (pictures != null)
                returnValue = pictures.FirstOrDefault();

            return returnValue;
        }

        public List<PictureReference> Lookup(string pattern, MatchCode matchType, LanguageID languageID, int page, int pageSize)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(canonicalPattern, "Key", matchType, true, null, page, pageSize);
            List<PictureReference> returnValue = Query(matcher, languageID);
            return returnValue;
        }

        // Note: Actual page count will be the union of individual language queries.
        public List<PictureReference> Lookup(string pattern, MatchCode matchType, List<LanguageID> languageIDs, int page, int pageSize)
        {
            List<PictureReference> returnValue = new List<PictureReference>();
            List<PictureReference> lookup;

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
