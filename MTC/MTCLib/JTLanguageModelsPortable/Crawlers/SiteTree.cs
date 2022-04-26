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

namespace JTLanguageModelsPortable.Crawlers
{
    public class SiteTree : BaseObjectKeyed
    {
        protected SiteNode _Root;
        public List<SiteNode> Children { get { return _Root.Children; } }

        public SiteTree(string name, SiteNode root) : base(name)
        {
            _Root = root;
        }

        public SiteTree(SiteTree other)
            : base(other)
        {
            Copy(other);
            ModifiedFlag = false;
        }

        public SiteTree(XElement element)
        {
            OnElement(element);
        }

        public SiteTree()
        {
            ClearSiteTree();
        }

        public override void Clear()
        {
            base.Clear();
            ClearSiteTree();
        }

        public void ClearSiteTree()
        {
            _Root = new SiteNode();
        }

        public void Copy(SiteTree other)
        {
            if (other == null)
            {
                _Root = new SiteNode();
                return;
            }

            _Root = (SiteNode)other.Root.Clone();
            ModifiedFlag = true;
        }

        public override IBaseObject Clone()
        {
            return new SiteTree(this);
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

        public SiteNode Root
        {
            get
            {
                return _Root;
            }
            set
            {
                if (value != _Root)
                {
                    _Root = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<SiteNode> GetTreeNodes(string lessonPathPattern)
        {
            if (_Root == null)
                return null;

            char[] seps = { '/' };
            string[] parts = lessonPathPattern.Split(seps);
            List<SiteNode> parentNodes = new List<SiteNode>(1) { _Root };
            List<SiteNode> childNodes = new List<SiteNode>();

            foreach (string part in parts)
            {
                foreach (SiteNode parentNode in parentNodes)
                {
                    if (parentNode.Children == null)
                        continue;

                    if (part == "*")
                        childNodes.AddRange(parentNode.Children);
                    else
                    {
                        foreach (SiteNode childNode in parentNode.Children)
                        {
                            if ((childNode.Name == part) || Regex.IsMatch(childNode.Name, part))
                                childNodes.Add(childNode);
                        }
                    }
                }

                parentNodes = childNodes;
                childNodes = new List<SiteNode>();
            }

            return parentNodes;
        }

        public void DisplayInformation(StreamWriter streamWriter)
        {
            streamWriter.WriteLine(Name + ":");

            if (Root == null)
                return;

            Root.DisplayInformation(streamWriter, 1);
        }

        public SiteNode AddChild(SiteNode child)
        {
            return Root.AddChild(child);
        }

        public SiteNode AddChild(string name, string nodeType, string nodeUrl, string nodePath, IBaseObjectKeyed item)
        {
            return Root.AddChild(name, nodeType, nodeUrl, nodePath, item);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (_Root != null)
                element.Add(_Root.GetElement("Root"));
            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            _Root = new SiteNode(childElement);
            return true;
        }
    }
}
