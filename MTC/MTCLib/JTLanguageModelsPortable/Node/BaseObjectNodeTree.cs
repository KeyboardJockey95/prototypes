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
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Node
{
    public enum LevelCode
    {
        None,
        AbsoluteBeginner,
        Beginner,
        Intermediate,
        Advanced,
        Any
    };

    public class BaseObjectNodeTree : BaseObjectNode
    {
        protected LevelCode _Level;
        protected List<BaseObjectNode> _Nodes;
        protected int _NodeOrdinal;
        protected int _RemoteKey;
        public Dictionary<object, BaseObjectNodeTree> TreeCache;   // For reference items. Not saved.
        public static List<string> LevelStrings = new List<string>()
            { "None", "Absolute Beginner", "Beginner", "Intermediate", "Advanced" };
        public static List<string> LevelStringsAny = new List<string>()
            { "None", "Absolute Beginner", "Beginner", "Intermediate", "Advanced", "Any" };

        public BaseObjectNodeTree(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs, string owner,
                List<BaseObjectNode> children,
                List<IBaseObjectKeyed> options, MarkupTemplate markupTemplate, MarkupTemplateReference markupReference,
                List<BaseObjectContent> contentChildren, List<BaseObjectContent> contentList, NodeMaster master,
                List<BaseObjectNode> nodes)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner, null, null, children,
                options, markupTemplate, markupReference, contentChildren, contentList, master)
        {
            _Level = LevelCode.None;
            _Nodes = nodes;
            _NodeOrdinal = 0;
            _RemoteKey = -1;
            TreeCache = null;
        }

        public BaseObjectNodeTree(BaseObjectNodeTree other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectNodeTree(BaseObjectNodeTree other, object key)
            : base(other, key)
        {
            base.Copy(other);
            _Nodes = null;
            _NodeOrdinal = 0;
            _RemoteKey = -1;
            TreeCache = null;
            ModifiedFlag = false;
        }

        public BaseObjectNodeTree(XElement element)
        {
            OnElement(element);
        }

        public BaseObjectNodeTree()
        {
            ClearBaseObjectNodeTree();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectNodeTree();
        }

        public void ClearBaseObjectNodeTree()
        {
            _Level = LevelCode.None;
            _Nodes = null;
            _NodeOrdinal = 0;
            _RemoteKey = -1;
            TreeCache = null;
        }

        public virtual void CopyNodes(BaseObjectNodeTree other)
        {
            if (other != null)
            {
                _NodeOrdinal = other.NodeOrdinal;

                if (other.Nodes != null)
                {
                    _Nodes = new List<BaseObjectNode>(other.Nodes.Count());

                    foreach (BaseObjectNode node in other.Nodes)
                        _Nodes.Add(new BaseObjectNode(node));
                }
                else
                    _Nodes = null;
            }
            else
            {
                _Nodes = null;
                _NodeOrdinal = 0;
            }
        }

        public void Copy(BaseObjectNodeTree other)
        {
            base.Copy(other);
            _Level = other.Level;
            _RemoteKey = other.RemoteKey;
            CopyNodes(other);
            TreeCache = null;
        }

        public void CopyDeep(BaseObjectNodeTree other)
        {
            base.CopyDeep(other);
            _Level = other.Level;
            _RemoteKey = other.RemoteKey;
            CopyNodes(other);
            TreeCache = null;
        }

        public override void CopyProfile(BaseObjectNode other)
        {
            base.CopyProfile(other);

            if (other is BaseObjectNodeTree)
                Level = ((BaseObjectNodeTree)other).Level;
        }

        public override void CopyProfileExpand(BaseObjectNode other, UserProfile userProfile)
        {
            base.CopyProfileExpand(other, userProfile);

            if (other is BaseObjectNodeTree)
                Level = ((BaseObjectNodeTree)other).Level;
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectNodeTree(this);
        }

        public LevelCode Level
        {
            get
            {
                return _Level;
            }
            set
            {
                if (value != _Level)
                {
                    _Level = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string LevelString
        {
            get
            {
                return GetStringFromLevel(_Level);
            }
            set
            {
                LevelCode level = GetLevelFromString(value);
                Level = level;
            }
        }

        public static string GetStringFromLevel(LevelCode level)
        {
            string str;

            switch (level)
            {
                case LevelCode.None:
                    str = "None";
                    break;
                case LevelCode.AbsoluteBeginner:
                    str = "Absolute Beginner";
                    break;
                case LevelCode.Beginner:
                    str = "Beginner";
                    break;
                case LevelCode.Intermediate:
                    str = "Intermediate";
                    break;
                case LevelCode.Advanced:
                    str = "Advanced";
                    break;
                case LevelCode.Any:
                    str = "Any";
                    break;
                default:
                    str = "None";
                    break;
            }

            return str;
        }

        public static LevelCode GetLevelFromString(string str)
        {
            LevelCode level;

            switch (str)
            {
                case "None":
                    level = LevelCode.None;
                    break;
                case "AbsoluteBeginner":
                case "Absolute Beginner":
                    level = LevelCode.AbsoluteBeginner;
                    break;
                case "Beginner":
                    level = LevelCode.Beginner;
                    break;
                case "Intermediate":
                    level = LevelCode.Intermediate;
                    break;
                case "Advanced":
                    level = LevelCode.Advanced;
                    break;
                case "Any":
                    level = LevelCode.Any;
                    break;
                default:
                    level = LevelCode.None;
                    break;
            }

            return level;
        }

        public override string MediaTildeUrl
        {
            get
            {
                return ApplicationData.MediaTildeUrl + "/"+ MediaUtilities.FileFriendlyName(Owner) + "/" + Directory;
            }
        }

        public override BaseObjectNodeTree LocalTree
        {
            get
            {
                return this;
            }
        }

        public override bool IsTree()
        {
            return true;
        }

        public override bool IsGroup()
        {
            return false;
        }

        public override string TypeLabel
        {
            get
            {
                if (IsCourse())
                    return "Course";
                else
                    return "Plan";
            }
        }

        public string SourceKeyHash
        {
            get
            {
                if (_Key == null)
                    return "null";
                string source = Source;
                string sourceHash;
                if (!String.IsNullOrEmpty(source))
                    sourceHash = source.Substring(0, 1);
                else
                    sourceHash = "U";
                string sourceKeyHash = sourceHash + KeyString;
                return sourceKeyHash;
            }
        }

        public List<BaseObjectNode> Nodes
        {
            get
            {
                return _Nodes;
            }
            set
            {
                if (value != _Nodes)
                {
                    _Nodes = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int NodeOrdinal
        {
            get
            {
                return _NodeOrdinal;
            }
            set
            {
                if (_NodeOrdinal != value)
                {
                    _NodeOrdinal = value;
                    ModifiedFlag = true;
                }
            }
        }

        public int RemoteKey
        {
            get
            {
                return _RemoteKey;
            }
            set
            {
                if (value != _RemoteKey)
                {
                    _RemoteKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public IBaseObject GetObjectFromPath(List<string> path, LanguageID uiLanguageID)
        {
            if (path == null)
                return null;

            int nodeCount = path.Count();

            if (nodeCount == 0)
                return null;

            string treeName = path[0];

            if (treeName != GetTitleString(uiLanguageID))
                return null;

            if (nodeCount == 1)
                return this;

            IBaseObject obj = GetObjectFromPathRecurse(path, uiLanguageID, 1);

            return obj;
        }

        public bool HasNodes()
        {
            return (((_Nodes != null) && (_Nodes.Count() != 0)) ? true : false);
        }

        public bool HasNodesWithSource(string source)
        {
            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    if (node.Source == source)
                        return true;
                }
            }

            return false;
        }

        public List<BaseObjectNode> LookupNodes(Matcher matcher)
        {
            if (_Nodes == null)
                return new List<BaseObjectNode>();
            IEnumerable<BaseObjectNode> lookupQuery =
                from node in _Nodes
                where (matcher.Match(node))
                select node;
            return lookupQuery.ToList();
        }

        public BaseObjectNode GetNode(object key)
        {
            if ((_Nodes != null) && (key != null))
                return _Nodes.FirstOrDefault(x => x.MatchKey(key));
            return null;
        }

        public List<BaseObjectNode> GetNodes(List<object> keys)
        {
            List<BaseObjectNode> nodes = new List<BaseObjectNode>();

            if ((_Nodes != null) && (keys != null))
            {
                foreach (object key in keys)
                {
                    BaseObjectNode node = GetNode(key);

                    if (node != null)
                        nodes.Add(node);
                }
            }

            return nodes;
        }

        public BaseObjectNode GetNodeWithSource(object key, string source)
        {
            if ((_Nodes != null) && (key != null))
                return _Nodes.FirstOrDefault(x => x.MatchKey(key) && (x.Source == source));
            return null;
        }

        public BaseObjectNode GetNodeWithSourceAndTitle(string source, string title, LanguageID languageID)
        {
            if ((_Nodes != null) && (source != null))
                return _Nodes.FirstOrDefault(x => (x.Source == source) && (x.Title != null) && (x.Title.Text(languageID) == title));
            return null;
        }

        public BaseObjectNode GetNodeWithLabelAndTitle(string label, string title, LanguageID languageID)
        {
            if ((_Nodes != null) && (label != null))
                return _Nodes.FirstOrDefault(x => (x.Label == label) && (x.Title != null) && (x.Title.Text(languageID) == title));
            return null;
        }

        public BaseObjectNode GetNodeWithTitle(string title, LanguageID languageID)
        {
            if (_Nodes != null)
                return _Nodes.FirstOrDefault(x => (x.Title != null) && (x.Title.Text(languageID) == title));
            return null;
        }

        public BaseObjectNode GetChildNodeWithLabel(string label)
        {
            BaseObjectNode child = GetChildWithLabel(label);
            return child;
        }

        public BaseObjectNode GetNodeIndexed(int index)
        {
            if ((_Nodes != null) && (index >= 0) && (index < _Nodes.Count()))
                return _Nodes.ElementAt(index);
            return null;
        }

        public List<BaseObjectNode> GetNodesWithType(string typeName)
        {
            List<BaseObjectNode> nodes = new List<BaseObjectNode>();

            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    if (node.GetType().Name == typeName)
                        nodes.Add(node);
                }
            }

            return nodes;
        }

        public BaseObjectNode GetSiblingNode(BaseObjectNode node)
        {
            if (node == null)
                return null;
            BaseObjectNode sibling = null;
            BaseObjectNode child;
            if (node.HasParent())
            {
                BaseObjectNode parent = node.Parent;
                if (parent == null)
                    throw new ObjectException("Parent node is null.");
                child = parent.GetChild(node.Key);
                if (child == null)
                    return null;
                int index = parent.Children.IndexOf(child);
                if (index < parent.ChildCount() - 1)
                    index++;
                else if (index > 0)
                    index--;
                sibling = parent.GetChildIndexed(index);
            }
            else
            {
                child = GetChild(node.Key);
                if (child == null)
                    return null;
                int index = Children.IndexOf(child);
                if (index < ChildCount() - 1)
                    index++;
                else if (index > 0)
                    index--;
                sibling = GetChildIndexed(index);
            }
            return sibling;
        }

        public BaseObjectNode GetNextSiblingNode(BaseObjectNode node)
        {
            if (node == null)
                return null;
            BaseObjectNode sibling = null;
            BaseObjectNode child;
            if (node.HasParent())
            {
                BaseObjectNode parent = node.Parent;
                if (parent == null)
                    throw new ObjectException("Parent node is null.");
                child = parent.GetChild(node.Key);
                if (child == null)
                    return null;
                int index = parent.Children.IndexOf(child);
                if (index < parent.ChildCount() - 1)
                {
                    index++;
                    sibling = parent.GetChildIndexed(index);
                }
            }
            else
            {
                child = GetChild(node.Key);
                if (child == null)
                    return null;
                int index = Children.IndexOf(child);
                if (index < ChildCount() - 1)
                {
                    index++;
                    sibling = GetChildIndexed(index);
                }
            }
            return sibling;
        }

        public BaseObjectNode GetPreviousSiblingNode(BaseObjectNode node)
        {
            if (node == null)
                return null;
            BaseObjectNode sibling = null;
            BaseObjectNode child;
            if (node.HasParent())
            {
                BaseObjectNode parent = node.Parent;
                if (parent == null)
                    throw new ObjectException("Parent node is null.");
                child = parent.GetChild(node.Key);
                if (child == null)
                    return null;
                int index = parent.Children.IndexOf(child);
                if (index > 0)
                {
                    index--;
                    sibling = parent.GetChildIndexed(index);
                }
            }
            else
            {
                child = GetChild(node.Key);
                if (child == null)
                    return null;
                int index = Children.IndexOf(child);
                if (index > 0)
                {
                    index--;
                    sibling = GetChildIndexed(index);
                }
            }
            return sibling;
        }

        public List<BaseObjectNode> CollectNodesWithLabel(string label)
        {
            List<BaseObjectNode> list = new List<BaseObjectNode>();
            List<BaseObjectNode> children = Children;

            if (children != null)
            {
                foreach (BaseObjectNode child in children)
                {
                    if (child.Label == label)
                        list.Add(child);
                    else
                        CollectNodesWithLabel(child, label, list);
                }
            }

            return list;
        }

        public void CollectNodesWithLabel(BaseObjectNode node, string label, List<BaseObjectNode> list)
        {
            if (node.ChildCount() != 0)
            {
                List<BaseObjectNode> children = node.Children;

                foreach (BaseObjectNode child in children)
                {
                    if (child.Label == label)
                        list.Add(child);
                    else
                        CollectNodesWithLabel(child, label, list);
                }
            }
        }

        public bool AddNode(BaseObjectNode node)
        {
            if (_Nodes == null)
                _Nodes = new List<BaseObjectNode>(1) { node };
            else
                _Nodes.Add(node);
            node.Key = ++_NodeOrdinal;
            node.Tree = this;
            ModifiedFlag = true;
            return true;
        }

        public void AddNode(BaseObjectNode parent, BaseObjectNode node)
        {
            AddNode(node);

            if (parent != null)
            {
                node.Parent = parent;
                parent.AddChild(node);
            }
            else
                AddChild(node);
        }

        public bool InsertNodeIndexed(int index, BaseObjectNode node)
        {
            if (_Nodes == null)
                _Nodes = new List<BaseObjectNode>(1) { node };
            else if (index < _Nodes.Count())
                _Nodes.Insert(index, node);
            else
                _Nodes.Add(node);
            node.Key = ++_NodeOrdinal;
            node.Tree = this;
            ModifiedFlag = true;
            return true;
        }

        public bool DeleteNode(BaseObjectNode node)
        {
            if (_Nodes != null)
            {
                if (_Nodes.Remove(node))
                {
                    node.Tree = null;
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteNodeKey(object key)
        {
            if ((_Nodes != null) && (key != null))
            {
                BaseObjectNode node = GetNode(key);
                if (node != null)
                {
                    _Nodes.Remove(node);
                    node.Tree = null;
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool DeleteNodeIndexed(int index)
        {
            if ((_Nodes != null) && (index >= 0) && (index < _Nodes.Count()))
            {
                BaseObjectNode node = _Nodes[index];
                node.Tree = null;
                _Nodes.RemoveAt(index);
                ModifiedFlag = true;
                return true;
            }
            return false;
        }

        public void DeleteAllNodes()
        {
            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                    node.Tree = null;

                if (_Nodes.Count() != 0)
                    ModifiedFlag = true;

                _Nodes.Clear();
            }
        }

        public void DeleteAllNodesWithLabel(string label)
        {
            if (_Nodes != null)
            {
                int count = _Nodes.Count();
                int index;
                for (index = count - 1; index >= 0; index--)
                {
                    BaseObjectNode node = _Nodes[index];
                    if (node.Label != label)
                        continue;
                    DeleteChild(node);
                    _Nodes.RemoveAt(index);
                    node.Tree = null;
                    ModifiedFlag = true;
                }
            }
        }

        public int NodeCount()
        {
            if (_Nodes != null)
                return (_Nodes.Count());
            return 0;
        }

        public int NodeCountWithLabel(string label)
        {
            int count = 0;
            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    if (node.Label == label)
                        count++;
                }
            }
            return count;
        }

        public BaseObjectNode NodeNavigation(BaseObjectNode startingNode, string direction)
        {
            BaseObjectNode node = null;

            if (startingNode == null)
                return null;

            switch (direction)
            {
                case "Previous":
                    node = GetPreviousSiblingNode(startingNode);
                    break;
                case "Up":
                    node = startingNode.Parent;
                    if (node == null)
                        node = this;
                    break;
                case "Down":
                    if (startingNode.Children != null)
                        node = startingNode.Children.FirstOrDefault();
                    break;
                case "Next":
                default:
                    node = GetNextSiblingNode(startingNode);
                    break;
            }

            return node;
        }

        public bool ContentNavigation(BaseObjectNode startingNode, BaseObjectContent startingContent, string direction,
            out BaseObjectNode endingNode, out BaseObjectContent endingContent)
        {
            bool flag = true;

            endingNode = startingNode;
            endingContent = startingContent;

            if (startingNode == null)
                startingNode = this;

            if (startingContent == null)
                return false;

            switch (direction)
            {
                case "Previous":
                    if ((endingContent = startingNode.GetPreviousSiblingContent(startingContent)) == null)
                    {
                        endingContent = startingContent;
                        flag = false;
                    }
                    break;
                case "Up":
                    if (startingNode.IsTree())
                        flag = false;
                    else if (startingContent.HasContentParent())
                        endingContent = startingContent.ContentParent;
                    else
                    {
                        endingNode = startingNode.Parent;
                        if (endingNode == null)
                            endingNode = this;
                        endingContent = endingNode.GetContent(startingContent.KeyString);
                    }
                    if (endingContent == null)
                    {
                        endingNode = startingNode;
                        endingContent = startingContent;
                        flag = false;
                    }
                    break;
                case "Down":
                    if (startingContent.HasContent())
                        endingContent = startingContent.ContentList.FirstOrDefault();
                    else if (startingNode.Children != null)
                    {
                        endingNode = startingNode.Children.FirstOrDefault();
                        endingContent = endingNode.GetContent(startingContent.KeyString);
                        if (endingContent == null)
                        {
                            endingNode = startingNode;
                            endingContent = startingContent;
                            flag = false;
                        }
                    }
                    else
                        flag = false;
                    break;
                case "Next":
                default:
                    if ((endingContent = startingNode.GetNextSiblingContent(startingContent)) == null)
                    {
                        endingContent = startingContent;
                        flag = false;
                    }
                    break;
            }

            return flag;
        }

        public BaseObjectContent GetNodeContentWithStorageKey(object contentStorageKey, ContentClassType classType)
        {
            BaseObjectContent content = null;

            if (_Nodes != null)
            {
                int count = _Nodes.Count();
                int index;
                for (index = 0; index < count; index++)
                {
                    BaseObjectNode node = _Nodes[index];
                    content = node.GetContentWithStorageKey(contentStorageKey, classType);
                    if (content != null)
                        return content;
                }
            }
            return content;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if (_Nodes != null)
                {
                    foreach (BaseObjectNode node in _Nodes)
                    {
                        if (node.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                base.Modified = value;

                if (_Nodes != null)
                {
                    foreach (BaseObjectNode node in _Nodes)
                        node.Modified = false;
                }
            }
        }

        public void ResolveMasterReferences(IMainRepository mainRepository)
        {
            Dictionary<int, NodeMaster> masterCache = new Dictionary<int, NodeMaster>();
            NodeMaster master = null;

            ResolveMasterReference(mainRepository);

            master = Master;

            if (master != null)
                masterCache.Add(master.KeyInt, master);

            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    NodeMasterReference masterReference = node.MasterReference;

                    master = null;

                    if (masterReference != null)
                    {
                        if (masterReference.Item == null)
                        {
                            if (masterCache.TryGetValue(masterReference.KeyInt, out master))
                                masterReference.Item = master;
                            else
                            {
                                node.ResolveMasterReference(mainRepository);

                                NodeMaster testMaster;

                                if (masterCache.TryGetValue(masterReference.KeyInt, out testMaster))
                                    masterReference.Item = master = testMaster;
                                else
                                {
                                    master = masterReference.Item;

                                    if (master != null)
                                        masterCache.Add(master.KeyInt, master);
                                }
                            }
                        }
                        else
                        {
                            NodeMaster testMaster;

                            master = masterReference.Item;

                            if (!masterCache.TryGetValue(masterReference.KeyInt, out testMaster))
                                masterCache.Add(master.KeyInt, master);
                        }
                    }
                }
            }
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (recurseChildren)
                ResolveMasterReferences(mainRepository);

            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (recurseChildren && (_Nodes != null))
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    if (!node.SaveReferences(mainRepository, recurseParents, recurseChildren))
                        returnValue = false;
                }
            }

            if (!base.SaveReferences(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override bool UpdateReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (recurseChildren && (_Nodes != null))
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    if (!node.UpdateReferences(mainRepository, recurseParents, recurseChildren))
                        returnValue = false;
                }
            }

            if (!base.UpdateReferences(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override bool UpdateReferencesCheck(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (recurseChildren && (_Nodes != null))
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    if (!node.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren))
                        returnValue = false;
                }
            }

            if (!base.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            if (recurseChildren && (_Nodes != null))
            {
                foreach (BaseObjectNode node in _Nodes)
                    node.ClearReferences(recurseParents, recurseChildren);
            }

            base.ClearReferences(recurseParents, recurseChildren);
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            base.CollectReferences(references, externalSavedChildren,
                null, nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                mediaFiles, visitFunction);

            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                {
                    bool useIt = true;

                    if ((nodeSelectFlags != null) && nodeSelectFlags.TryGetValue(node.KeyInt, out useIt) && !useIt)
                        continue;

                    node.CollectReferences(references, externalSavedChildren, null,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                        mediaFiles, visitFunction);
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XAttribute("Level", LevelString));

            if ((_Nodes != null) && (_Nodes.Count() != 0))
            {
                element.Add(new XAttribute("NodeCount", _Nodes.Count().ToString()));
                element.Add(new XAttribute("NodeOrdinal", _NodeOrdinal.ToString()));

                if (_RemoteKey != -1)
                    element.Add(new XAttribute("RemoteKey", _RemoteKey.ToString()));

                foreach (BaseObjectNode node in _Nodes)
                    element.Add(node.GetElement("Node"));
            }

            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<int, bool> childNodeFlags,
            Dictionary<string, bool> childContentFlags)
        {
            XElement element = base.GetElementFiltered(name, childNodeFlags, childContentFlags);

            element.Add(new XAttribute("Level", LevelString));

            if ((_Nodes != null) && (_Nodes.Count() != 0))
            {
                int nodeCount = 0;

                element.Add(new XAttribute("NodeOrdinal", _NodeOrdinal.ToString()));

                if (_RemoteKey != -1)
                    element.Add(new XAttribute("RemoteKey", _RemoteKey.ToString()));

                foreach (BaseObjectNode node in _Nodes)
                {
                    bool useIt = true;

                    if ((childNodeFlags != null) && childNodeFlags.TryGetValue(node.KeyInt, out useIt) && !useIt)
                        continue;

                    element.Add(node.GetElementFiltered("Node", childNodeFlags, childContentFlags));
                    nodeCount++;
                }

                element.Add(new XAttribute("NodeCount", nodeCount.ToString()));
            }

            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Level":
                    _Level = GetLevelFromString(attributeValue);
                    break;
                case "NodeCount":
                    _Nodes = new List<BaseObjectNode>(ObjectUtilities.GetIntegerFromString(attributeValue, 0));
                    break;
                case "NodeOrdinal":
                    _NodeOrdinal = Convert.ToInt32(ObjectUtilities.GetIntegerFromString(attributeValue, 0));
                    break;
                case "RemoteKey":
                    RemoteKey = ObjectUtilities.ParseIntKeyString(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            bool returnValue;

            switch (childElement.Name.LocalName)
            {
                case "Node":
                    BaseObjectNode node = new BaseObjectNode(childElement);
                    node.Tree = this;
                    if (_Nodes == null)
                        _Nodes = new List<BaseObjectNode>(1) { node };
                    else
                        _Nodes.Add(node);
                    returnValue = true;
                    break;
                case "TreeData":    // Legacy
                    returnValue = true;
                    break;
                case "TreeDataTag":    // Legacy
                    returnValue = true;
                    break;
                default:
                    returnValue = base.OnChildElement(childElement);
                    break;
            }

            return returnValue;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseObjectNodeTree otherBaseObjectNodeTree = other as BaseObjectNodeTree;

            if (_Level != otherBaseObjectNodeTree.Level)
                return (int)_Level - (int)otherBaseObjectNodeTree.Level;

            if (otherBaseObjectNodeTree == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = CompareNodeLists(_Nodes, otherBaseObjectNodeTree.Nodes);

            return diff;
        }

        public static int Compare(BaseObjectNodeTree item1, BaseObjectNodeTree item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            int diff = item1.Compare(item2);
            if (diff != 0)
                return diff;
            return diff;
        }

        public static int CompareKeys(BaseObjectNodeTree object1, BaseObjectNodeTree object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (_Nodes != null)
            {
                foreach (BaseObjectNode node in _Nodes)
                    node.OnFixup(fixups);
            }
        }
    }
}
