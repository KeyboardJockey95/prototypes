using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Forum
{
    public class ForumCategory : BaseObjectTitled
    {
        public ForumCategory(object key, MultiLanguageString title, MultiLanguageString description, int index)
            : base(key)
        {
            Title = title;
            Description = description;
            Index = index;
        }

        public ForumCategory(ForumCategory other)
            : base(other)
        {
        }

        public ForumCategory(XElement element)
        {
            OnElement(element);
        }

        public ForumCategory()
        {
            Clear();
        }

        public override IBaseObject Clone()
        {
            return new ForumCategory(this);
        }

        public static int Compare(ForumCategory object1, ForumCategory object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }
    }
}
