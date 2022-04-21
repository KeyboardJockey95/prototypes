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
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Node
{
    public class BaseObjectNode : BaseMarkupContainer
    {
        protected BaseObjectNodeTree _Tree;
        protected NodeMasterReference _MasterReference;
        protected int _ParentKey;
        protected List<int> _ChildrenKeys;
        protected List<BaseObjectContent> _ContentList;

        public BaseObjectNode(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner,
                BaseObjectNodeTree tree, BaseObjectNode parent, List<BaseObjectNode> children,
                List<IBaseObjectKeyed> options, MarkupTemplate markupTemplate, MarkupTemplateReference markupReference,
                List<BaseObjectContent> contentChildren, List<BaseObjectContent> contentList, NodeMaster master)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner, options, markupTemplate, markupReference,
                null, contentChildren)
        {
            _Tree = tree;
            if (master != null)
                _MasterReference = new NodeMasterReference(master);
            else
                _MasterReference = null;
            Parent = parent;
            Children = children;
            _ContentList = contentList;
            ModifiedFlag = false;
        }

        public BaseObjectNode(BaseObjectNode other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectNode(BaseObjectNode other, object key)
            : base(other, key)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public BaseObjectNode(XElement element)
        {
            OnElement(element);
        }

        public BaseObjectNode()
        {
            ClearBaseObjectNode();
        }

        public override void Clear()
        {
            base.Clear();
            ClearBaseObjectNode();
        }

        public void ClearBaseObjectNode()
        {
            _Tree = null;
            _MasterReference = null;
            _ParentKey = 0;
            _ChildrenKeys = null;
            _ContentList = null;
        }

        public void Copy(BaseObjectNode other)
        {
            base.Copy(other);

            if (other == null)
            {
                ClearBaseObjectNode();
                return;
            }

            _Tree = other.Tree;

            if (other.MasterReference != null)
                _MasterReference = new NodeMasterReference(other.MasterReference);
            else
                _MasterReference = null;

            _ParentKey = other.ParentKeyInt;

            if (other.ChildrenKeys != null)
                _ChildrenKeys = new List<int>(other.ChildrenIntKeys);
            else
                ChildrenKeys = null;

            if (other.ContentList != null)
                _ContentList = new List<BaseObjectContent>(other.ContentList);
            else
                _ContentList = null;
        }

        public void CopyDeep(BaseObjectNode other)
        {
            Copy(other);
            base.CopyDeep(other);
        }

        public virtual void CopyProfile(BaseObjectNode other)
        {
            CopyTitledObjectAndLanguages(other);

            if (other.MasterReference != null)
            {
                if (_MasterReference != null)
                {
                    if (!_MasterReference.MatchKey(other.MasterReference.Key))
                        MasterReference = new NodeMasterReference(other.MasterReference);
                }
                else
                    MasterReference = new NodeMasterReference(other.MasterReference);
            }
            else
                MasterReference = null;

            CopyMarkup(other);
        }

        public virtual void CopyProfileExpand(BaseObjectNode other, UserProfile userProfile)
        {
            CopyTitledObjectAndLanguagesExpand(other, userProfile);

            if (other.MasterReference != null)
            {
                if (_MasterReference != null)
                {
                    if (!_MasterReference.MatchKey(other.MasterReference.Key))
                        MasterReference = new NodeMasterReference(other.MasterReference);
                }
                else
                    MasterReference = new NodeMasterReference(other.MasterReference);
            }
            else
                MasterReference = null;

            CopyMarkup(other);
        }

        public override IBaseObject Clone()
        {
            return new BaseObjectNode(this);
        }

        public BaseObjectNode CloneNodeShallow()
        {
            BaseObjectNode node = new BaseObjectNode(this);
            node.ChildrenKeys = new List<object>();
            node.ContentChildrenKeys = new List<string>();
            node.ContentList = new List<BaseObjectContent>();
            return node;
        }

        public BaseObjectNodeTree Tree
        {
            get
            {
                return _Tree;
            }
            set
            {
                _Tree = value;
            }
        }

        public virtual BaseObjectNodeTree LocalTree
        {
            get
            {
                return _Tree;
            }
        }

        public virtual bool IsTree()
        {
            return false;
        }

        public bool IsCourse()
        {
            return Source == "Courses";
        }

        public bool IsPlan()
        {
            return Source == "Plans";
        }

        public virtual bool IsGroup()
        {
            switch (Label)
            {
                case "Group":
                    return true;
                case "Lesson":
                    return false;
                default:
                    if (HasChildren())
                        return true;
                    return false;
            }
        }

        public virtual bool IsLesson()
        {
            switch (Label)
            {
                case "Group":
                    return false;
                case "Lesson":
                    return true;
                default:
                    return false;
            }
        }

        public override string TypeLabel
        {
            get
            {
                if (IsGroup())
                    return "Group";
                else
                    return "Lesson";
            }
        }

        public override List<int> GetIndexPath()
        {
            List<int> indexPath;

            BaseObjectNode node = Parent;
            BaseObjectNodeTree tree = Tree;

            if (node != null)
            {
                indexPath = node.GetIndexPath();
                indexPath.Add(node.GetChildIndex(this));
            }
            else if (tree != null)
            {
                indexPath = tree.GetIndexPath();
                indexPath.Add(tree.GetChildIndex(this));
            }
            else
                return new List<int>();

            return indexPath;
        }

        public override List<string> GetNamePath(LanguageID uiLanguageID)
        {
            List<string> namePath;

            BaseObjectNode parentNode = Parent;
            BaseObjectNodeTree tree = Tree;

            if (parentNode != null)
                namePath = parentNode.GetNamePath(uiLanguageID);
            else if (tree != null)
                namePath = tree.GetNamePath(uiLanguageID);
            else
                return new List<string>() { GetTitleString(uiLanguageID) };

            namePath.Add(GetTitleString(uiLanguageID));

            return namePath;
        }

        public List<string> GetOriginalNamePath(LanguageID uiLanguageID)
        {
            List<string> namePath;

            BaseObjectNode parentNode = Parent;
            BaseObjectNodeTree tree = Tree;

            if (parentNode != null)
                namePath = parentNode.GetOriginalNamePath(uiLanguageID);
            else if (tree != null)
                namePath = tree.GetOriginalNamePath(uiLanguageID);
            else
            {
                MultiLanguageString originalTitle = GetOption("OriginalTitle") as MultiLanguageString;

                if (originalTitle != null)
                {
                    string newTitle = originalTitle.Text(uiLanguageID);
                    return new List<string>() { newTitle };
                }

                return new List<string>() { GetTitleString(uiLanguageID) };
            }

            namePath.Add(GetTitleString(uiLanguageID));

            return namePath;
        }

        public override string GetNamePathString(LanguageID uiLanguageID, string separator)
        {
            List<string> namePath = GetNamePath(uiLanguageID);
            string namePathString = TextUtilities.GetStringFromStringListDelimited(namePath, separator);
            return namePathString;
        }

        public string GetOriginalNamePathString(LanguageID uiLanguageID, string separator)
        {
            List<string> namePath = GetOriginalNamePath(uiLanguageID);
            string namePathString = TextUtilities.GetStringFromStringListDelimited(namePath, separator);
            return namePathString;
        }

        public MultiLanguageString GetNamePathMLS(List<LanguageID> languageIDs, string separator, string key)
        {
            MultiLanguageString mls = new MultiLanguageString(key);

            foreach (LanguageID languageID in languageIDs)
            {
                List<string> namePath = GetNamePath(languageID);
                string namePathString = TextUtilities.GetStringFromStringListDelimited(namePath, separator);
                mls.Add(new LanguageString(key, languageID, namePathString));
            }

            return mls;
        }

        public MultiLanguageString GetOrignalNamePathMLS(List<LanguageID> languageIDs, string separator, string key)
        {
            MultiLanguageString mls = new MultiLanguageString(key);

            foreach (LanguageID languageID in languageIDs)
            {
                List<string> namePath = GetOriginalNamePath(languageID);
                string namePathString = TextUtilities.GetStringFromStringListDelimited(namePath, separator);
                mls.Add(new LanguageString(key, languageID, namePathString));
            }

            return mls;
        }

        public string HierarchyImageFileName
        {
            get
            {
                if (HasImageFile)
                    return _ImageFileName;
                if (Parent != null)
                    return Parent.HierarchyImageFileName;
                return _ImageFileName;
            }
        }

        public string HierarchyImageFileTildeUrl
        {
            get
            {
                if (HasImageFile)
                    return ImageFileTildeUrl;
                else if (Parent != null)
                    return Parent.HierarchyImageFileTildeUrl;
                return ImageFileTildeUrl;
            }
        }

        public string HierarchyImageFileUrl
        {
            get
            {
                if (HasImageFile)
                    return ImageFileTildeUrl;
                else if (Parent != null)
                    return Parent.HierarchyImageFileUrl;
                return ImageFileUrl;
            }
        }

        public string HierarchyImageFileUrlWithMediaCheck
        {
            get
            {
                if (HasImageFile)
                    return ImageFileUrlWithMediaCheck;
                else if (Parent != null)
                    return Parent.HierarchyImageFileUrlWithMediaCheck;
                return ImageFileUrlWithMediaCheck;
            }
        }

        public bool HierarchyImageFileIsExternal
        {
            get
            {
                if (HasImageFile)
                    return ImageFileIsExternal;
                else if (Parent != null)
                    return Parent.HierarchyImageFileIsExternal;
                return ImageFileIsExternal;
            }
        }

        public string HierarchyImageFilePath
        {
            get
            {
                if (HasImageFile)
                    return ImageFilePath;
                else if (Parent != null)
                    return Parent.HierarchyImageFilePath;
                return ImageFilePath;
            }
        }

        public bool HierarchyImageFileExists
        {
            get
            {
                if (HasImageFile)
                    return ImageFileExists;
                else if (Parent != null)
                    return Parent.HierarchyImageFileExists;
                return ImageFileExists;
            }
        }

        public bool HasHierarchyImageFile
        {
            get
            {
                if (HasImageFile)
                    return HasImageFile;
                else if (Parent != null)
                    return Parent.HasHierarchyImageFile;
                return HasImageFile;
            }
        }

        public MediaStorageState HierarchyImageFileStorageState
        {
            get
            {
                if (HasImageFile)
                    return ImageFileStorageState;
                else if (Parent != null)
                    return Parent.HierarchyImageFileStorageState;
                return ImageFileStorageState;
            }
        }

        public NodeMasterReference MasterReference
        {
            get
            {
                return _MasterReference;
            }
            set
            {
                if (_MasterReference != value)
                {
                    _MasterReference = value;
                    ModifiedFlag = true;
                }
            }
        }

        public NodeMaster Master
        {
            get
            {
                if (_MasterReference != null)
                    return _MasterReference.Item;

                return null;
            }
            set
            {
                if ((_MasterReference != null) && (_MasterReference.Item != value))
                    MasterReference = new NodeMasterReference(value);
                else if (_MasterReference == null)
                    MasterReference = new NodeMasterReference(value);
            }
        }

        public int MasterKey
        {
            get
            {
                if (_MasterReference != null)
                    return _MasterReference.KeyInt;

                return 0;
            }
            set
            {
                if ((_MasterReference != null) && (_MasterReference.KeyInt != value))
                    MasterReference.Key = value;
                else if (_MasterReference == null)
                    MasterReference = new NodeMasterReference(value, Guid.Empty);
            }
        }

        public virtual void SetupMarkupTemplateFromMaster()
        {
            NodeMaster master = Master;

            if (master == null)
                return;

            if (master.IsLocalMarkupTemplate)
            {
                if (_LocalMarkupTemplate == null)
                {
                    if (master.CopyMarkupTemplate != null)
                        LocalMarkupTemplate = new MarkupTemplate(master.CopyMarkupTemplate);
                    else
                        LocalMarkupTemplate = new MarkupTemplate("(local)");

                    _LocalMarkupTemplate.LocalOwningObject = this;
                }
            }
            else
            {
                if (master.MarkupReference != null)
                    MarkupReference = new MarkupTemplateReference(master.MarkupReference);
                else
                    MarkupReference = null;
            }
        }

        public object ParentKey
        {
            get
            {
                return _ParentKey;
            }
            set
            {
                if ((int)value != _ParentKey)
                {
                    _ParentKey = (int)value;
                    ModifiedFlag = true;
                }
            }
        }

        public int ParentKeyInt
        {
            get
            {
                return _ParentKey;
            }
            set
            {
                if (value != _ParentKey)
                {
                    _ParentKey = value;
                    ModifiedFlag = true;
                }
            }
        }

        public BaseObjectNode Parent
        {
            get
            {
                if (_ParentKey > 0)
                {
                    BaseObjectNodeTree localTree = LocalTree;

                    if (localTree != null)
                        return localTree.GetNode(_ParentKey);
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    if (value.KeyInt != _ParentKey)
                    {
                        _ParentKey = value.KeyInt;
                        ModifiedFlag = true;
                    }
                }
                else if (_ParentKey > 0)
                {
                    _ParentKey = 0;
                    ModifiedFlag = true;
                }
            }
        }

        public string ParentSource
        {
            get
            {
                if (Parent != null)
                    return Parent.Source;
                return null;
            }
        }

        public bool HasParent()
        {
            return ((_ParentKey > 0) ? true : false);
        }

        public List<BaseObjectNode> NodeHierarchyList
        {
            get
            {
                List<BaseObjectNode> list = new List<BaseObjectNode>() { this };
                BaseObjectNode node = this;
                while (node.ParentKeyInt > 0)
                {
                    node = node.Parent;
                    if (node != null)
                        list.Insert(0, node);
                    else
                        break;
                }
                return list;
            }
        }

        public List<object> ChildrenKeys
        {
            get
            {
                if (_ChildrenKeys != null)
                    return _ChildrenKeys.Cast<object>().ToList();

                return null;
            }
            set
            {
                if (value != ChildrenKeys)
                {
                    if (value != null)
                        _ChildrenKeys = value.Cast<int>().ToList();
                    else
                        _ChildrenKeys = null;
                    ModifiedFlag = true;
                }
            }
        }

        public List<int> ChildrenIntKeys
        {
            get
            {
                return _ChildrenKeys;
            }
            set
            {
                if (value != _ChildrenKeys)
                {
                    if (value != null)
                        _ChildrenKeys = new List<int>(value);
                    else
                        _ChildrenKeys = null;
                    ModifiedFlag = true;
                }
            }
        }

        public List<BaseObjectNode> Children
        {
            get
            {
                if (_ChildrenKeys != null)
                {
                    List<BaseObjectNode> children = LocalTree.GetNodes(ChildrenKeys);
                    return children;
                }
                return null;
            }
            set
            {
                _ChildrenKeys = new List<int>();

                if (value != null)
                {
                    foreach (BaseObjectNode node in value)
                        _ChildrenKeys.Add(node.KeyInt);
                }

                ModifiedFlag = true;
            }
        }

        public bool HasChildren()
        {
            return (((_ChildrenKeys != null) && (_ChildrenKeys.Count() != 0)) ? true : false);
        }

        public bool HasChild(object key)
        {
            if ((_ChildrenKeys != null) && (key != null))
                return _ChildrenKeys.Contains((int)key);

            return false;
        }

        public bool HasChildWithLabel(string label)
        {
            List<BaseObjectNode> children = Children;

            if (children != null)
            {
                foreach (BaseObjectNode child in children)
                {
                    if (child.Label == label)
                        return true;
                }
            }

            return false;
        }

        public List<BaseObjectNode> LookupChildren(Matcher matcher)
        {
            List<BaseObjectNode> children = Children;
            if (children == null)
                return new List<BaseObjectNode>();
            IEnumerable<BaseObjectNode> lookupQuery =
                from child in children
                where (matcher.Match(child))
                select child;
            return lookupQuery.ToList();
        }

        public BaseObjectNode GetChild(object key)
        {
            if ((_ChildrenKeys != null) && (key != null) && _ChildrenKeys.Contains((int)key))
                return LocalTree.GetNode(key);
            return null;
        }

        public BaseObjectNode FindChild(string title)
        {
            List<BaseObjectNode> children = Children;
            if (children == null)
                return null;
            foreach (BaseObjectNode child in children)
            {
                if (child.Name == title)
                    return child;
            }
            return null;
        }

        public BaseObjectNode FindChild(string title, LanguageID languageID)
        {
            List<BaseObjectNode> children = Children;
            if (children == null)
                return null;
            foreach (BaseObjectNode child in children)
            {
                if (child.GetTitleString(languageID) == title)
                    return child;
            }
            return null;
        }

        public IBaseObject GetObjectFromPathRecurse(List<string> path, LanguageID uiLanguageID, int nodeIndex)
        {
            int nodeCount = path.Count();

            if (nodeIndex >= nodeCount)
                return null;

            string nodeName = path[nodeIndex];

            BaseObjectNode childNode = FindChild(nodeName, uiLanguageID);

            if (childNode != null)
            {
                if (nodeIndex == nodeCount - 1)
                    return childNode;

                return childNode.GetObjectFromPathRecurse(path, uiLanguageID, nodeIndex + 1);
            }
            else
            {
                BaseObjectContent content = GetContent(nodeName);

                if (content != null)
                {
                    if (nodeIndex == nodeCount - 1)
                        return content;

                    return GetObjectFromPathRecurse(path, uiLanguageID, nodeIndex + 1);
                }
            }

            return null;
        }

        public int GetChildIndex(BaseObjectNode childNode)
        {
            if (childNode == null)
                return -1;

            return GetChildKeyIndex(childNode.KeyInt);
        }

        public int GetChildKeyIndex(int childKey)
        {
            if (_ChildrenKeys == null)
                return -1;

            return _ChildrenKeys.IndexOf(childKey);
        }

        public BaseObjectNode GetChildIndexed(int index)
        {
            if ((_ChildrenKeys != null) && (index >= 0) && (index < _ChildrenKeys.Count()))
                return LocalTree.GetNode(_ChildrenKeys.ElementAt(index));
            return null;
        }

        public BaseObjectNode GetChildWithLabel(string label)
        {
            List<BaseObjectNode> children = Children;
            BaseObjectNode child = null;

            if (children != null)
                child = children.FirstOrDefault(x => (x.Label == label));

            return child;
        }

        public BaseObjectNode GetChildWithSourceAndTitle(string label, string title, LanguageID languageID)
        {
            List<BaseObjectNode> children = Children;
            if ((children != null) && (label != null))
                return children.FirstOrDefault(x => (x.Label == label) && (x.Title != null) && (x.Title.Text(languageID) == title));
            return null;
        }

        public BaseObjectNode GetChildWithTitle(string title, LanguageID languageID)
        {
            List<BaseObjectNode> children = Children;
            if ((children != null) && (title != null))
                return children.FirstOrDefault(x => (x.Title != null) && (x.Title.Text(languageID) == title));
            return null;
        }

        public bool AddChild(BaseObjectNode child)
        {
            if (child != null)
            {
                if (_ChildrenKeys != null)
                {
                    if (!_ChildrenKeys.Contains(child.KeyInt))
                    {
                        child.Index = _ChildrenKeys.Count();
                        _ChildrenKeys.Add(child.KeyInt);
                        ReindexChildren();
                        ModifiedFlag = true;
                        return true;
                    }
                }
                else
                {
                    child.Index = 0;
                    _ChildrenKeys = new List<int>(1) { child.KeyInt };
                    ReindexChildren();
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public bool InsertChildIndexed(int index, BaseObjectNode child)
        {
            if (child != null)
            {
                if (_ChildrenKeys != null)
                {
                    if ((index >= 0) && (index <= _ChildrenKeys.Count()))
                    {
                        child.Index = index;
                        _ChildrenKeys.Insert(index, child.KeyInt);
                        ReindexChildren();
                        ModifiedFlag = true;
                        return true;
                    }
                }
                else if (index == 0)
                {
                    child.Index = index;
                    _ChildrenKeys = new List<int>(1) { child.KeyInt };
                    ReindexChildren();
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public void AddChildNode(BaseObjectNode node)
        {
            LocalTree.AddNode(node);
            if (IsTree())
                node.ParentKey = 0;
            else
                node.Parent = this;
            AddChild(node);
        }

        public void InsertChildNode(int index, BaseObjectNode node)
        {
            LocalTree.AddNode(node);
            if (IsTree())
                node.ParentKey = 0;
            else
                node.Parent = this;
            InsertChildIndexed(index, node);
        }

        public void InsertChildNode(BaseObjectNode sibling, BaseObjectNode node)
        {
            LocalTree.AddNode(node);
            if (IsTree())
                node.ParentKey = 0;
            else
                node.Parent = this;
            int index = GetChildIndex(sibling);
            if (index != -1)
                InsertChildIndexed(index, node);
            else
                AddChild(node);
        }

        public bool DeleteChild(BaseObjectNode child)
        {
            if (child != null)
            {
                if (_ChildrenKeys != null)
                {
                    if (_ChildrenKeys.Remove(child.KeyInt))
                    {
                        ReindexChildren();
                        ModifiedFlag = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool DeleteChildKey(object key)
        {
            if (key != null)
            {
                if (_ChildrenKeys != null)
                {
                    if (_ChildrenKeys.Remove((int)key))
                    {
                        ReindexChildren();
                        ModifiedFlag = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool DeleteChildIndexed(int index)
        {
            if (_ChildrenKeys != null)
            {
                if ((index >= 0) && (index < _ChildrenKeys.Count()))
                {
                    _ChildrenKeys.RemoveAt(index);
                    ReindexChildren();
                    ModifiedFlag = true;
                    return true;
                }
            }
            return false;
        }

        public void DeleteAllChildren()
        {
            if (_ChildrenKeys != null)
                _ChildrenKeys.Clear();
        }

        public int ChildCount()
        {
            if (_ChildrenKeys != null)
                return (_ChildrenKeys.Count());
            return 0;
        }

        public int LessonCount()
        {
            int lessonCount = 0;

            if (HasChildren())
            {
                foreach (BaseObjectNode childNode in Children)
                {
                    if (childNode.IsLesson())
                        lessonCount++;
                    else
                        lessonCount += childNode.LessonCount();
                }
            }

            return lessonCount;
        }

        public bool MoveChild(BaseObjectNode child, bool moveUp)
        {
            if ((_ChildrenKeys == null) || (child == null))
                return false;

            int key = child.KeyInt;
            int currentIndex = _ChildrenKeys.IndexOf(key);
            int newIndex;

            if (currentIndex == -1)
                return false;

            if (moveUp)
            {
                if (currentIndex == 0)
                    return false;

                newIndex = currentIndex - 1;
            }
            else
            {
                if (currentIndex == _ChildrenKeys.Count() - 1)
                    return false;

                newIndex = currentIndex + 1;
            }

            _ChildrenKeys.RemoveAt(currentIndex);
            _ChildrenKeys.Insert(newIndex, key);
            ReindexChildren();
            ModifiedFlag = true;

            return true;
        }

        public bool ReindexChildren()
        {
            bool returnValue = false;

            if (_ChildrenKeys == null)
                return returnValue;

            int index = 0;

            foreach (int key in _ChildrenKeys)
            {
                BaseObjectNode child = GetChild(key);

                if (child == null)
                    continue;

                if (child.Index != index)
                {
                    child.Index = index;
                    ModifiedFlag = true;
                    returnValue = true;
                }

                index++;
            }

            return returnValue;
        }

        public List<int> GetDecendentKeys(List<int> keys)
        {
            if (keys == null)
                keys = new List<int>();

            if (_ChildrenKeys == null)
                return keys;

            foreach (BaseObjectNode childNode in Children)
            {
                keys.Add(childNode.KeyInt);
                childNode.GetDecendentKeys(keys);
            }

            return keys;
        }

        public override bool AddContentChild(BaseObjectContent content)
        {
            if (content == null)
                return false;
            if (AddContent(content))
                return base.AddContentChild(content);
            return false;
        }

        public override bool InsertContentChildIndexed(int index, BaseObjectContent content)
        {
            if (content == null)
                return false;
            if (!AddContent(content))
                return false;
            if (!base.InsertContentChildIndexed(index, content))
                return false;
            return true;
        }

        public override List<BaseObjectContent> ContentChildren
        {
            get
            {
                List<BaseObjectContent> contentList = new List<BaseObjectContent>();

                if (_ContentChildrenKeys != null)
                {
                    foreach (string key in _ContentChildrenKeys)
                    {
                        BaseObjectContent content = GetContent(key);

                        if (content != null)
                            contentList.Add(content);
                    }
                }

                return contentList;
            }
            set
            {
                base.ContentChildren = value;
            }
        }

        public override List<BaseObjectContent> ContentList
        {
            get
            {
                return _ContentList;
            }
            set
            {
                if (_ContentList != value)
                {
                    _ContentList = value;
                    ReindexContent();
                    ModifiedFlag = true;
                }
            }
        }

        public override List<string> ContentKeysList
        {
            get
            {
                List<string> contentKeys = new List<string>();
                if (_ContentList != null)
                {
                    foreach (BaseObjectContent content in _ContentList)
                        contentKeys.Add(content.KeyString);
                }
                return contentKeys;
            }
        }

        public bool AddContent(BaseObjectContent content)
        {
            if (content == null)
                return false;
            if (_ContentList != null)
            {
                if (!_ContentList.Contains(content))
                    _ContentList.Add(content);
            }
            else
                _ContentList = new List<BaseObjectContent>() { content };
            ModifiedFlag = true;
            return true;
        }

        public List<BaseObjectContent> CollectContent(string contentType, string contentSubType,
            List<BaseObjectContent> contentList = null)
        {
            List<BaseObjectContent> children = (contentList != null ? contentList : new List<BaseObjectContent>());

            GetContentListWithTypeAndSubType(contentType, contentSubType, children);

            if (ChildCount() != 0)
            {
                foreach (BaseObjectNode childNode in Children)
                    childNode.CollectContent(contentType, contentSubType, children);
            }

            return children;
        }

        public List<BaseObjectContent> CollectContent(ContentClassType contentClass,
            List<BaseObjectContent> contentList = null)
        {
            List<BaseObjectContent> children = (contentList != null ? contentList : new List<BaseObjectContent>());

            List<BaseObjectContent> tmp = GetContentWithStorageClass(contentClass);

            if ((tmp != null) && (tmp.Count() != 0))
                children.AddRange(tmp);

            if (ChildCount() != 0)
            {
                foreach (BaseObjectNode childNode in Children)
                    childNode.CollectContent(contentClass, children);
            }

            return children;
        }

        public override string MediaTildeUrl
        {
            get
            {
                BaseObjectNode parent = Parent;
                string parentMediaTildeUrl;
                if (parent != null)
                    parentMediaTildeUrl = parent.MediaTildeUrl;
                else
                    parentMediaTildeUrl = LocalTree.MediaTildeUrl;
                return parentMediaTildeUrl + "/" + Directory;
            }
        }

        /* Let's not combine directory names.
        public override string ComposeDirectory()
        {
            string directory = MediaUtilities.FileFriendlyName(GetTitleString());

            if (Parent != null)
                directory = MediaUtilities.FileFriendlyName(Parent.Directory + "_" + directory);

            return directory;
        }
        */

        public bool CopyNodes(BaseObjectNode sourceNodeOrTree, Dictionary<int, bool> sourceNodeSelectFlags,
            Dictionary<int, bool> targetNodeSelectFlags, CopyPasteType copyMode, NodeUtilities nodeUtilities)
        {
            bool returnValue = true;

            if (sourceNodeOrTree == null)
                return false;

            BaseObjectNodeTree targetTree = LocalTree;
            BaseObjectNode selectedTargetNode = null;
            BaseObjectNode parentTargetNode = null;
            int selectedTargetNodeIndex = -1;

            switch (copyMode)
            {
                case CopyPasteType.Before:
                    selectedTargetNode = GetFirstSelectedNode(targetNodeSelectFlags);
                    if (selectedTargetNode != null)
                        parentTargetNode = selectedTargetNode.Parent;
                    if (parentTargetNode == null)
                        parentTargetNode = this;
                    if (selectedTargetNode != null)
                        selectedTargetNodeIndex = parentTargetNode.GetChildIndex(selectedTargetNode);
                    break;
                case CopyPasteType.Replace:
                    selectedTargetNode = GetFirstSelectedNode(targetNodeSelectFlags);
                    if (selectedTargetNode != null)
                        parentTargetNode = selectedTargetNode.Parent;
                    if (parentTargetNode == null)
                        parentTargetNode = this;
                    if (selectedTargetNode != null)
                        selectedTargetNodeIndex = parentTargetNode.GetChildIndex(selectedTargetNode);
                    if (!nodeUtilities.DeleteSelectedNodes(this, targetNodeSelectFlags))
                        returnValue = false;
                    break;
                case CopyPasteType.After:
                    selectedTargetNode = GetLastSelectedNode(targetNodeSelectFlags);
                    if (selectedTargetNode != null)
                        parentTargetNode = selectedTargetNode.Parent;
                    if (parentTargetNode == null)
                        parentTargetNode = this;
                    if (selectedTargetNode != null)
                        selectedTargetNodeIndex = parentTargetNode.GetChildIndex(selectedTargetNode);
                    if (selectedTargetNodeIndex != -1)
                        selectedTargetNodeIndex++;
                    break;
                case CopyPasteType.Under:
                    selectedTargetNode = GetFirstSelectedNode(targetNodeSelectFlags);
                    if (selectedTargetNode != null)
                        parentTargetNode = selectedTargetNode;
                    if (parentTargetNode == null)
                        parentTargetNode = this;
                    selectedTargetNodeIndex = parentTargetNode.ChildCount();
                    break;
                case CopyPasteType.Prepend:
                    parentTargetNode = this;
                    selectedTargetNodeIndex = 0;
                    break;
                case CopyPasteType.All:
                    parentTargetNode = this;
                    selectedTargetNodeIndex = 0;
                    if (!nodeUtilities.DeleteNodeChildrenAndContentHelper(targetTree, this, true))
                        returnValue = false;
                    if (targetNodeSelectFlags != null)
                        targetNodeSelectFlags.Clear();
                    break;
                case CopyPasteType.Append:
                default:
                    parentTargetNode = this;
                    selectedTargetNodeIndex = ChildCount();
                    break;
            }

            if (targetNodeSelectFlags != null)
                nodeUtilities.InitializeNodeSelectFlags(this, targetNodeSelectFlags, false);

            int dmy;

            if (!parentTargetNode.InsertSelectedNodes(selectedTargetNodeIndex, sourceNodeOrTree, sourceNodeSelectFlags,
                    nodeUtilities, targetNodeSelectFlags, out dmy))
                returnValue = false;

            return returnValue;
        }

        protected bool InsertSelectedNodes(int targetNodeIndex, BaseObjectNode sourceNodeOrTree,
            Dictionary<int, bool> sourceNodeSelectFlags, NodeUtilities nodeUtilities,
            Dictionary<int, bool> targetNodeSelectFlags, out int nextTargetNodeIndex)
        {
            bool returnValue = true;

            nextTargetNodeIndex = targetNodeIndex;

            if (sourceNodeOrTree.ChildCount() == 0)
                return true;

            if (targetNodeIndex == -1)
                targetNodeIndex = ChildCount();

            foreach (int sourceChildKey in sourceNodeOrTree.ChildrenIntKeys)
            {
                BaseObjectNode sourceChildNode = sourceNodeOrTree.GetChild(sourceChildKey);
                bool useIt = false;

                if (sourceChildNode == null)
                    continue;

                if (sourceNodeSelectFlags == null)
                    useIt = true;
                else
                    sourceNodeSelectFlags.TryGetValue(sourceChildKey, out useIt);

                if (useIt)
                {
                    BaseObjectNode targetChildNode = sourceChildNode.CloneNodeShallow();

                    InsertChildNode(targetNodeIndex, targetChildNode);

                    targetChildNode.SetupDirectory();
                    nodeUtilities.EnsureUniqueNode(LocalTree, targetChildNode);

                    if (!targetChildNode.CopyNodeContent(sourceChildNode, null, null, CopyPasteType.All, nodeUtilities))
                        returnValue = false;

                    if (targetNodeSelectFlags != null)
                        targetNodeSelectFlags.Add(targetChildNode.KeyInt, true);

                    int dmy;

                    if (!targetChildNode.InsertSelectedNodes(0, sourceChildNode, sourceNodeSelectFlags,
                            nodeUtilities, targetNodeSelectFlags, out dmy))
                        returnValue = false;

                    targetNodeIndex++;
                }
                else if (!InsertSelectedNodes(targetNodeIndex, sourceChildNode, sourceNodeSelectFlags, nodeUtilities,
                        targetNodeSelectFlags, out targetNodeIndex))
                    returnValue = false;
            }

            nextTargetNodeIndex = targetNodeIndex;

            return returnValue;
        }

        public BaseObjectNode GetFirstNode()
        {
            if ((_ChildrenKeys == null) || (_ChildrenKeys.Count() == 0))
                return null;

            return GetChildIndexed(0);
        }

        public BaseObjectNode GetLastNode()
        {
            if ((_ChildrenKeys == null) || (_ChildrenKeys.Count() == 0))
                return null;

            return GetChildIndexed(_ChildrenKeys.Count() - 1);
        }

        public BaseObjectNode GetFirstSelectedNode(Dictionary<int, bool> targetNodeSelectFlags)
        {
            if ((_ChildrenKeys == null) || (_ChildrenKeys.Count() == 0))
                return null;

            int index;
            int count = _ChildrenKeys.Count();

            for (index = 0; index < count; index++)
            {
                BaseObjectNode childNode = GetChildIndexed(index);

                if (childNode == null)
                    continue;

                if (targetNodeSelectFlags == null)
                    return childNode;

                bool useIt;

                if (targetNodeSelectFlags.TryGetValue(childNode.KeyInt, out useIt))
                {
                    if (useIt)
                        return childNode;
                }
            }

            return null;
        }

        public BaseObjectNode GetLastSelectedNode(Dictionary<int, bool> targetNodeSelectFlags)
        {
            if ((_ChildrenKeys == null) || (_ChildrenKeys.Count() == 0))
                return null;

            int index;
            int count = _ChildrenKeys.Count();

            for (index = count - 1; index >= 0; index--)
            {
                BaseObjectNode childNode = GetChildIndexed(index);

                if (childNode == null)
                    continue;

                if (targetNodeSelectFlags == null)
                    return childNode;

                bool useIt;

                if (targetNodeSelectFlags.TryGetValue(childNode.KeyInt, out useIt))
                {
                    if (useIt)
                        return childNode;
                }
            }

            return null;
        }

        public bool CopyNodeContent(BaseObjectNode sourceNodeOrTree, Dictionary<string, bool> sourceContentSelectFlags,
            Dictionary<string, bool> targetContentSelectFlags, CopyPasteType copyMode, NodeUtilities nodeUtilities)
        {
            bool returnValue = true;

            if (sourceNodeOrTree == null)
                return false;

            BaseObjectContent selectedTargetContent = null;
            BaseContentContainer parentTargetContent = null;
            int selectedTargetContentIndex = -1;

            switch (copyMode)
            {
                case CopyPasteType.Before:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Replace:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (!nodeUtilities.DeleteNodeSelectedContentHelperNoUpdate(LocalTree, this, targetContentSelectFlags))
                        return false;
                    break;
                case CopyPasteType.After:
                    selectedTargetContent = GetLastSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (selectedTargetContentIndex != -1)
                        selectedTargetContentIndex++;
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Under:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    selectedTargetContentIndex = parentTargetContent.ContentChildrenCount();
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Prepend:
                    parentTargetContent = this;
                    selectedTargetContentIndex = 0;
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.All:
                    parentTargetContent = this;
                    selectedTargetContentIndex = 0;
                    if (!nodeUtilities.DeleteNodeAllContentHelperNoUpdate(LocalTree, this))
                        return false;
                    if (targetContentSelectFlags != null)
                        targetContentSelectFlags.Clear();
                    break;
                case CopyPasteType.Append:
                default:
                    parentTargetContent = this;
                    selectedTargetContentIndex = ContentChildrenCount();
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
            }

            if (!parentTargetContent.InsertSelectedContents(selectedTargetContentIndex, this,
                    sourceNodeOrTree, sourceContentSelectFlags, nodeUtilities, targetContentSelectFlags))
                returnValue = false;

            return returnValue;
        }

        public bool CopyContent(BaseObjectContent sourceContent, Dictionary<string, bool> sourceContentSelectFlags,
            Dictionary<string, bool> targetContentSelectFlags, CopyPasteType copyMode, NodeUtilities nodeUtilities)
        {
            bool returnValue = true;

            if (sourceContent == null)
                return false;

            BaseObjectContent selectedTargetContent = null;
            BaseContentContainer parentTargetContent = null;
            int selectedTargetContentIndex = -1;
            BaseObjectNode node = this;
            BaseObjectNodeTree localTree = node.Tree;

            if (localTree == null)
                localTree = node as BaseObjectNodeTree;

            switch (copyMode)
            {
                case CopyPasteType.Before:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Replace:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (!nodeUtilities.DeleteNodeSelectedContentHelperNoUpdate(localTree, node, targetContentSelectFlags))
                        return false;
                    break;
                case CopyPasteType.After:
                    selectedTargetContent = GetLastSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent.ParentContainer;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    if (selectedTargetContent != null)
                        selectedTargetContentIndex = parentTargetContent.GetChildContentIndex(selectedTargetContent);
                    if (selectedTargetContentIndex != -1)
                        selectedTargetContentIndex++;
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Under:
                    selectedTargetContent = GetFirstSelectedContent(targetContentSelectFlags);
                    if (selectedTargetContent != null)
                        parentTargetContent = selectedTargetContent;
                    if (parentTargetContent == null)
                        parentTargetContent = this;
                    selectedTargetContentIndex = parentTargetContent.ContentChildrenCount();
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.Prepend:
                    parentTargetContent = this;
                    selectedTargetContentIndex = 0;
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
                case CopyPasteType.All:
                    parentTargetContent = this;
                    selectedTargetContentIndex = 0;
                    if (!nodeUtilities.DeleteNodeAllContentHelperNoUpdate(localTree, this))
                        return false;
                    if (targetContentSelectFlags != null)
                        targetContentSelectFlags.Clear();
                    break;
                case CopyPasteType.Append:
                default:
                    parentTargetContent = this;
                    selectedTargetContentIndex = ContentChildrenCount();
                    if (targetContentSelectFlags != null)
                        nodeUtilities.InitializeContentSelectFlags(this, targetContentSelectFlags, false);
                    break;
            }

            if (!parentTargetContent.InsertSelectedContents(selectedTargetContentIndex, node,
                    sourceContent, sourceContentSelectFlags, nodeUtilities, targetContentSelectFlags))
                returnValue = false;

            return returnValue;
        }

        public BaseObjectContent GetNextSiblingContent(BaseObjectContent content)
        {
            if ((content == null) || (_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;
            BaseObjectContent sibling = null;
            int index;
            string siblingKey;
            if (content.HasContentParent())
            {
                BaseObjectContent contentParent = content.ContentParent;
                index = contentParent.ContentChildrenKeys.IndexOf(content.KeyString);
                if (index < contentParent.ContentChildrenCount() - 1)
                {
                    index++;
                    siblingKey = contentParent.ContentChildrenKeys[index];
                    sibling = GetContent(siblingKey);
                }
            }
            else
            {
                index = _ContentChildrenKeys.IndexOf(content.KeyString);
                if (index < _ContentChildrenKeys.Count() - 1)
                {
                    index++;
                    siblingKey = _ContentChildrenKeys[index];
                    sibling = GetContent(siblingKey);
                }
            }
            return sibling;
        }

        public BaseObjectContent GetPreviousSiblingContent(BaseObjectContent content)
        {
            if ((content == null) || (_ContentChildrenKeys == null) || (_ContentChildrenKeys.Count() == 0))
                return null;
            BaseObjectContent sibling = null;
            int index;
            string siblingKey;
            if (content.HasContentParent())
            {
                BaseObjectContent contentParent = content.ContentParent;
                index = contentParent.ContentChildrenKeys.IndexOf(content.KeyString);
                if (index > 0)
                {
                    index--;
                    siblingKey = contentParent.ContentChildrenKeys[index];
                    sibling = GetContent(siblingKey);
                }
            }
            else
            {
                index = _ContentChildrenKeys.IndexOf(content.KeyString);
                if (index > 0)
                {
                    index--;
                    siblingKey = _ContentChildrenKeys[index];
                    sibling = GetContent(siblingKey);
                }
            }
            return sibling;
        }

        public List<string> GetTextContentKeys()
        {
            List<BaseObjectContent> contentList = ContentList;
            List<string> contentKeys = new List<string>();

            if (contentList == null)
                return contentKeys;

            foreach (BaseObjectContent content in contentList)
            {
                if (content.ContentClass == ContentClassType.StudyList)
                    contentKeys.Add(content.KeyString);
            }

            return contentKeys;
        }

        public List<string> GetTextContentAndDefaultKeys()
        {
            List<BaseObjectContent> contentList = ContentList;
            List<string> contentKeys = new List<string>();

            if (contentList == null)
                return contentKeys;

            foreach (BaseObjectContent content in contentList)
            {
                if (content.ContentClass == ContentClassType.StudyList)
                    contentKeys.Add(content.KeyString);
            }

            foreach (string str in BaseObjectContent.CommonTextKeys)
            {
                if (contentKeys.Contains(str))
                    continue;

                contentKeys.Add(str);
            }

            return contentKeys;
        }

        public static List<OptionDescriptor> DefaultDescriptors;

        public static List<OptionDescriptor> GetDefaultDescriptors()
        {
            List<OptionDescriptor> newOptionDescriptors = new List<OptionDescriptor>()
                {
                    new OptionDescriptor("DisableStatistics", "flag", "Disable statistics",
                        "This option indicates that this node should not calculate statistics if true.", "false"),
                    new OptionDescriptor("HideStatisticsFromParent", "flag", "Hide statistics from parent",
                        "This option indicates that this node should hide statistics from parents if true.", "false"),
                    BaseContentStorage.CreateEditPermissionOptionDescriptor("Inherit")
                };
            return newOptionDescriptors;
        }

        public override List<OptionDescriptor> OptionDescriptors
        {
            get
            {
                if (DefaultDescriptors == null)
                    DefaultDescriptors = GetDefaultDescriptors();
                return DefaultDescriptors;
            }
        }

        public override string GetInheritedOptionValue(string optionKey)
        {
            string optionValue = GetOptionString(optionKey);
            if (!String.IsNullOrEmpty(optionValue))
            {
                if (optionValue != "Inherited")
                    return optionValue;
            }
            BaseObjectNode parent = Parent;
            if (parent != null)
                return parent.GetInheritedOptionValue(optionKey);
            else if (Tree != null)
                return Tree.GetInheritedOptionValue(optionKey);
            return null;
        }

        public override bool FindContainerAndOptionFlag(string optionKey, out BaseMarkupContainer container, out bool flag)
        {
            bool returnValue = base.FindContainerAndOptionFlag(optionKey, out container, out flag);

            if (returnValue)
                return returnValue;

            BaseObjectNode parent = Parent;
            if (parent != null)
                returnValue = parent.FindContainerAndOptionFlag(optionKey, out container, out flag);
            else if (Tree != null)
                returnValue = Tree.FindContainerAndOptionFlag(optionKey, out container, out flag);

            return returnValue;
        }

        public override bool Modified
        {
            get
            {
                if (base.Modified)
                    return true;

                if ((_MasterReference != null) && _MasterReference.Modified)
                    return true;

                if (_ContentList != null)
                {
                    foreach (BaseObjectContent content in _ContentList)
                    {
                        if (content.Modified)
                            return true;
                    }
                }

                return false;
            }
            set
            {
                if (_MasterReference != null)
                    _MasterReference.Modified = false;

                if (_ContentList != null)
                {
                    foreach (BaseObjectContent content in _ContentList)
                        content.Modified = false;
                }
            }
        }

        public void ResolveMasterReference(IMainRepository mainRepository)
        {
            if (MasterReference != null)
                MasterReference.ResolveReference(mainRepository);
        }

        public override void ResolveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            if (_MasterReference != null)
            {
                _MasterReference.ResolveReference(mainRepository);

                if (Master != null)
                    Master.ResolveReferences(mainRepository, false, false);
            }

            if (ContentList != null)
            {
                foreach (BaseObjectContent content in ContentList)
                    content.ResolveReferences(mainRepository, false, false);
            }

            if (recurseChildren && HasChildren())
            {
                List<BaseObjectNode> childNodes = Children;

                foreach (BaseObjectNode childNode in childNodes)
                    childNode.ResolveReferences(mainRepository, false, true);
            }

            base.ResolveReferences(mainRepository, recurseParents, recurseChildren);
        }

        public void ResolveDescendentReferences(IMainRepository mainRepository)
        {
            List<BaseObjectNode> childNodes = Children;

            if (childNodes != null)
            {
                foreach (BaseObjectNode childNode in childNodes)
                {
                    childNode.ResolveReferences(mainRepository, false, false);
                    childNode.ResolveDescendentReferences(mainRepository);
                }
            }
        }

        public override bool SaveReferences(IMainRepository mainRepository, bool recurseParents, bool recurseChildren)
        {
            bool returnValue = true;

            if (ContentList != null)
            {
                foreach (BaseObjectContent content in ContentList)
                {
                    if (!content.SaveReferences(mainRepository, false, false))
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

            if (ContentList != null)
            {
                foreach (BaseObjectContent content in ContentList)
                {
                    if (!content.UpdateReferences(mainRepository, false, false))
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

            if (ContentList != null)
            {
                foreach (BaseObjectContent content in ContentList)
                {
                    if (!content.UpdateReferencesCheck(mainRepository, false, false))
                        returnValue = false;
                }
            }

            if (!base.UpdateReferencesCheck(mainRepository, recurseParents, recurseChildren))
                returnValue = false;

            return returnValue;
        }

        public override void ClearReferences(bool recurseParents, bool recurseChildren)
        {
            if (ContentList != null)
            {
                foreach (BaseObjectContent content in ContentList)
                    content.ClearReferences(false, false);
            }

            base.ClearReferences(recurseParents, recurseChildren);
        }

        public override void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction)
        {
            bool useThis = true;

            if ((nodeSelectFlags != null) && (nodeSelectFlags.Count != 0))
            {
                if (!nodeSelectFlags.TryGetValue(KeyInt, out useThis))
                    useThis = false;
            }

            if (useThis)
            {
                if (Master != null)
                {
                    Master.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                        mediaFiles, visitFunction);
                    AddUniqueReference(references, Master);
                }
            }

            List<BaseObjectNode> childNodes = Children;

            if (childNodes != null)
            {
                foreach (BaseObjectNode childNode in childNodes)
                {
                    bool useIt = true;

                    if ((nodeSelectFlags != null) && nodeSelectFlags.TryGetValue(childNode.KeyInt, out useIt) && !useIt)
                        continue;

                    if (useIt && (externalNonSavedChildren != null))
                        externalNonSavedChildren.Add(childNode);

                    childNode.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                        mediaFiles, visitFunction);
                }
            }

            base.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                        mediaFiles, visitFunction);

            /* Done in inherited class.
            List<BaseObjectContent> contentList = ContentChildren;

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                    content.CollectReferences(references, externalSavedChildren, externalNonSavedChildren,
                        nodeSelectFlags, contentSelectFlags, itemSelectFlags, languageSelectFlags,
                        mediaFiles, visitFunction);
            }
            */
        }

        public void CollectDescendents(List<BaseObjectNode> descendents)
        {
            if (HasChildren())
            {
                foreach (BaseObjectNode childNode in Children)
                {
                    descendents.Add(childNode);
                    childNode.CollectDescendents(descendents);
                }
            }
        }

        public void CollectDescendentKeys(List<int> descendentKeys)
        {
            if (HasChildren())
            {
                foreach (BaseObjectNode childNode in Children)
                {
                    descendentKeys.Add(childNode.KeyInt);
                    childNode.CollectDescendentKeys(descendentKeys);
                }
            }
        }

        public void CollectContentKeys(List<string> contentKeys)
        {
            if (HasChildren())
            {
                foreach (BaseObjectNode childNode in Children)
                    childNode.CollectContentKeys(contentKeys);
            }

            List<BaseObjectContent> contentList = ContentList;

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    string contentKey = content.KeyString;

                    if (!contentKeys.Contains(contentKey))
                        contentKeys.Add(content.KeyString);
                }
            }
        }

        public void CollectStudyListAndDocumentContentKeys(List<string> contentKeys)
        {
            if (HasChildren())
            {
                foreach (BaseObjectNode childNode in Children)
                    childNode.CollectStudyListAndDocumentContentKeys(contentKeys);
            }

            List<BaseObjectContent> contentList = ContentList;

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if ((content.ContentClass != ContentClassType.StudyList) && (content.ContentClass != ContentClassType.DocumentItem))
                        continue;

                    string contentKey = content.KeyString;

                    if (!contentKeys.Contains(contentKey))
                        contentKeys.Add(content.KeyString);
                }
            }
        }

        public void CollectMediaContentKeys(List<string> contentKeys)
        {
            if (HasChildren())
            {
                foreach (BaseObjectNode childNode in Children)
                    childNode.CollectMediaContentKeys(contentKeys);
            }

            List<BaseObjectContent> contentList = ContentList;

            if (contentList != null)
            {
                foreach (BaseObjectContent content in contentList)
                {
                    if (content.ContentClass != ContentClassType.MediaItem)
                        continue;

                    string contentKey = content.KeyString;

                    if (!contentKeys.Contains(contentKey))
                        contentKeys.Add(content.KeyString);
                }
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (_MasterReference != null)
                element.Add(_MasterReference.GetElement("MasterReference"));

            if (_ParentKey > 0)
                element.Add(new XElement("ParentKey", _ParentKey.ToString()));

            if ((_ChildrenKeys != null) && (_ChildrenKeys.Count() != 0))
                element.Add(ObjectUtilities.GetElementFromIntList("ChildrenKeys", _ChildrenKeys));

            if ((_ContentList != null) && (_ContentList.Count() != 0))
            {
                foreach (BaseObjectContent content in _ContentList)
                    element.Add(content.GetElement("Content"));
            }

            return element;
        }

        public override XElement GetElementFiltered(string name, Dictionary<int, bool> childNodeFlags,
            Dictionary<string, bool> childContentFlags)
        {
            XElement element = base.GetElementFiltered(name, childNodeFlags, childContentFlags);

            if (_MasterReference != null)
                element.Add(_MasterReference.GetElement("MasterReference"));

            if (_ParentKey > 0)
            {
                bool useIt = true;
                BaseObjectNode parentNode = Parent;

                while (parentNode != null)
                {
                    if (childNodeFlags != null)
                        childNodeFlags.TryGetValue(parentNode.KeyInt, out useIt);

                    if (useIt)
                    {
                        element.Add(new XElement("ParentKey", parentNode.KeyString));
                        break;
                    }

                    parentNode = parentNode.Parent;
                }
            }

            if ((_ChildrenKeys != null) && (_ChildrenKeys.Count() != 0))
            {
                List<int> childrenKeys = new List<int>();
                LoadChildrenKeys(childrenKeys, childNodeFlags);
                element.Add(ObjectUtilities.GetElementFromIntList("ChildrenKeys", childrenKeys));
            }

            if ((_ContentList != null) && (_ContentList.Count() != 0))
            {
                foreach (BaseObjectContent content in _ContentList)
                {
                    bool useIt = true;

                    if ((childContentFlags != null) && childContentFlags.TryGetValue(content.KeyString, out useIt) && !useIt)
                        continue;

                    element.Add(content.GetElementFiltered("Content", null, childContentFlags));
                }
            }

            return element;
        }

        public void LoadChildrenKeys(List<int> childrenKeys, Dictionary<int, bool> childNodeFlags)
        {
            List<BaseObjectNode> childNodes = Children;

            if (childNodes == null)
                return;

            foreach (BaseObjectNode childNode in childNodes)
            {
                bool useIt = true;
                int childKey = childNode.KeyInt;

                if (childNodeFlags != null)
                    childNodeFlags.TryGetValue(childKey, out useIt);

                if (useIt)
                    childrenKeys.Add(childKey);
                else
                    childNode.LoadChildrenKeys(childrenKeys, childNodeFlags);
            }
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "MasterReference":
                    _MasterReference = new NodeMasterReference(childElement);
                    break;
                case "ParentKey":
                    _ParentKey = ObjectUtilities.GetIntegerFromString(childElement.Value, 0);
                    break;
                case "ChildrenKeys":
                    _ChildrenKeys = ObjectUtilities.GetIntListFromString(childElement.Value);
                    // Hack to remove infinite recursion.
                    if (!(this is BaseObjectNodeTree))
                    {
                        for (int i = _ChildrenKeys.Count() - 1; i >= 0; i--)
                        {
                            if (_ChildrenKeys[i] == KeyInt)
                                _ChildrenKeys.RemoveAt(i);
                        }
                    }
                    break;
                case "Content":
                    BaseObjectContent content = new BaseObjectContent(childElement);
                    content.Node = this;
                    if (_ContentList == null)
                        _ContentList = new List<BaseObjectContent>(1) { content };
                    else
                        _ContentList.Add(content);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public override int Compare(IBaseObjectKeyed other)
        {
            BaseObjectNode otherBaseObjectNode = other as BaseObjectNode;

            if (otherBaseObjectNode == null)
                return base.Compare(other);

            int diff = base.Compare(other);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareInts(_ParentKey, otherBaseObjectNode.ParentKeyInt);

            if (diff != 0)
                return diff;

            diff = ObjectUtilities.CompareIntLists(_ChildrenKeys, otherBaseObjectNode.ChildrenIntKeys);

            return diff;
        }

        public static int Compare(BaseObjectNode item1, BaseObjectNode item2)
        {
            if (((object)item1 == null) && ((object)item2 == null))
                return 0;
            if ((object)item1 == null)
                return -1;
            if ((object)item2 == null)
                return 1;
            int diff = item1.Compare(item2);
            return diff;
        }

        public static int CompareKeys(BaseObjectNode object1, BaseObjectNode object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return ObjectUtilities.CompareKeys(object1, object2);
        }

        public static int CompareNodeLists(List<BaseObjectNode> object1, List<BaseObjectNode> object2)
        {
            return ObjectUtilities.CompareTypedObjectLists<BaseObjectNode>(object1, object2);
        }

        public override void OnFixup(FixupDictionary fixups)
        {
            base.OnFixup(fixups);

            if (_MasterReference != null)
                _MasterReference.OnFixup(fixups);

            if (_MarkupReference != null)
                _MarkupReference.OnFixup(fixups);

            if (HasParent())
            {
                string xmlKey = ParentKey.ToString();
                string source = Source;

                if (String.IsNullOrEmpty(source))
                    source = "Nodes";

                IBaseObjectKeyed keyedObject = fixups.Get(source, xmlKey);

                if (keyedObject != null)
                {
                    if (keyedObject is BaseObjectNode)
                    {
                        // Fixup parent key.
                        ParentKey = keyedObject.KeyInt;
                    }
                    else
                        throw new Exception("BaseObjectNode.OnFixup: child node not a node.");
                }
                // else it might be a target, in which case let controller reset the parent.
            }

            if (HasChildren())
            {
                int count = ChildCount();
                int index;

                for (index = 0; index < count; index++)
                {
                    int childKey = ChildrenIntKeys[index];
                    string xmlKey = childKey.ToString();
                    IBaseObjectKeyed keyedObject = fixups.Get("Nodes", xmlKey);

                    if (keyedObject != null)
                    {
                        if (keyedObject is BaseObjectNode)
                        {
                            BaseObjectNode childNode = keyedObject as BaseObjectNode;

                            if (LocalTree != null)
                            {
                                if (HasChild(childKey))
                                {
                                    // Fixup child key.
                                    int childIndex = _ChildrenKeys.IndexOf(childKey);
                                    int intKey = childNode.KeyInt;
                                    if (_ChildrenKeys[childIndex] != intKey)
                                    {
                                        _ChildrenKeys[childIndex] = intKey;
                                        ModifiedFlag = true;
                                    }
                                }
                            }
                            else
                                throw new Exception("BaseObjectNode.OnFixup: No local tree set.");
                        }
                        else
                            throw new Exception("BaseObjectNode.OnFixup: child node not a node.");
                    }
                }
            }
        }
    }
}
