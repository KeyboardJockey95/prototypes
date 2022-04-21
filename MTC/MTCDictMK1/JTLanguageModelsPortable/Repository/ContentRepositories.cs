using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;

namespace JTLanguageModelsPortable.Repository
{
    public class ContentStudyListRepository : BaseRepository<ContentStudyList>
    {
        public ContentStudyListRepository(IObjectStore objectStore) : base(objectStore) { }
        public ContentStudyListRepository() : base(new ObjectStore()) { }
        public ContentStudyList GetGuid(Guid guid)
        {
            List<ContentStudyList> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            ContentStudyList obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }

    public class ContentMediaItemRepository : BaseRepository<ContentMediaItem>
    {
        public ContentMediaItemRepository(IObjectStore objectStore) : base(objectStore) { }
        public ContentMediaItemRepository() : base(new ObjectStore()) { }
        public ContentMediaItem GetGuid(Guid guid)
        {
            List<ContentMediaItem> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            ContentMediaItem obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }

    public class ContentDocumentItemRepository : BaseRepository<ContentDocumentItem>
    {
        public ContentDocumentItemRepository(IObjectStore objectStore) : base(objectStore) { }
        public ContentDocumentItemRepository() : base(new ObjectStore()) { }
        public ContentDocumentItem GetGuid(Guid guid)
        {
            List<ContentDocumentItem> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            ContentDocumentItem obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }

    public class SandboxRepository : BaseRepository<Sandbox>
    {
        public SandboxRepository(IObjectStore objectStore) : base(objectStore) { }
        public SandboxRepository() : base(new ObjectStore()) { }
        public Sandbox GetGuid(Guid guid)
        {
            List<Sandbox> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            Sandbox obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }

    public class UserRunItemRepository : LanguageBaseRepository<UserRunItem>
    {
        public UserRunItemRepository(ILanguageObjectStore objectStore) : base(objectStore) { }

        public bool GetAllUserRunItems(
            string owner,
            string profile,
            LanguageID languageID,
            List<LanguageDescriptor> languageDescriptors,
            bool showNew,
            bool showActive,
            bool showLearned,
            List<string> sortOrder,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            int pageNumber,
            int pageSize,
            out int pageCount,
            out int totalCount)
        {
            string key;
            string canonicalPattern;
            MatchCode matchType = MatchCode.StartsWith;
            UserRunItem testItem;
            List<UserRunItem> userRunItems;
            bool returnValue = true;

            key = UserRunItem.ComposeKey(owner, profile, String.Empty);

            StringMatcher matcher = new StringMatcher(
                key, "Key", matchType, true, null, pageNumber, pageSize);

            if (pageSize != 0)
            {
                totalCount = QueryCount(matcher, languageID);
                pageCount = totalCount / pageSize;

                if ((totalCount % pageSize) != 0)
                    pageCount++;
            }
            else
            {
                totalCount = 0;
                pageCount = 0;
            }

            userRunItems = Query(matcher, languageID);

            if (userRunItems != null)
            {
                NodeUtilities.FilterUserRunItems(userRunItems, userRunItemDictionary, showNew, showActive, showLearned);

                if (sortOrder != null)
                    NodeUtilities.SortUserRunItems(userRunItems, sortOrder, languageDescriptors);

                foreach (UserRunItem userRunItem in userRunItems)
                {
                    canonicalPattern = userRunItem.TextLower;

                    if (!userRunItemDictionary.TryGetValue(canonicalPattern, out testItem))
                    {
                        userRunItemDictionary.Add(canonicalPattern, userRunItem);
                        userRunItemList.Add(userRunItem);
                    }
                }
            }
            else
                returnValue = false;

            return returnValue;
        }

        public bool GetUserRunItems(
            List<string> patterns,
            string owner,
            string profile,
            LanguageID languageID,
            List<LanguageDescriptor> languageDescriptors,
            bool showNew,
            bool showActive,
            bool showLearned,
            List<string> sortOrder,
            Dictionary<string, UserRunItem> userRunItemDictionary,
            List<UserRunItem> userRunItemList,
            int pageNumber,
            int pageSize,
            out int pageCount,
            out int totalCount)
        {
            int count = patterns.Count();
            List<string> keys = new List<string>(count);
            int index;
            string pattern;
            string canonicalPattern;
            string key;
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            MatchCode matchType = MatchCode.Exact;
            MatchCode matchTypeOut = MatchCode.Exact;
            List<MatchCode> matchTypes = new List<MatchCode>(count);
            bool sameMatchCode = true;
            UserRunItem testItem;
            List<UserRunItem> userRunItems;
            bool returnValue = true;

            totalCount = 0;
            pageSize = 0;

            for (index = 0; index < count; index++)
            {
                pattern = patterns[index];

                if (!canonical.Canonical(out matchTypeOut, out canonicalPattern, matchType, pattern))
                    canonicalPattern = pattern;

                key = UserRunItem.ComposeKey(owner, profile, canonicalPattern);
                keys.Add(key);

                if (matchType != matchTypeOut)
                    sameMatchCode = false;

                matchTypes.Add(matchTypeOut);
            }

            if (sameMatchCode)
            {
                StringMatcher matcher = new StringMatcher(
                    keys, "Key", matchType, true, null, pageNumber, pageSize);

                if (pageSize != 0)
                {
                    int countOne = QueryCount(matcher, languageID);

                    if (countOne != -1)
                        totalCount += countOne;
                }

                userRunItems = Query(matcher, languageID);

                NodeUtilities.FilterUserRunItems(userRunItems, userRunItemDictionary, showNew, showActive, showLearned);

                if (userRunItems != null)
                {
                    foreach (UserRunItem userRunItem in userRunItems)
                    {
                        canonicalPattern = userRunItem.TextLower;

                        if (!userRunItemDictionary.TryGetValue(canonicalPattern, out testItem))
                        {
                            userRunItemDictionary.Add(canonicalPattern, userRunItem);
                            userRunItemList.Add(userRunItem);
                        }
                    }
                }
            }
            else
            {
                for (index = 0; index < count; index++)
                {
                    key = keys[index];
                    matchType = matchTypes[index];
                    StringMatcher matcher = new StringMatcher(
                        key, "Key", matchType, true, null, pageNumber, pageSize);

                    if (pageSize != 0)
                        totalCount = QueryCount(matcher, languageID);

                    userRunItems = Query(matcher, languageID);

                    if (userRunItems != null)
                    {
                        NodeUtilities.FilterUserRunItems(userRunItems, userRunItemDictionary, showNew, showActive, showLearned);

                        foreach (UserRunItem userRunItem in userRunItems)
                        {
                            canonicalPattern = userRunItem.TextLower;

                            if (!userRunItemDictionary.TryGetValue(canonicalPattern, out testItem))
                            {
                                userRunItemDictionary.Add(canonicalPattern, userRunItem);
                                userRunItemList.Add(userRunItem);
                            }
                        }
                    }
                }
            }

            if (pageSize != 0)
            {
                pageCount = totalCount / pageSize;

                if ((totalCount % pageSize) != 0)
                    pageCount++;
            }
            else
            {
                totalCount = 0;
                pageCount = 0;
            }

            if (sortOrder != null)
                NodeUtilities.SortUserRunItems(userRunItemList, sortOrder, languageDescriptors);

            return returnValue;
        }

        public UserRunItem GetUserRunItem(
            string pattern,
            string owner,
            string profile,
            LanguageID languageID)
        {
            List<UserRunItem> entries = Lookup(pattern, owner, profile, MatchCode.Exact, languageID, 0, 0);
            UserRunItem returnValue = null;

            if (entries != null)
                returnValue = entries.FirstOrDefault();

            return returnValue;
        }

        public List<UserRunItem> Lookup(
            string pattern,
            string owner,
            string profile,
            MatchCode matchType,
            LanguageID languageID,
            int page,
            int pageSize)
        {
            ConvertCanonical canonical = new ConvertCanonical(languageID, true);
            string canonicalPattern;
            pattern = pattern.ToLower();
            if (!canonical.Canonical(out matchType, out canonicalPattern, matchType, pattern))
                canonicalPattern = pattern;
            StringMatcher matcher = new StringMatcher(UserRunItem.ComposeKey(owner, profile, canonicalPattern), "Key", matchType, true, null, page, pageSize);
            List<UserRunItem> returnValue = Query(matcher, languageID);
            return returnValue;
        }

        public bool DeleteAllUserRunItems(
            string owner,
            string profile,
            LanguageID languageID,
            out int totalCount,
            out string errorMessage)
        {
            string key;
            MatchCode matchType = MatchCode.StartsWith;
            List<UserRunItem> userRunItems;
            bool returnValue = true;

            totalCount = 0;
            errorMessage = null;

            key = UserRunItem.ComposeKey(owner, profile, String.Empty);

            StringMatcher matcher = new StringMatcher(
                key, "Key", matchType, true, null, 0, 0);

            userRunItems = Query(matcher, languageID);

            if ((userRunItems != null) && (userRunItems.Count() != 0))
            {
                totalCount = userRunItems.Count();
                List<object> keyList = new List<object>(totalCount);

                foreach (UserRunItem userRunItem in userRunItems)
                    keyList.Add(UserRunItem.ComposeKey(owner, profile, userRunItem.TextLower));

                if (!DeleteKeyList(keyList, languageID))
                {
                    errorMessage = "Error deleting vocabulary.";
                    returnValue = false;
                }
            }
            else
            {
                errorMessage = "No vocabulary to delete.";
                returnValue = false;
            }

            return returnValue;
        }
    }

    public class ContentStatisticsRepository : BaseRepository<ContentStatistics>
    {
        public ContentStatisticsRepository(IObjectStore objectStore) : base(objectStore) { }
    }
}
