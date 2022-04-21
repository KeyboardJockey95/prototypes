using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Node;

namespace JTLanguageModelsPortable.Crawlers
{
    public class SiteNode : BaseObjectKeyed
    {
        protected string _NodeType;
        protected string _NodeUrl;
        protected string _NodePath;
        protected IBaseObjectKeyed _Item;
        protected List<SiteNode> _Children;
        protected SiteNode _Parent;

        public SiteNode(string name, string nodeType, string nodeUrl, string nodePath, IBaseObjectKeyed item) : base(name)
        {
            _NodeType = nodeType;
            _NodeUrl = nodeUrl;
            _NodePath = nodePath;
            _Item = item;
            _Children = new List<SiteNode>();
        }

        public SiteNode(SiteNode other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public SiteNode(XElement element)
        {
            OnElement(element);
        }

        public SiteNode()
        {
            ClearSiteNode();
        }

        public override void Clear()
        {
            base.Clear();
            ClearSiteNode();
        }

        public void ClearSiteNode()
        {
            _NodeType = null;
            _NodeUrl = null;
            _NodePath = null;
            _Item = null;
            _Children = new List<SiteNode>();
        }

        public void Copy(SiteNode other)
        {
            if (other == null)
            {
                _NodeType = null;
                _NodeUrl = null;
                _NodePath = null;
                _Item = null;
                _Children = new List<SiteNode>();
                return;
            }

            _NodeType = other.NodeType;
            _NodeUrl = other.NodeUrl;
            _NodePath = other.NodePath;
            if (other.Item != null)
                _Item = (IBaseObjectKeyed)other.Item.Clone();
            else
                _Item = null;
            _Children = new List<SiteNode>();
            foreach (SiteNode child in other.Children)
                _Children.Add((SiteNode)child.Clone());
            ModifiedFlag = true;
        }

        public override IBaseObject Clone()
        {
            return new SiteNode(this);
        }

        public override string Name
        {
            get
            {
                return KeyString;
            }
            set
            {
                Key = value;
            }
        }

        public string NodeType
        {
            get
            {
                return _NodeType;
            }
            set
            {
                if (value != _NodeType)
                {
                    _NodeType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string NodeUrl
        {
            get
            {
                return _NodeUrl;
            }
            set
            {
                if (value != _NodeUrl)
                {
                    _NodeUrl = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string NodePath
        {
            get
            {
                return _NodePath;
            }
            set
            {
                if (value != _NodePath)
                {
                    _NodePath = value;
                    ModifiedFlag = true;
                }
            }
        }

        public IBaseObjectKeyed Item
        {
            get
            {
                return _Item;
            }
            set
            {
                if (value != _Item)
                {
                    _Item = value;
                    ModifiedFlag = true;
                }
            }
        }

        public T GetTypedItem<T>() where T : BaseObjectKeyed
        {
            return _Item as T;
        }

        public List<SiteNode> Children
        {
            get
            {
                return _Children;
            }
            set
            {
                if (value != _Children)
                {
                    _Children = value;
                    ModifiedFlag = true;
                }
            }
        }

        public SiteNode Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
            }
        }

        public SiteNode Root
        {
            get
            {
                SiteNode root = this;
                while (root.Parent != null)
                    root = root.Parent;
                return root;
            }
        }

        public void DisplayInformation(StreamWriter streamWriter, int nestLevel)
        {
            string line = SiteTextTools.Indent(nestLevel, 4) + Name + ", " + NodeType + ", " + NodeUrl + ", " + NodePath;
            streamWriter.WriteLine(line);

            //if (_Item != null)
            //    TextTools.WriteIndentedText(nestLevel + 1, 4, _Item.ToString(), streamWriter);

            if (Children != null)
            {
                foreach (SiteNode child in Children)
                    child.DisplayInformation(streamWriter, nestLevel + 1);
            }
        }

        public SiteNode AddChild(SiteNode child)
        {
            Children.Add(child);
            child.Parent = this;
            ModifiedFlag = true;
            return child;
        }

        public SiteNode AddChild(string name, string nodeType, string nodeUrl, string nodePath, IBaseObjectKeyed item)
        {
            SiteNode child = new SiteNode(name, nodeType, nodeUrl, nodePath, item);
            return AddChild(child);
        }

        public SiteNode AddUniqueChild(SiteNode child)
        {
            SiteNode oldNode = FindChild(child.Name);
            if (oldNode != null)
                DeleteChild(oldNode);
            Children.Add(child);
            child.Parent = this;
            ModifiedFlag = true;
            return child;
        }

        public SiteNode AddUniqueChild(string name, string nodeType, string nodeUrl, string nodePath, IBaseObjectKeyed item)
        {
            SiteNode oldNode = FindChild(name);
            if (oldNode != null)
                DeleteChild(oldNode);
            SiteNode child = new SiteNode(name, nodeType, nodeUrl, nodePath, item);
            return AddUniqueChild(child);
        }

        public SiteNode FindChild(string name)
        {
            SiteNode node = Children.FirstOrDefault(x => x.Name == name);
            return node;
        }

        public bool DeleteChild(SiteNode node)
        {
            return Children.Remove(node);
        }

        public bool MatchPath(string pathPattern)
        {
            char[] seps = { '/' };
            string[] parts = pathPattern.Split(seps);
            List<SiteNode> nodes = new List<SiteNode>();
            SiteNode node = this;
            int count = parts.Count();
            int index;

            while (node.Parent != null)
            {
                switch (node.NodeType)
                {
                    case "WordList":
                    case "Level":
                    case "SubLevel":
                    case "Group":
                    case "Lesson":
                        nodes.Insert(0, node);
                        break;
                    default:
                        break;
                }
                node = node.Parent;
            }

            if (count > nodes.Count())
                count = nodes.Count();

            for (index = 0; index < count; index++)
            {
                string part = parts[index];
                node = nodes[index];

                if (part == "*")
                    continue;
                else if (part == node.Name)
                    continue;
                else if (Regex.IsMatch(node.Name, "^" + part + "$"))
                    continue;

                return false;
            }

            return true;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_NodeType != null)
                element.Add(new XAttribute("NodeType", _NodeType));
            if (_NodeUrl != null)
                element.Add(new XAttribute("NodeUrl", _NodeUrl));
            if (_NodePath != null)
                element.Add(new XAttribute("NodePath", _NodePath));
            if (_Item != null)
                element.Add(_Item.Xml);
            if (_Children != null)
            {
                foreach (SiteNode child in _Children)
                    element.Add(child.Xml);
            }
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "NodeType":
                    NodeType = attributeValue;
                    break;
                case "NodeUrl":
                    NodeUrl = attributeValue;
                    break;
                case "NodePath":
                    NodePath = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public override bool OnChildElement(XElement childElement)
        {
            if (childElement.Name.LocalName != "SiteNode")
                Item = ObjectUtilities.ResurrectBase(childElement);
            else
                AddChild(new SiteNode(childElement));
            return true;
        }
    }
}
