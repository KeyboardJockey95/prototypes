using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Repository
{
    public class ChangeLogItemRepository : BaseRepository<ChangeLogItem>
    {
        public ChangeLogItemRepository(IObjectStore objectStore) : base(objectStore) { }

        public List<ChangeLogItem> GetChangeLogItems(
            string userName,
            string profileName,
            DateTime startTime,
            DateTime endTime,
            int treeKey,
            int nodeKey,
            string contentKey,
            int contentStorageKey,
            EditActionType actionType)
        {
            List<Matcher> matchers = new List<Matcher>();

            if (!String.IsNullOrEmpty(userName))
                matchers.Add(new StringMatcher(userName, "UserName", MatchCode.Exact, 0, 0));

            if (!String.IsNullOrEmpty(profileName))
                matchers.Add(new StringMatcher(profileName, "ProfileName", MatchCode.Exact, 0, 0));

            if ((startTime != DateTime.MinValue) && (endTime != DateTime.MaxValue))
                matchers.Add(new DateTimeMatcher(startTime, endTime, "LogTime", MatchCode.Between, 0, 0));

            if (treeKey != -1)
                matchers.Add(new IntMatcher(treeKey, "TreeKey", MatchCode.Exact, 0, 0));

            if (nodeKey != -1)
                matchers.Add(new IntMatcher(nodeKey, "NodeKey", MatchCode.Exact, 0, 0));

            if (!String.IsNullOrEmpty(contentKey))
                matchers.Add(new StringMatcher(contentKey, "ContentKey", MatchCode.Exact, 0, 0));

            if (contentStorageKey != -1)
                matchers.Add(new IntMatcher(contentStorageKey, "ContentStorageKey", MatchCode.Exact, 0, 0));

            if (actionType != EditActionType.None)
                matchers.Add(
                    new StringMatcher(
                        EditAction.GetEditActionTypeString(actionType),
                        "ActionType",
                        MatchCode.Exact,
                        0,
                        0));

            CompoundMatcher matcher = new CompoundMatcher(matchers, null, MatchCode.And, 0, 0);

            List<ChangeLogItem> changeLogItems;

            if (matchers.Count() != 0)
                changeLogItems = Query(matcher);
            else
                changeLogItems = GetAll();

            return changeLogItems;
        }
    }
}
