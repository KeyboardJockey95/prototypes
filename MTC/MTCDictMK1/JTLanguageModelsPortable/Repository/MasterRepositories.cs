using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;

namespace JTLanguageModelsPortable.Repository
{
    public class MarkupTemplateRepository : BaseRepository<MarkupTemplate>
    {
        public MarkupTemplateRepository(IObjectStore objectStore) : base(objectStore) { }
        public List<MarkupTemplate> GetList(string userName)
        {
            List<MarkupTemplate> markupTemplates = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            if (markupTemplates != null)
                markupTemplates.Sort(MarkupTemplate.CompareIndices);
            return markupTemplates;
        }
        public MarkupTemplate GetNamed(string userName, string name)
        {
            List<MarkupTemplate> list = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            MarkupTemplate obj = null;
            if (list != null)
                obj = list.FirstOrDefault(x => (x.Title != null) && (x.Title.Count() != 0) && (x.Title.AnyTextMatchesExact(name)));
            return obj;
        }
        public MarkupTemplate GetGuid(Guid guid)
        {
            List<MarkupTemplate> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            MarkupTemplate obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
        public List<MarkupTemplate> GetAutomatedTemplates(string userName)
        {
            List<MarkupTemplate> markupTemplates = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            if (markupTemplates != null)
            {
                int count = markupTemplates.Count;
                int index;

                for (index = count - 1; index >= 0; index--)
                {
                    MarkupTemplate testMarkup = markupTemplates[index];

                    if (testMarkup.FindMarkupElement("Generate", null, -1) == null)
                        markupTemplates.RemoveAt(index);
                }
            }
            if (markupTemplates != null)
                markupTemplates.Sort(MarkupTemplate.CompareIndices);
            return markupTemplates;
        }
    }

    public class NodeMasterRepository : BaseRepository<NodeMaster>
    {
        public NodeMasterRepository(IObjectStore objectStore) : base(objectStore) { }
        public List<NodeMaster> GetList(string userName)
        {
            List<NodeMaster> list = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            if (list != null)
                list.Sort(NodeMaster.CompareIndices);
            return list;
        }
        public NodeMaster GetNamed(string userName, string name)
        {
            List<NodeMaster> list = Query(new StringMatcher(userName, "Owner", MatchCode.Exact, 0, 0));
            NodeMaster obj = null;
            if (list != null)
                obj = list.FirstOrDefault(x => (x.Title != null) && (x.Title.Count() != 0) && (x.Title.AnyTextMatchesExact(name)));
            return obj;
        }
        public NodeMaster GetGuid(Guid guid)
        {
            List<NodeMaster> list = Query(new GuidMatcher(guid, "Guid", MatchCode.Exact, 0, 0));
            NodeMaster obj = null;
            if (list != null)
                obj = list.FirstOrDefault();
            return obj;
        }
    }
}
