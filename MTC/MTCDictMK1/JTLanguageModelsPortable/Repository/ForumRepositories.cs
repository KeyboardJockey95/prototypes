using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Forum;
using JTLanguageModelsPortable.Matchers;

namespace JTLanguageModelsPortable.Repository
{
    public class ForumCategoryRepository : BaseRepository<ForumCategory>
    {
        public ForumCategoryRepository(IObjectStore objectStore) : base(objectStore) { }
    }

    public class ForumHeadingRepository : BaseRepository<ForumHeading>
    {
        public ForumHeadingRepository(IObjectStore objectStore) : base(objectStore) { }
    }

    public class ForumTopicRepository : BaseRepository<ForumTopic>
    {
        public ForumTopicRepository(IObjectStore objectStore) : base(objectStore) { }

        public List<ForumTopic> Lookup(string owner, object forumID)
        {
            List<ForumTopic> lookupQuery;

            if (String.IsNullOrEmpty(owner) && (forumID == null))
                return GetAll();
            else if (!String.IsNullOrEmpty(owner) && (forumID == null))
            {
                Matcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, 0, 0);
                lookupQuery = Query(ownerMatcher);
            }
            else if (String.IsNullOrEmpty(owner) && (forumID != null))
            {
                IntMatcher headingMatcher = new IntMatcher((int)forumID, "HeadingKey", MatchCode.Exact, 0, 0);
                lookupQuery = Query(headingMatcher);
            }
            else
            {
                Matcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, 0, 0);
                IntMatcher headingMatcher = new IntMatcher((int)forumID, "HeadingKey", MatchCode.Exact, 0, 0);
                CompoundMatcher matcher = new CompoundMatcher(new List<Matcher>() { ownerMatcher, headingMatcher }, null, MatchCode.And, 0, 0);
                lookupQuery = Query(matcher);
            }

            return lookupQuery;
        }
    }

    public class ForumPostingRepository : BaseRepository<ForumPosting>
    {
        public ForumPostingRepository(IObjectStore objectStore) : base(objectStore) { }

        public List<ForumPosting> Lookup(string owner, object headingID, object topicID, LanguageStringMatcher textMatcher)
        {
            List<ForumPosting> lookupQuery = null;

            if (String.IsNullOrEmpty(owner) && (headingID == null) && (topicID == null))
            {
                List<ForumPosting> postings = GetAll();

                if ((postings != null) && (textMatcher != null))
                {
                    if (textMatcher.PageSize > 0)
                        postings = postings.Where(x => textMatcher.Match(x.Text)).
                            Skip((textMatcher.Page - 1) * textMatcher.PageSize).Take(textMatcher.PageSize).ToList();
                    else
                        postings = postings.Where(x => textMatcher.Match(x.Text)).ToList();
                }

                return postings;
            }
            else if (String.IsNullOrEmpty(owner))
            {
                if (topicID != null)
                {
                    IntMatcher topicMatcher = new IntMatcher((int)topicID, "TopicKey", MatchCode.Exact, 0, 0);
                    lookupQuery = Query(topicMatcher);
                }
                else if (headingID != null)
                {
                    IntMatcher headingMatcher = new IntMatcher((int)headingID, "HeadingKey", MatchCode.Exact, 0, 0);
                    lookupQuery = Query(headingMatcher);
                }
            }
            else
            {
                if (topicID != null)
                {
                    Matcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, 0, 0);
                    IntMatcher topicMatcher = new IntMatcher((int)topicID, "TopicKey", MatchCode.Exact, 0, 0);
                    CompoundMatcher matcher = new CompoundMatcher(new List<Matcher>() { ownerMatcher, topicMatcher }, null, MatchCode.And, 0, 0);
                    lookupQuery = Query(matcher);
                }
                else if (headingID != null)
                {
                    Matcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, 0, 0);
                    IntMatcher headingMatcher = new IntMatcher((int)headingID, "HeadingKey", MatchCode.Exact, 0, 0);
                    CompoundMatcher matcher = new CompoundMatcher(new List<Matcher>() { ownerMatcher, headingMatcher }, null, MatchCode.And, 0, 0);
                    lookupQuery = Query(matcher);
                }
                else
                {
                    Matcher ownerMatcher = new StringMatcher(owner, "Owner", MatchCode.Exact, 0, 0);
                    lookupQuery = Query(ownerMatcher);
                }
            }

            if ((lookupQuery != null) && (textMatcher != null))
                lookupQuery = lookupQuery.Where(x => textMatcher.Match(x.Text)).ToList();

            return lookupQuery;
        }
    }
}
