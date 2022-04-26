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
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Master
{
    public class NodeMasterReference : ObjectReferenceGuid<NodeMaster>
    {
        public NodeMasterReference(NodeMaster item)
            : base(
                (item != null ? item.Key : null),
                "NodeMasters",
                ((item != null) ? item.Guid : Guid.Empty),
                item)
        {
        }

        public NodeMasterReference(object key, Guid guid)
            : base(key, "NodeMasters", guid, null)
        {
        }

        public NodeMasterReference(NodeMasterReference other)
            : base(other)
        {
        }

        public NodeMasterReference(XElement element)
            : base(element)
        {
        }

        public NodeMasterReference()
        {
        }

        public override IBaseObject Clone()
        {
            return new NodeMasterReference(this);
        }
    }
}
