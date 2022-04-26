using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Admin
{
    public class UserID : BaseObjectKeyed
    {
        public UserID(object key)
            : base(key)
        {
        }

        public UserID(XElement element)
        {
            OnElement(element);
        }

        public UserID()
        {
        }
    }
}
