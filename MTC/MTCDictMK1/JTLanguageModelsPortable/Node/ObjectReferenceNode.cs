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
    public class ObjectReferenceNode : ObjectReference<BaseObjectNode>
    {
        public ObjectReferenceNode(object key, string source, BaseObjectNode item)
            : base(key, source, item)
        {
        }

        public ObjectReferenceNode(BaseObjectNode item)
            : base((item != null ? item.Key : null), (item != null ? item.Source : null), item)
        {
        }

        public ObjectReferenceNode(ObjectReferenceNode other)
            : base(other)
        {
        }

        public ObjectReferenceNode(XElement element)
        {
            OnElement(element);
        }

        public ObjectReferenceNode()
        {
            ClearObjectReference();
        }
    }
}
