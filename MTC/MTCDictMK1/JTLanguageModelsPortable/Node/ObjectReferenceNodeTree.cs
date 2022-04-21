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
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Node
{
    public class ObjectReferenceNodeTree : ObjectReferenceTitled
    {
        protected LevelCode _Level;

        public ObjectReferenceNodeTree(object key, MultiLanguageString title, MultiLanguageString description,
                string source, string package, string label, string imageFileName, int index, bool isPublic,
                List<LanguageID> targetLanguageIDs, List<LanguageID> hostLanguageIDs, string owner)
            : base(key, title, description, source, package, label, imageFileName, index, isPublic,
                targetLanguageIDs, hostLanguageIDs, owner)
        {
            _Level = LevelCode.None;
        }

        public ObjectReferenceNodeTree(object key, string source)
            : base(key, source)
        {
            _Level = LevelCode.None;
        }

        public ObjectReferenceNodeTree(string source, BaseObjectNodeTree item)
            : base(source, item)
        {
            if (item != null)
                _Level = item.Level;
            else
                _Level = LevelCode.None;
        }

        public ObjectReferenceNodeTree(string source, object key, BaseObjectNodeTree item)
            : base(source, key, item)
        {
            if (item != null)
                _Level = item.Level;
            else
                _Level = LevelCode.None;
        }

        public ObjectReferenceNodeTree(ObjectReferenceNodeTree other, object key, string source)
            : base(other, key, source)
        {
            CopyReference(other);
            ModifiedFlag = false;
        }

        public ObjectReferenceNodeTree(ObjectReferenceNodeTree other)
            : base(other, other.Key, other.Source)
        {
            CopyReference(other);
            ModifiedFlag = false;
        }

        public ObjectReferenceNodeTree(XElement element)
        {
            OnElement(element);
        }

        public ObjectReferenceNodeTree()
        {
            ClearObjectReferenceNodeTree();
        }

        public override void Clear()
        {
            base.Clear();
            ClearObjectReferenceNodeTree();
        }

        public void ClearObjectReferenceNodeTree()
        {
            _Level = LevelCode.None;
        }

        public void CopyObjectReferenceNodeTree(ObjectReferenceNodeTree other)
        {
            base.CopyObjectReferenceTitled(other);
            _Level = other.Level;
        }

        public void CopyReference(ObjectReferenceNodeTree other)
        {
            base.CopyReference(other);
            _Level = other.Level;
        }

        public void UpdateReference(BaseObjectNodeTree other)
        {
            base.UpdateReference(other);
            _Level = other.Level;
            _Owner = other.Owner;
        }

        public void Copy(ObjectReferenceNodeTree other)
        {
            base.Copy(other);
            _Level = other.Level;
        }

        public void CopyDeep(ObjectReferenceNodeTree other)
        {
            base.CopyDeep(other);
            _Level = other.Level;
        }

        public override IBaseObject Clone()
        {
            return new ObjectReferenceNodeTree(this);
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
                return BaseObjectNodeTree.GetStringFromLevel(_Level);
            }
            set
            {
                LevelCode level = BaseObjectNodeTree.GetLevelFromString(value);
                Level = level;
            }
        }

        public BaseObjectNodeTree Tree
        {
            get
            {
                return _Item as BaseObjectNodeTree;
            }
            set
            {
                _Item = value;
            }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            element.Add(new XAttribute("Level", LevelString));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Level":
                    _Level = BaseObjectNodeTree.GetLevelFromString(attributeValue);
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }
    }
}
